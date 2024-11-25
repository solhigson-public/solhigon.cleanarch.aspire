using System.Security.Claims;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Solhigson.Framework.Identity;
using Solhigson.Framework.Infrastructure.Dependency;
using Solhigson.Framework.Utilities;
using Solhigson.Framework.Web.Api;
using SolhigsonAspnetCoreApp.Application.Services.Abstractions;
using SolhigsonAspnetCoreApp.Domain.Entities;
using SolhigsonAspnetCoreApp.Infrastructure;
using SolhigsonAspnetCoreApp.Infrastructure.ApplicationSettings;

namespace SolhigsonAspnetCoreApp.Application.Services;

public class ServicesWrapper : IDependencyInject
{
    public ServicesWrapper(AppSettings appSettings, IHttpContextAccessor httpContextAccessor,
        IApiRequestService apiRequestService, IBackgroundJobClient backgroundJobClient,
        SolhigsonIdentityManager<AppUser, AppDbContext> identityManager, IWebHostEnvironment webHostEnvironment)
    {
        AppSettings = appSettings;
        HttpContextAccessor = httpContextAccessor;
        ApiRequestService = apiRequestService;
        BackgroundJobClient = backgroundJobClient;
        IdentityManager = identityManager;
        WebHostEnvironment = webHostEnvironment;
    }

    public string RootUrl => HttpUtils.UrlRoot(HttpContextAccessor?.HttpContext);
    public ClaimsPrincipal? ClaimsUser => HttpContextAccessor?.HttpContext?.User;

    #region Injected via constructor - no property reference to ServicesWrapper

    public AppSettings AppSettings { get; }
    public IHttpContextAccessor HttpContextAccessor { get; }
    public IApiRequestService ApiRequestService { get; }
    public IBackgroundJobClient BackgroundJobClient { get; }
    public IWebHostEnvironment WebHostEnvironment { get; }
    public SolhigsonIdentityManager<AppUser, AppDbContext> IdentityManager { get; }

    #endregion

    #region Injected via Autofac Property Autowired - may or may not have circular dependency property reference to ServicesWrapper

    public ConfigService ConfigService { get; set; }
    public UtilityService UtilityService { get; set; }
    public IAuditLogService AuditLogService { get; set; }
    public UserService UserService { get; set; }

    #endregion
}