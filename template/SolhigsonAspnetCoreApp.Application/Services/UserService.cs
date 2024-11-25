using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Solhigson.Framework.Auditing;
using Solhigson.Framework.Data;
using Solhigson.Framework.Dto;
using Solhigson.Framework.EfCore;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Infrastructure;
using Solhigson.Framework.Utilities;
using Solhigson.Framework.Web;
using SolhigsonAspnetCoreApp.Domain.Dto;
using SolhigsonAspnetCoreApp.Domain.Entities;
using SolhigsonAspnetCoreApp.Domain.ViewModels;
using SolhigsonAspnetCoreApp.Infrastructure;
using SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions;
//using Solhigson.Framework.Infrastructure;
using Constants = SolhigsonAspnetCoreApp.Domain.Constants;

namespace SolhigsonAspnetCoreApp.Application.Services;

public class UserService : ServiceBase
{
    private const string UserSessionKey = $"::{Constants.ApplicationName}::user::current::";

    private SessionUser? _currentSessionUser;

    public UserService(IRepositoryWrapper repositoryWrapper) : base(repositoryWrapper)
    {
    }

    public void SetCurrentSessionUser(SessionUser sessionUser)
    {
        HelperFunctions.SafeSetSessionData(UserSessionKey, sessionUser, ServicesWrapper.HttpContextAccessor);
    }

    public SessionUser? GetCurrentSessionUser(bool throwExceptionIfNull = true)
    {
        if (_currentSessionUser is not null) return _currentSessionUser;
        _currentSessionUser = HelperFunctions.SafeGetSessionData<SessionUser>(UserSessionKey,
            ServicesWrapper.HttpContextAccessor);
        if (_currentSessionUser is not null) return _currentSessionUser;

        if (throwExceptionIfNull) throw new SessionExpiredException();

        return null;
    }

    public async Task<ResponseInfo> CreateUserAsync(UserDetailsInfo info)
    {
        var response = new ResponseInfo();
        var role = await RepositoryWrapper.DbContext.Roles.Include(t => t.RoleGroup).Where(t => t.Name
            == info.Role).FromCacheSingleAsync();
        if (role is null) return response.Fail($"Role does not exist: {info.Role}");

        if (role.IsSystemRole() && info.InstitutionId != Constants.DefaultInstitutionId)
            return response.Fail("System role does not match institution");

        if (role.IsInstitutionRole() && info.InstitutionId == Constants.DefaultInstitutionId)
            return response.Fail("Institution role does not match institution");

        if (info.Password != info.ConfirmPassword) return response.Fail("Passwords do not match");
        var user = info.Adapt<AppUser>();
        user.UserName = info.Email;
        user.Enabled = true;
        user.EmailConfirmed = user.PhoneNumberConfirmed = true;

        var result = await ServicesWrapper.IdentityManager.UserManager.CreateAsync(user, info.Password);
        if (!result.Succeeded) return response.Fail(result.Errors.FirstOrDefault()?.Description);
        await ServicesWrapper.IdentityManager.UserManager.AddToRoleAsync(user, info.Role);

        var passwordChangeInfo = "";
        if (user.RequirePasswordChange)
            passwordChangeInfo =
                "You will be required to change your password when you first login.<br/><br/>";

        await ServicesWrapper.UtilityService.SendMailAsync(new EmailParameter
        {
            To = new[] { info.Email },
            Subject = $"{Constants.ApplicationName} - Account Created",
            Template = "AccountCreatedEmail",
            TemplatePlaceholders = new Dictionary<string, string>
            {
                { "[[Name]]", info.Firstname },
                { "[[roleName]]", info.Role },
                { "[[username]]", info.Email },
                { "[[password]]", info.Password },
                { "[[url]]", ServicesWrapper.RootUrl },
                { "[[passwordchangeinfo]]", passwordChangeInfo }
            }
        });

        await AuditHelper.AuditAsync("Create User", new List<AuditEntry>
        {
            new()
            {
                Table = user.Email,
                Changes = new List<AuditChange>
                {
                    new()
                    {
                        ColumnName = "Institution",
                        NewValue = await ServicesWrapper.UtilityService.GetInstitutionNameAsync(user)
                    },
                    new() { ColumnName = "Role", NewValue = role.Name }
                }
            }
        });

        return response.Success();
    }

