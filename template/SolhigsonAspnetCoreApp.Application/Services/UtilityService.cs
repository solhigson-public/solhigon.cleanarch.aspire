using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Solhigson.Framework.Dto;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Logging;
using Solhigson.Framework.Notification;
using Solhigson.Framework.Services.Abstractions;
using Solhigson.Framework.Utilities.Extensions;
using SolhigsonAspnetCoreApp.Domain;
using SolhigsonAspnetCoreApp.Domain.CacheModels;
using SolhigsonAspnetCoreApp.Domain.Dto;
using SolhigsonAspnetCoreApp.Domain.Entities;
using SolhigsonAspnetCoreApp.Domain.Interfaces;
using SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions;

namespace SolhigsonAspnetCoreApp.Application.Services;

public class UtilityService : ServiceBase
{
    private static readonly LogWrapper Logger = LogManager.GetLogger(typeof(UtilityService).FullName);
    private readonly INotificationService _notificationService;

    public UtilityService(IRepositoryWrapper repositoryWrapper, INotificationService notificationService) : base(
        repositoryWrapper)
    {
        _notificationService = notificationService;
    }

    public static ResponseInfo ValidateModel(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);
        if (!isValid)
        {
            var msg = "";
            foreach (var vResult in results) msg += $"{vResult.ErrorMessage}, ";
            return ResponseInfo.FailedResult(msg);
        }

        var stringType = typeof(string);
        foreach (var prop in model.GetType().GetProperties()
                     .Where(t => t.PropertyType.IsClass && t.PropertyType != stringType &&
                                 t.GetAttribute<RequiredAttribute>() is not null))
        {
            var val = prop.GetValue(model);
            if (val is null) return ResponseInfo.FailedResult($"{prop.Name} is required");

            var validationResult = ValidateModel(val);
            if (!validationResult.IsSuccessful) return validationResult;
        }

        return ResponseInfo.SuccessResult();
    }

    private void SendMail(EmailNotificationDetail emailNotificationDetail)
    {
        var template = emailNotificationDetail.TemplatePlaceholders
                       ?? new Dictionary<string, string>();

        template.Add("[[TemplateHeader]]", ServicesWrapper.AppSettings.EmailTemplateHeader);

        ServicesWrapper.BackgroundJobClient.Enqueue(() =>
            _notificationService.SendMail(emailNotificationDetail));
    }

    public async ValueTask SendMailAsync(EmailParameter emailParameter)
    {
        var emailNotificationDetail = new EmailNotificationDetail
        {
            TemplateName = emailParameter.Template,
            FromAddress = ServicesWrapper.AppSettings.DefaultFromEmailAddress,
            FromDisplayAddress = emailParameter.FromDisplayAddress ??
                                 ServicesWrapper.AppSettings.DefaultFromEmailDisplayName,
            Subject = emailParameter.Subject,
            TemplatePlaceholders = emailParameter.TemplatePlaceholders,
            Attachments = emailParameter.Attachments
        };

        if (emailParameter.To?.Any() == true)
            foreach (var email in emailParameter.To)
                emailNotificationDetail.AddToAddress(email);

        if (emailParameter.Cc?.Any() == true)
            foreach (var email in emailParameter.Cc)
                emailNotificationDetail.AddCcAddress(email);

        if (emailParameter.Bcc?.Any() == true)
            foreach (var email in emailParameter.Bcc)
                emailNotificationDetail.AddBccAddress(email);

        if (emailParameter.Roles?.Any() == true)
        {
            var usersToNotify = await ServicesWrapper.UserService.GetUsersInRoles(
                emailParameter.Roles.ToArray(),
                emailParameter.InstitutionId);

            foreach (var userToNotify in usersToNotify) emailNotificationDetail.AddToAddress(userToNotify.Email);
        }

        if (!string.IsNullOrWhiteSpace(emailParameter.Permission))
        {
            var usersToNotify = await ServicesWrapper.UserService.GetUsersWithPermission(
                emailParameter.InstitutionId,
                emailParameter.Permission);

            foreach (var userToNotify in usersToNotify) emailNotificationDetail.AddToAddress(userToNotify.Email);
        }

        SendMail(emailNotificationDetail);
    }


    public DateTime DefaultTokenExpiryTime()
    {
        return DateTime.UtcNow.AddHours(ServicesWrapper.AppSettings.UserTokenValidityPeriodHrs);
    }

    public static string TruncateWithLeadingDots(string val, int length)
    {
        if (string.IsNullOrEmpty(val)) return null;

        return val.Length > length ? val[..length] + "..." : val;
    }

    public async Task<InstitutionCacheModel?> GetInstitutionAsync(string institutionId)
    {
        if (institutionId == Constants.DefaultInstitutionId) return Constants.SystemInstitution;

        return await RepositoryWrapper.InstitutionRepository.GetByIdCachedAsync(institutionId);
    }

    public async Task<string?> GetEntityNameForAuditAsync(string id, string tableName)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(tableName)) return string.Empty;

        if (tableName.ToLower() == GetTableName(typeof(Country)))
            return (await RepositoryWrapper.CountryRepository.GetByIdCachedAsync(id))?.Name;

        if (tableName.ToLower() == GetTableName(typeof(Currency)))
            return (await RepositoryWrapper.CurrencyRepository.GetByIdCachedAsync(id))?.Name;

        return string.Empty;
    }

    private static string GetTableName(Type entityType)
    {
        var tableAttribute = entityType.GetAttribute<TableAttribute>();
        var name = tableAttribute?.Name ?? entityType.Name;
        return name.ToLower();
    }

    public async Task<string?> GetInstitutionNameAsync(IInstitutionEntity entity)
    {
        return (await RepositoryWrapper.InstitutionRepository.GetByIdCachedAsync(entity.InstitutionId))?.Name;
    }
}