using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Solhigson.Framework.Web;
using Solhigson.Framework.Web.Attributes;
using SolhigsonAspnetCoreApp.Application.Services;
using SolhigsonAspnetCoreApp.Domain.Dto;

namespace SolhigsonAspnetCoreApp.Application.Web;

public class MvcBaseController : SolhigsonMvcControllerBase
{
    private SessionUser? _sessionUser;
    public ServicesWrapper ServicesWrapper { get; set; }
    public SessionUser? SessionUser => _sessionUser ??= ServicesWrapper?.UserService.GetCurrentSessionUser();

    protected ActionResult RedirectToAppropriateView(bool tryGetUserFromSession = false)
    {
        if (_sessionUser == null && tryGetUserFromSession)
            _sessionUser = ServicesWrapper?.UserService.GetCurrentSessionUser(false);

        if (_sessionUser == null) return RedirectToAction("login", "home", new { area = "" });

        if (string.IsNullOrWhiteSpace(_sessionUser.Role?.Name))
        {
            HttpContext.Session.Clear();
            SetErrorMessage("Unexpected error - no roles have been assigned to you");
            return RedirectToAction("login", "home", new { area = "" });
        }

        if (!string.IsNullOrWhiteSpace(_sessionUser.Role?.StartPage)) return Redirect(_sessionUser.Role.StartPage);

        return RedirectToAction("Dashboard", "Home", new { area = "" });
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var endPoint = filterContext.HttpContext.GetEndpoint();

        if (endPoint == null
            || endPoint.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            base.OnActionExecuting(filterContext);
            return;
        }

        _sessionUser ??= ServicesWrapper.UserService.GetCurrentSessionUser(false);

        if (_sessionUser == null)
        {
            SetErrorMessage("Your session has expired, kindly login.");
            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Login.cshtml"
            };
            return;
        }

        SetClaimsPrincipal(_sessionUser);

        var permissionName = endPoint.Metadata.GetMetadata<PermissionAttribute>()?.Name;

        if (string.IsNullOrWhiteSpace(permissionName))
        {
            base.OnActionExecuting(filterContext);
            return;
        }

        var verifyResult = ServicesWrapper.IdentityManager.PermissionManager
            .VerifyPermissionAsync(permissionName, User).Result;

        if (verifyResult.IsSuccessful)
        {
            base.OnActionExecuting(filterContext);
            return;
        }

        SetErrorMessage("Forbidden");
        filterContext.Result = RedirectToAction("_403", "Home");
    }

    public void SetClaimsPrincipal(SessionUser user)
    {
        if (user is null) return;
        var claimsIdentity = new ClaimsIdentity(UserService.GenerateClaims(user), "session");
        HttpContext.User = new ClaimsPrincipal();
        HttpContext.User.AddIdentity(claimsIdentity);
    }
}