    public async Task<ResponseInfo<PagedList<UserModel>>> SearchUsersAsync(string requesterRole, int page = 1,
        int pageSize = 20, string institutionId = null,
        string name = null, string email = null, string role = null)
    {
        var response = new ResponseInfo<PagedList<UserModel>>();

        try
        {
            var roles = await GetRolesUserCanAdminister(requesterRole, true);
            if (!roles.IsSuccessful)
                return response.Fail($"Error while checking requester role status: {roles.Message}");

            var rList = roles.Data.Select(t => t.Name);
            var query = from u in RepositoryWrapper.DbContext.Users
                join ur in RepositoryWrapper.DbContext.UserRoles
                    on u.Id equals ur.UserId
                join r in RepositoryWrapper.DbContext.Roles
                    on ur.RoleId equals r.Id
                where rList.Contains(r.Name)
                select new
                {
                    u.Id, u.Firstname, u.OtherNames, u.NormalizedEmail,
                    u.Lastname, u.UserName, u.Email, u.NormalizedUserName,
                    u.PhoneNumber, u.Enabled, Role = r.Name,
                    u.InstitutionId
                };
            // var query = RepositoryWrapper.DbContext.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(institutionId)) query = query.Where(t => t.InstitutionId == institutionId);

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(t => t.Firstname.Contains(name)
                                         || t.Lastname.Contains(name));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(t => t.NormalizedEmail.Contains(email)
                                         || t.NormalizedUserName.Contains(email));

            if (!string.IsNullOrWhiteSpace(role))
                query = from u in query.Where(t => t.Role == role)
                    select u;

            var result = await query.ToPagedListAsync(page, pageSize);
            var adapt = new List<UserModel>();
            foreach (var model in result.Results.Select(user => user.Adapt<UserModel>()))
            {
                model.CheckStatus();
                model.Institution = (await RepositoryWrapper.InstitutionRepository.GetByIdCachedAsync(model.InstitutionId))?.Name ??
                                    Constants.DefaultInstitutionName;
                adapt.Add(model);
            }

            var finalResult = PagedList.Create(adapt, result.TotalCount, result.CurrentPage, result.PageSize);
            return response.Success(finalResult);
        }
        catch (Exception e)
        {
            this.LogError(e);
        }

        return response.Fail();
    }

    private static void CheckIfDifferent(string property, string oldVal, string newVal, List<AuditChange> changes)
    {
        if (string.Compare(oldVal, newVal, StringComparison.OrdinalIgnoreCase) != 0)
            changes.Add(new AuditChange
            {
                ColumnName = property,
                OriginalValue = oldVal,
                NewValue = newVal
            });
    }

    public async Task<ResponseInfo> UpdateUser(string userId, UserDetailsInfo info)
    {
        var user = await ServicesWrapper.IdentityManager.UserManager.FindByIdAsync(userId);
        if (user is null) return ResponseInfo.FailedResult("User not found");

        var username = user.UserName;
        var changes = new List<AuditChange>();
        if (string.Compare(user.Email, info.Email, StringComparison.OrdinalIgnoreCase) != 0)
        {
            var normalizedEmail = info.Email.ToUpper();
            if (await RepositoryWrapper.DbContext.Users.AnyAsync(t => t.NormalizedUserName == normalizedEmail))
                return ResponseInfo.FailedResult(
                    $"Cannot change email from {user.Email} to {info.Email}, as {info.Email} " +
                    $"is already in use.");

            changes.Add(new AuditChange
            {
                ColumnName = "Email",
                OriginalValue = user.Email,
                NewValue = info.Email
            });
            user.UserName = info.Email;
            user.Email = info.Email;
        }

        CheckIfDifferent(nameof(user.Firstname), user.Firstname, info.Firstname, changes);
        user.Firstname = info.Firstname;

        CheckIfDifferent(nameof(user.Lastname), user.Lastname, info.Lastname, changes);
        user.Lastname = info.Lastname;

        CheckIfDifferent(nameof(user.OtherNames), user.OtherNames, info.OtherNames, changes);
        user.OtherNames = info.OtherNames;

        CheckIfDifferent(nameof(user.PhoneNumber), user.PhoneNumber, info.PhoneNumber, changes);
        user.PhoneNumber = info.PhoneNumber;

        await ServicesWrapper.IdentityManager.UserManager.UpdateAsync(user);

        var roles = await ServicesWrapper.IdentityManager.UserManager.GetRolesAsync(user);
        await ServicesWrapper.IdentityManager.UserManager.RemoveFromRolesAsync(user, roles);

        CheckIfDifferent(nameof(info.Role), roles.FirstOrDefault(), info.Role, changes);
        await ServicesWrapper.IdentityManager.UserManager.AddToRoleAsync(user, info.Role);

        if (changes.Any())
            await AuditHelper.AuditAsync("Update User", new List<AuditEntry>
            {
                new()
                {
                    Table = username,
                    Changes = changes
                }
            });

        return ResponseInfo.SuccessResult();
    }

