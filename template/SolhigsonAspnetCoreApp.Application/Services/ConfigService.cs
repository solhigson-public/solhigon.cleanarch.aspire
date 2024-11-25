using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Solhigson.Framework.Data;
using Solhigson.Framework.Dto;
using Solhigson.Framework.EfCore;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Identity;
using Solhigson.Framework.Logging;
using Solhigson.Framework.Persistence.EntityModels;
using Solhigson.Framework.Services;
using SolhigsonAspnetCoreApp.Domain;
using SolhigsonAspnetCoreApp.Domain.CacheModels;
using SolhigsonAspnetCoreApp.Domain.Dto;
using SolhigsonAspnetCoreApp.Domain.Entities;
using SolhigsonAspnetCoreApp.Domain.ViewModels;
using SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions;

namespace SolhigsonAspnetCoreApp.Application.Services;

public class ConfigService : ServiceBase
{
    private readonly SolhigsonConfigurationService _solhigsonConfigurationService;

    public ConfigService(IRepositoryWrapper repositoryWrapper,
        SolhigsonConfigurationService solhigsonConfigurationService) : base(repositoryWrapper)
    {
        _solhigsonConfigurationService = solhigsonConfigurationService;
    }

    #region Users

    #endregion

    #region Permissions

    public async Task<ResponseInfo> CreateOrEditPermissionAsync(PermissionModel model)
    {
        SolhigsonPermission permission = null;
        if (!string.IsNullOrWhiteSpace(model.Id))
            permission = await RepositoryWrapper.DbContext.Permissions
                .FirstOrDefaultAsync(t => t.Id == model.Id);

        if (permission is null)
        {
            var existing =
                await RepositoryWrapper.DbContext.Permissions.FirstOrDefaultAsync(t => t.Name == model.Name);
            if (existing != null)
                return ResponseInfo.FailedResult($"Permission with name already exists: {existing.Name}");

            model.Id = Guid.NewGuid().ToString();
            permission = model.Adapt<SolhigsonPermission>();
            RepositoryWrapper.DbContext.Add(permission);
        }
        else
        {
            var name = permission.Name;
            permission = model.Adapt(permission);
            permission.Name = name;
            RepositoryWrapper.DbContext.Update(permission);
        }

        await RepositoryWrapper.SaveChangesAsync();
        return ResponseInfo.SuccessResult();
    }

    public async Task<List<PermissionModel>> GetTopLevelPermissionsAsync()
    {
        return await RepositoryWrapper.DbContext.Permissions.Where(t => t.IsMenuRoot && t.Enabled)
            .Select(t => new PermissionModel { Id = t.Id, Description = t.Description }).ToListAsync();
    }

    public async Task<ResponseInfo<PagedList<PermissionModel>>> SearchPermissionsAsync(int page = 1,
        int pageSize = 20,
        string searchParams = null)
    {
        var response = new ResponseInfo<PagedList<PermissionModel>>();

        try
        {
            var query = RepositoryWrapper.DbContext.Permissions.Include(t => t.Parent).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchParams))
                query = query.Where(t => t.Description.Contains(searchParams)
                                         || t.Name.Contains(searchParams));

            var result = await query
                .Select(t => new
                {
                    t.Id, t.Description, t.Enabled,
                    t.Icon, t.Name, t.Url, t.IsMenu, t.MenuIndex, t.ParentId,
                    t.IsMenuRoot, t.OnClickFunction, Parent = t.Parent.Description
                }).ToPagedListAsync(page, pageSize);
            var adapt = new List<PermissionModel>();
            foreach (var user in result.Results)
            {
                var model = user.Adapt<PermissionModel>();
                var allowedRoles =
                    await ServicesWrapper.IdentityManager.PermissionManager.GetAllowedRolesForPermissionAsync(
                        model.Name);
                foreach (var role in allowedRoles) model.AllowedRoles += $"{role}<br/>";

                adapt.Add(model);
            }

