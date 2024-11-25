using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Solhigson.Framework.Dto;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Web;
using SolhigsonAspnetCoreApp.Infrastructure;

namespace SolhigsonAspnetCoreApp.Application.Web.Filters;

[AttributeUsage(AttributeTargets.Class)]
public class NonProductionEnvironment : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is not ControllerBase cont || !cont.IsProduction()) return;

        if (cont.HttpContext.IsApiController() && cont is SolhigsonApiControllerBase)
        {
            context.Result = new JsonResult(ResponseInfo.FailedResult("Not available in Production"))
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
        else
        {
            context.Result = new ViewResult
            {
                ViewName = "~/Views/Home/Dashboard.cshtml"
            };
            if (cont is SolhigsonMvcControllerBase sCont)
                sCont.SetDisplayMessage("Not available in production", PageMessageType.Error);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    private static void SetFailed(ActionExecutingContext context, string message, string responseCode)
    {
        context.Result = new JsonResult(ResponseInfo.FailedResult(message, responseCode))
        {
            StatusCode = StatusCodes.Status200OK
        };
    }
}