    public async Task<ResponseInfo> ChangePasswordAsync(string userName, string currentPassword, string newPassword,
        string retypeNewPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
            return ResponseInfo.FailedResult("Current password is required");
        if (string.IsNullOrWhiteSpace(newPassword)) return ResponseInfo.FailedResult("New password is required");
        if (newPassword != retypeNewPassword)
            return ResponseInfo.FailedResult("New password doesn't match with confirmation password");
        var user = await ServicesWrapper.IdentityManager.UserManager.FindByNameAsync(userName);

        var result =
            await ServicesWrapper.IdentityManager.UserManager.ChangePasswordAsync(user, currentPassword,
                newPassword);

        if (!result.Succeeded) return ResponseInfo.FailedResult(result.Errors.FirstOrDefault()?.Description);

        await RepositoryWrapper.DbContext.Entry(user).ReloadAsync();
        await AuditHelper.AuditAsync("Change Password");
        return ResponseInfo.SuccessResult("Your password has been successfully changed");
    }

    public async Task<ResponseInfo<string>> ResetPasswordAsync(string userId, string token, string password,
        string retypeNewPassword)
    {
        if (password != retypeNewPassword)
            return ResponseInfo.FailedResult<string>("New password doesn't match with confirmation password");

        var user = await ServicesWrapper.IdentityManager.UserManager.FindByIdAsync(userId);
        if (user == null) return ResponseInfo.FailedResult<string>("Invalid User");

        var result =
            await ServicesWrapper.IdentityManager.UserManager.ResetPasswordAsync(user, token, password);

        if (!result.Succeeded) return ResponseInfo.FailedResult<string>(result.Errors.FirstOrDefault()?.Description);

        await RepositoryWrapper.DbContext.Entry(user).ReloadAsync();
        user.RequirePasswordChange = false;
        await RepositoryWrapper.SaveChangesAsync();
        await AuditHelper.AuditAsync("Complete Password Reset", "Email", "", user.UserName);
        return ResponseInfo.SuccessResult<string>(user.UserName);
    }

    public async Task<ResponseInfo<AppUser>> GetAbridgedUserAsync(string email)
    {
        var user = await (from u in RepositoryWrapper.DbContext.Users
            join ur in RepositoryWrapper.DbContext.UserRoles
                on u.Id equals ur.UserId
            join r in RepositoryWrapper.DbContext.Roles.Include(t => t.RoleGroup)
                on ur.RoleId equals r.Id
            where u.UserName == email
            select new AppUser
            {
                Id = u.Id,
                Firstname = u.Firstname,
                Lastname = u.Lastname,
                UserRole = r
            }).FirstOrDefaultAsync();

        return user is null
            ? ResponseInfo.FailedResult<AppUser>($"Invalid user: {email}")
            : ResponseInfo.SuccessResult(user);
    }

    public async Task<ResponseInfo<SessionUser>> SignInAsync(string userName, string password)
    {
        var response = new ResponseInfo<SessionUser>();
        var signInResult = await ServicesWrapper.IdentityManager.SignIn(userName, password);

        if (!signInResult.IsSuccessful) return response.Fail("Email or password is incorrect", StatusCode.UnAuthorised);

        if (!signInResult.User.Enabled) return response.Fail("Your account is not enabled.");

        var role = signInResult.User.Roles.FirstOrDefault();

        if (role is null) return response.Fail("User not assigned to any role");

        var user = signInResult.User.Adapt<SessionUser>();
        user.Role = role;
        user.RequiresTwoFactor = signInResult.RequiresTwoFactor;
        user.IsLockedOut = signInResult.IsLockedOut;
        user.Institution = await ServicesWrapper.UtilityService.GetInstitutionAsync(user.InstitutionId);
        return user.Institution is null
            ? response.Fail("User institution profile is invalid")
            : response.Success(user);
    }