            var finalResult = PagedList.Create(adapt, result.TotalCount, result.CurrentPage, result.PageSize);
            return response.Success(finalResult);
        }
        catch (Exception e)
        {
            this.ELogError(e);
        }

        return response.Fail();
    }

    #endregion

    #region Roles

    public async Task<ResponseInfo<List<RoleGroupModel>>> GetRoleGroupsAsync()
    {
        var response = new ResponseInfo<List<RoleGroupModel>>();
        try
        {
            var groups = await RepositoryWrapper.DbContext.RoleGroups
                .OrderBy(t => t.Name)
                .ProjectToType<RoleGroupModel>().ToListAsync();
            return response.Success(groups);
        }
        catch (Exception e)
        {
            this.ELogError(e);
        }

        return response.Fail();
    }

    public async Task<List<RoleDto>> GetRolesByInstitution(string institutionId, bool isSystemUser)
    {
        var query = RepositoryWrapper.DbContext.Roles.AsQueryable();

        if (institutionId == Constants.DefaultInstitutionId)
        {
            query = query.Where(t => t.RoleGroup.Name == Constants.AppRoles.Groups.System);
        }
        else
        {
            query = query.Where(t => t.RoleGroup.Name == Constants.AppRoles.Groups.Institution);
            if (isSystemUser) query = query.Where(t => t.Name == Constants.AppRoles.InstitutionAdministrator);
        }

        return await query.ProjectToType<RoleDto>().FromCacheListAsync(typeof(SolhigsonAspNetRole));
    }

    public async Task<ResponseInfo<List<RoleModel>>> GetRolesAsync(string roleGroupId = null)
    {
        var response = new ResponseInfo<List<RoleModel>>();
        try
        {
            var query = RepositoryWrapper.DbContext.Roles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(roleGroupId)) query = query.Where(t => t.RoleGroupId == roleGroupId);

            var result = await query.Include(t => t.RoleGroup)
                .OrderBy(t => t.RoleGroup.Name).ThenBy(t => t.Name)
                .ToListAsync();
            var roles = new List<RoleModel>();
            foreach (var role in result)
            {
                var model = role.Adapt<RoleModel>();
                model.RoleGroup = role.RoleGroup.Name;
                roles.Add(model);
            }

            return response.Success(roles);
        }
        catch (Exception e)
        {
            this.ELogError(e);
        }

        return response.Fail();
    }

    public async Task<ResponseInfo> CreateOrEditRoleAsync(RoleModel model)
    {
        SolhigsonAspNetRole role = null;
        if (!string.IsNullOrWhiteSpace(model.Id))
            role = await
                RepositoryWrapper.DbContext.Roles.Where(t => t.Id == model.Id).FirstOrDefaultAsync();

        IdentityResult result;
        if (role == null)
        {
            role = new SolhigsonAspNetRole
            {
                Name = model.Name,
                StartPage = model.StartPage,
                RoleGroupId = model.RoleGroupId
            };
            result = await ServicesWrapper.IdentityManager.RoleManager.CreateAsync(role);
        }
        else
        {
            role.StartPage = model.StartPage;
            role.RoleGroupId = model.RoleGroupId;
            result = await ServicesWrapper.IdentityManager.RoleManager.UpdateAsync(role);
        }

        return result.Succeeded
            ? ResponseInfo.SuccessResult()
            : ResponseInfo.FailedResult(result.Errors.FirstOrDefault()?.Description);
    }

    public async Task<ResponseInfo<RolePermissionModel>> GetAllPermissionsForRoleAsync(string roleName)
    {
        var response = new ResponseInfo<RolePermissionModel>();
        var role = await RepositoryWrapper.DbContext.Roles.FirstOrDefaultAsync(t => t.Name == roleName);
        if (role is null) return response.Fail($"Role does not exist: {roleName}");

        var rolePermissions =
            await RepositoryWrapper.DbContext.RolePermissions.Where(t => t.RoleId == role.Id)
                .ToListAsync();
        var model = await GetAllPermissionsAsync();
        GetSelectedPermissions(model, rolePermissions);
        model.Role = role.Name;
        model.RoleId = role.Id;
        return response.Success(model);
    }

    private static void GetSelectedPermissions(RolePermissionModel model,
        IEnumerable<SolhigsonRolePermission<string>> permissions)
    {
        var list = permissions.ToList();
        foreach (var perm in model.Permissions.SelectMany(obj => obj.Children))
        {
            perm.Selected = false;
            var perm1 = perm;
            foreach (var ur in list.Where(ur =>
                         string.Compare(ur.PermissionId, perm1.Value, StringComparison.OrdinalIgnoreCase) == 0))
                perm.Selected = true;
        }
    }

    public async Task<RolePermissionModel> GetAllPermissionsAsync()
    {
        var topLevel = await RepositoryWrapper.DbContext.Permissions
            .Where(t => t.IsMenuRoot).OrderBy(t => t.Name).ToListAsync();
        foreach (var top in topLevel)
            top.Children = await RepositoryWrapper.DbContext.Permissions
                .Where(t => t.ParentId == top.Id).ToListAsync();

        var uncategorized = new SolhigsonPermission
        {
            Description = "[Un-Categorized]",
            IsMenuRoot = true
        };
        uncategorized.Children = await RepositoryWrapper.DbContext.Permissions
            .Where(t => string.IsNullOrWhiteSpace(t.ParentId) && !t.IsMenuRoot).ToListAsync();
        if (uncategorized.Children.Any()) topLevel.Add(uncategorized);

        var model = new RolePermissionModel
        {
            Permissions = new List<RolePermission>()
        };
        foreach (var perm in topLevel)
        {
            var permModel = new RolePermission
            {
                Name = perm.Description,
                Id = perm.Id,
                Children = new List<SelectListItem>()
            };
            permModel.Children.AddRange(GetChildren(perm));
            model.Permissions.Add(permModel);
        }

        return model;
    }

    private static IEnumerable<SelectListItem> GetChildren(SolhigsonPermission permission)
    {
        var list = new List<SelectListItem>();
        if (permission.Children == null || !permission.Children.Any())
        {
            if (permission.IsMenuRoot)
                list.Add(new SelectListItem { Text = GetLabel(permission), Value = permission.Id });
        }
        else
        {
            list.AddRange(
                permission.Children.Select(obj => new SelectListItem { Text = GetLabel(obj), Value = obj.Id }));
        }

        if (!list.Any() &&
            (!string.IsNullOrWhiteSpace(permission.Url) ||
             !string.IsNullOrWhiteSpace(permission.OnClickFunction)) && permission.IsMenuRoot)
            list.Add(new SelectListItem { Text = GetLabel(permission), Value = permission.Url });

        return list;
    }

    private static string GetLabel(SolhigsonPermission function)
    {
        return string.IsNullOrWhiteSpace(function.Url)
            ? function.Description
            : $"{function.Description} ({function.Url})";
    }

    public async Task<ResponseInfo> SavePermissions(string roleName,
        IEnumerable<SelectListItem> selectedPermissions)
    {
        var response = new ResponseInfo();
        var role = await RepositoryWrapper.DbContext.Roles.FirstOrDefaultAsync(t => t.Name == roleName);
        if (role is null) return response.Fail($"Role does not exist: {roleName}");

        var rolePermissions =
            await RepositoryWrapper.DbContext.RolePermissions.Where(t => t.RoleId == role.Id)
                .ToListAsync();
        RepositoryWrapper.DbContext.RolePermissions.RemoveRange(rolePermissions);
        foreach (var obj in selectedPermissions)
        {
            var rolePermission = rolePermissions.FirstOrDefault(t =>
                string.Compare(t.PermissionId, obj.Value, StringComparison.Ordinal) == 0);
            if (rolePermission is null)
                RepositoryWrapper.DbContext.Add(new SolhigsonRolePermission<string>
                {
                    PermissionId = obj.Value,
                    RoleId = role.Id
                });
            else
                RepositoryWrapper.DbContext.Update(rolePermission);
        }

        await RepositoryWrapper.DbContext.SaveChangesAsync();
        return response.Success($"Permissions for {roleName} updated successfully");
    }

    #endregion

    #region App Settings

    public async Task<ResponseInfo> CreateOrUpdateAppSettingAsync(AppSetting appSetting)
    {
        if (appSetting.Id > 0)
        {
            var result = await _solhigsonConfigurationService.UpdateApplicationSettingAsync(appSetting);
            if (result.IsSuccessful && appSetting.Name == "Initialization:LogLevel")
                LogManager.SetLogLevel(appSetting.Value);

            return result;
        }

        return await _solhigsonConfigurationService.CreateApplicationSettingAsync(appSetting);
    }

    public async Task<ResponseInfo> DeleteAppSettingAsync(AppSetting appSetting)
    {
        return await _solhigsonConfigurationService.DeleteApplicationSettingAsync(appSetting);
    }

    public async Task<ResponseInfo<PagedList<AppSetting>>> SearchAppSettingAsync(int page = 1, int pageSize = 20,
        string name = null)
    {
        return await _solhigsonConfigurationService.SearchAppSettingsAsync(page, pageSize, name);
    }

    #endregion

    #region Notification Templates

    public async Task<ResponseInfo> CreateOrUpdateNotificationTemplateAsync(NotificationTemplate template)
    {
        return await _solhigsonConfigurationService.SaveNotificationTemplateAsync(template);
    }

    public async Task<ResponseInfo> DeleteNotificationTemplateAsync(NotificationTemplate template)
    {
        return await _solhigsonConfigurationService.DeleteNotificationTemplateAsync(template);
    }

    public async Task<ResponseInfo<NotificationTemplate>> GetNotificationTemplateAsync(string name)
    {
        return await _solhigsonConfigurationService.GetNotificationTemplateAsync(name);
    }

    public async Task<ResponseInfo<PagedList<NotificationTemplate>>> SearchNotificationTemplatesAsync(int page = 1,
        int pageSize = 20,
        string name = null)
    {
        return await _solhigsonConfigurationService.SearchNotificationTemplatesAsync(page, pageSize, name);
    }

    #endregion

    #region Country

    public async Task<ResponseInfo> CreateOrEditCountryAsync(CountryDto model)
    {
        var resp = new ResponseInfo();
        Country entity;
        if (!string.IsNullOrWhiteSpace(model.Id))
        {
            entity = await RepositoryWrapper.CountryRepository.GetByIdAsync(model.Id);
            if (entity is null) return resp.Fail("Unable to update country");
        }
        else
        {
            entity = RepositoryWrapper.CountryRepository.New();
        }

        model.Adapt(entity);
        await RepositoryWrapper.SaveChangesAsync();
        return resp.Success();
    }

    public async Task<List<CountryDto>> GetCountriesCachedAsync()
    {
        return await RepositoryWrapper.DbContext.Countries.ProjectToType<CountryDto>()
            .OrderBy(t => t.Name).FromCacheListAsync(typeof(Country));
    }

    public async Task<ResponseInfo<PagedList<CountryDto>>> SearchCountriesAsync(SearchCountryRequest request)
    {
        var response = new ResponseInfo<PagedList<CountryDto>>();
        try
        {
            var query = GetSearchCountriesQuery(request);
            if (!query.IsSuccessful) return response.Fail(query.InfoResult);

            var result = await query.Data
                .OrderBy(t => t.Name)
                .ProjectToType<CountryDto>().ToPagedListAsync(request.PageNumber, request.ItemsPerPage);

            return response.Success(result);
        }
        catch (Exception e)
        {
            this.ELogError(e);
        }

        return response.Fail();
    }

    private ResponseInfo<IQueryable<Country>> GetSearchCountriesQuery(SearchCountryRequest request)
    {
        var response = new ResponseInfo<IQueryable<Country>>();
        var query = RepositoryWrapper.DbContext.Countries.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Name)) query = query.Where(t => t.Name.Contains(request.Name));

        return response.Success(query);
    }

    public async Task<ResponseInfo<PagedList<CountryDto>>> SearchCountriesCached(SearchCountryRequest request)
    {
        var response = new ResponseInfo<PagedList<CountryDto>>();
        try
        {
            var query = GetSearchCountriesQuery(request);
            if (!query.IsSuccessful) return response.Fail(query.InfoResult);

            var result = await query.Data
                .OrderBy(t => t.Name)
                .ProjectToType<CountryDto>()
                .FromCacheListPagedAsync(request.PageNumber, request.ItemsPerPage, typeof(Country));


            return response.Success(result);
        }
        catch (Exception e)
        {
            this.LogError(e);
        }

        return response.Fail();
    }

    #endregion

    #region Currencies

    public async Task<ResponseInfo> CreateOrEditCurrencyAsync(CurrencyDto model)
    {
        var resp = new ResponseInfo();
        Currency entity;
        if (!string.IsNullOrWhiteSpace(model.Id))
        {
            entity = await RepositoryWrapper.CurrencyRepository.GetByIdAsync(model.Id);
            if (entity is null) return resp.Fail("Unable to update currency");
        }
        else
        {
            entity = RepositoryWrapper.CurrencyRepository.New();
        }

        model.Adapt(entity);
        await RepositoryWrapper.SaveChangesAsync();
        return resp.Success();
    }

    public async Task<List<CurrencyDto>> GetCurrenciesCached()
    {
        return await RepositoryWrapper.DbContext.Currencies.ProjectToType<CurrencyDto>()
            .OrderBy(t => t.Name).FromCacheListAsync(typeof(Currency));
    }

    public async Task<List<CurrencyDto>> GetCurrenciesAsync()
    {
        return await RepositoryWrapper.DbContext.Currencies.ProjectToType<CurrencyDto>()
            .OrderBy(t => t.Name).ToListAsync();
    }

    #endregion

    #region Institutions

    public async Task<InstitutionCacheModel?> GetInstitutionsCached(string institutionId)
    {
        return await RepositoryWrapper.InstitutionRepository.GetByIdCachedAsync(institutionId);
    }

    public async Task<List<InstitutionDto>> GetInstitutionsCached()
    {
        return await RepositoryWrapper.DbContext.Institutions.Select(t => new { t.Id, t.Name })
            .OrderBy(t => t.Name).ProjectToType<InstitutionDto>().FromCacheListAsync(typeof(Institution));
    }

    public async Task<ResponseInfo<PagedList<InstitutionDto>>> SearchInstitutionsAsync(PagedRequestBase request)
    {
        var response = new ResponseInfo<PagedList<InstitutionDto>>();
        try
        {
            var query = RepositoryWrapper.DbContext.Institutions.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Name))
                query = query.Where(t => t.Name.Contains(request.Name));

            var result = await query
                .OrderBy(t => t.Name)
                .ProjectToType<InstitutionDto>().ToPagedListAsync(request.PageNumber, request.ItemsPerPage);


            return response.Success(result);
        }
        catch (Exception e)
        {
            this.ELogError(e);
        }

        return response.Fail();
    }

    public async Task<ResponseInfo> CreateOrEditInstitutionAsync(InstitutionDto model)
    {
        var resp = new ResponseInfo();
        Institution entity;
        if (!string.IsNullOrWhiteSpace(model.Id))
        {
            entity = await RepositoryWrapper.InstitutionRepository.GetByIdAsync(model.Id);
            if (entity is null) return resp.Fail("Unable to update institution");
        }
        else
        {
            entity = RepositoryWrapper.InstitutionRepository.New();
        }

        model.Adapt(entity);
        await RepositoryWrapper.SaveChangesAsync();
        return resp.Success();
    }

    #endregion
}