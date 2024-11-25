using Solhigson.Framework.Web;
using SolhigsonAspnetCoreApp.Application.Services;

namespace SolhigsonAspnetCoreApp.ApiService.Controllers;

public class ApiBaseController : SolhigsonApiControllerBase
{
    public ServicesWrapper ServicesWrapper { get; set; }
}