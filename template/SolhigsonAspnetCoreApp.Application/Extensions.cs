using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Solhigson.Framework.Data;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Identity;
using Solhigson.Framework.Logging;
using Solhigson.Framework.Web.Middleware;
using SolhigsonAspnetCoreApp.Application.Services;
using SolhigsonAspnetCoreApp.Application.Web;
using SolhigsonAspnetCoreApp.Domain;
using SolhigsonAspnetCoreApp.Domain.Dto;
using LogManager = Solhigson.Framework.Logging.LogManager;

namespace SolhigsonAspnetCoreApp.Application;

public static class Extensions
{
    private static readonly LogWrapper Logger = LogManager.GetLogger(typeof(Extensions).FullName);

    public static string? DefaultConnectionString(this IConfiguration configuration)
    {
        return configuration.GetConnectionString("Default");
    }

    public static string? HangfireConnectionString(this IConfiguration configuration)
    {
        return configuration.GetConnectionString("Default");
    }


    internal static string GetContainerOrCollectionName(this IHostEnvironment env, string name)
    {
        return $"{name}-{env.EnvironmentName.ToUpper()}";
    }

    public static bool IsProduction(this ControllerBase controller)
    {
        return controller.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsProduction();
    }

    public static void AddFirstSelectListItem(this IList<SelectListItem> list, string text = "- Select -",
        string value = "")
    {
        list.Insert(0, new SelectListItem { Text = text, Value = value });
    }

    public static bool IsSystemRole(this SolhigsonAspNetRole? role)
    {
        return BelongsToRoleGroup(role, Constants.AppRoles.Groups.System);
    }

    public static bool IsInstitutionRole(this SolhigsonAspNetRole? role)
    {
        return BelongsToRoleGroup(role, Constants.AppRoles.Groups.Institution);
    }

    private static bool BelongsToRoleGroup(SolhigsonAspNetRole? role, string roleGroup)
    {
        return role?.RoleGroup?.Name == roleGroup;
    }

    public static bool IsSystemUser(this SessionUser user)
    {
        return user.Role.IsSystemRole();
    }

    public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, PagedRequestBase request)
        where T : class
    {
        var count = await source.CountAsync();
        var items = await source.AsNoTrackingWithIdentityResolution()
            .Skip((request.PageNumber - 1) * request.ItemsPerPage)
            .Take(request.ItemsPerPage).ToListAsync();
        return PagedList.Create(items, count, request.PageNumber, request.ItemsPerPage);
    }

    public static PagedList<T> ToPagedList<T>(this IQueryable<T> source, PagedRequestBase request) where T : class
    {
        var count = source.Count();
        var items = source.AsNoTrackingWithIdentityResolution().Skip((request.PageNumber - 1) * request.ItemsPerPage)
            .Take(request.ItemsPerPage).ToList();
        return PagedList.Create(items, count, request.PageNumber, request.ItemsPerPage);
    }

    public static string InstitutionId(this ClaimsPrincipal principal)
    {
        return principal.Identity.GetClaimValue(Constants.ClaimType.InstitutionId);
    }

    public static string Institution(this ClaimsPrincipal principal)
    {
        return principal.Identity.GetClaimValue(Constants.ClaimType.Institution);
    }

    #region Razor Page

    public static async Task<bool> IsPermissionAllowedAsync(this MvcBaseController controller, string permission)
    {
        return (await controller
            .ServicesWrapper
            .IdentityManager.PermissionManager.VerifyPermissionAsync(permission, controller.SessionUser?.Role.Name))
            .IsSuccessful;
    }

    public static SessionUser CurrentUser(this IRazorPage view)
    {
        return GetController(view).SessionUser;
    }

    private static MvcBaseController GetController(this IRazorPage view)
    {
        if (view.ViewContext.ActionDescriptor is not ControllerActionDescriptor cont) return null;

        if (view.ViewContext.HttpContext.RequestServices.GetService(cont.ControllerTypeInfo) is MvcBaseController
            baseController)
            return baseController;

        return null;
    }

    public static bool IsCurrentUserSystemUser(this IRazorPage view)
    {
        return CurrentUser(view).IsSystemUser();
    }

    public static async Task<IList<SelectListItem>> CountriesAsync(this IRazorPage view)
    {
        var list = (await view.GetController().ServicesWrapper.ConfigService.GetCountriesCachedAsync())
            .Select(t => new SelectListItem { Text = t.Name, Value = $"{t.Id}" }).ToList();
        list.AddFirstSelectListItem();
        return list;
    }

    public static async Task<IList<SelectListItem>> InstitutionsAsync(this IRazorPage view)
    {
        var list = (await view.GetController().ServicesWrapper.ConfigService.GetInstitutionsCached())
            .Select(t => new SelectListItem { Text = t.Name, Value = $"{t.Id}" }).ToList();
        list.Insert(0,
            new SelectListItem
                { Text = Constants.DefaultInstitutionName, Value = $"{Constants.DefaultInstitutionId}" });
        list.AddFirstSelectListItem("- All Institutions -");
        return list;
    }


    public static async Task<IList<SelectListItem>> CountriesCodeAsync(this IRazorPage view)
    {
        var list = (await view.GetController().ServicesWrapper.ConfigService.GetCountriesCachedAsync())
            .Select(t => new SelectListItem { Text = t.Name, Value = t.Code }).ToList();
        list.AddFirstSelectListItem();
        return list;
    }


    public static string StringVal(this IRazorPage view, string data)
    {
        return string.IsNullOrWhiteSpace(data) ? "-" : data;
    }


    public static async Task<bool> IsPermissionAllowedAsync(this IRazorPage view, string permission)
    {
        var controller = GetController(view);
        return await controller.IsPermissionAllowedAsync(permission);
    }

    #endregion
 
}