    public async Task<ResponseInfo<List<RoleDto>>> GetRolesUserCanAdminister(string userRole, bool isForSearch = false)
    {
        var response = new ResponseInfo<List<RoleDto>>();
        var role = await RepositoryWrapper.DbContext.Roles.Include(t => t.RoleGroup)
            .Where(t => t.Name == userRole).FirstOrDefaultAsync();

        if (role is null) return response.Fail($"role does not exist: {userRole}");

        if (!role.IsSystemRole() && !role.IsInstitutionRole())
            return response.Fail($"Your role: {userRole}, cannot administer other roles: {userRole}");

        List<RoleDto> result;
        var systemRolesQuery =
            RepositoryWrapper.DbContext.Roles.Where(t => t.RoleGroup.Name == Constants.AppRoles.Groups.System);
        var institutionRolesQuery =
            RepositoryWrapper.DbContext.Roles.Where(t => t.RoleGroup.Name == Constants.AppRoles.Groups.Institution);
        if (role.IsSystemRole())
        {
            if (role.Name != Constants.AppRoles.SystemAdministrator)
                systemRolesQuery = systemRolesQuery.Where(t => t.Name != Constants.AppRoles.SystemAdministrator);
            result = (await systemRolesQuery.ProjectToType<RoleDto>().FromCacheListAsync()).ToList();
            if (!isForSearch)
                institutionRolesQuery =
                    institutionRolesQuery.Where(t => t.Name == Constants.AppRoles.InstitutionAdministrator);
            result.AddRange(await institutionRolesQuery.ProjectToType<RoleDto>().FromCacheListAsync());
        }
        else
        {
            result = (await institutionRolesQuery.ProjectToType<RoleDto>().FromCacheListAsync()).ToList();
        }

        return response.Success(result.AsQueryable().OrderBy(t => t.Name).ToList());
    }

    public async Task<ResponseInfo> SendPasswordResetEmailAsync(string username, string url)
    {
        var response = new ResponseInfo();
        if (string.IsNullOrWhiteSpace(username)) return response.Fail("Please provide your email");

        var user = await ServicesWrapper.IdentityManager.UserManager.FindByNameAsync(username);
        if (user == null) return response.Fail("Invalid email");

        var expiresTime = ServicesWrapper.UtilityService.DefaultTokenExpiryTime();
        var token = await ServicesWrapper.IdentityManager.UserManager.GeneratePasswordResetTokenAsync(user);
        token = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{token}|{user.Id}|{expiresTime.ToUnixTimestamp()}"));
        var callback = $"{url}/{token}";

        await ServicesWrapper.UtilityService.SendMailAsync(new EmailParameter
        {
            To = new[] { user.Email },
            Subject = "Reset your password",
            Template = "PasswordResetEmail",
            TemplatePlaceholders = new Dictionary<string, string>
            {
                { "[[Name]]", user.Firstname },
                { "[[url]]", callback },
                { "[[expiry]]", $"{ServicesWrapper.AppSettings.UserTokenValidityPeriodHrs} hours" }
            }
        });

        await AuditHelper.AuditAsync("Reset Password Request", "Email", "", username);
        return response.Success("An email has been sent to you with instructions on resetting your password");
    }

    public async Task<List<NotificationUser>> GetUsersWithPermission(string institutionId, string permission)
    {
        return await (from user in RepositoryWrapper.DbContext.Users
            join userRole in RepositoryWrapper.DbContext.UserRoles
                on user.Id equals userRole.UserId
            join role in RepositoryWrapper.DbContext.Roles
                on userRole.RoleId equals role.Id
            join rolePerm in RepositoryWrapper.DbContext.RolePermissions
                on userRole.RoleId equals rolePerm.RoleId
            join perm in RepositoryWrapper.DbContext.Permissions
                on rolePerm.PermissionId equals perm.Id
            where perm.Name == permission
                  && user.Enabled
                  && user.InstitutionId == institutionId
            select user).ProjectToType<NotificationUser>().ToListAsync();
    }

    public async Task<List<NotificationUser>> GetUsersInRoles(string[] roles, string institutionId)
    {
        return await (from user in RepositoryWrapper.DbContext.Users
            join userRole in RepositoryWrapper.DbContext.UserRoles
                on user.Id equals userRole.UserId
            join role in RepositoryWrapper.DbContext.Roles
                on userRole.RoleId equals role.Id
            where roles.Contains(role.Name)
                  && user.Enabled
                  && user.InstitutionId == institutionId
            select user).ProjectToType<NotificationUser>().ToListAsync();
    }

    public async Task<bool> IsUserActionAllowed(SessionUser user, string action)
    {
        return (await ServicesWrapper.IdentityManager.PermissionManager
            .VerifyPermissionAsync(action, user.Role?.Name)).IsSuccessful;
    }

    public static IEnumerable<Claim> GenerateClaims(SessionUser user)
    {
        return new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Role, user.Role?.Name ?? ""),
            new(Constants.ClaimType.Institution, user.Institution?.Name ?? ""),
            new(Constants.ClaimType.InstitutionId, user.Institution?.Id ?? ""),
            new(Constants.ClaimType.IsSystemUser, $"{user.Role.IsSystemRole()}"),
            new(Constants.ClaimType.IsInstitutionUser, $"{user.Role.IsInstitutionRole()}")
        };
    }
}