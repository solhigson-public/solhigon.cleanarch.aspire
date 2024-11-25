using Solhigson.Framework.Web;
using SolhigsonAspnetCoreApp.Application.Services;

namespace SolhigsonAspnetCoreApp.Application.Web;

public class ApiBaseController : SolhigsonApiControllerBase
{
    public ServicesWrapper ServicesWrapper { get; set; }
}