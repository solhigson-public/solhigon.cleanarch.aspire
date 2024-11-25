using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Solhigson.Framework.Dto;
using Solhigson.Framework.Extensions;

namespace SolhigsonAspnetCoreApp.Application.Web.Filters;

[AttributeUsage(AttributeTargets.Class)]
public class ModelNotNullValidationAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is not ControllerBase cont || !context.HttpContext.IsApiController()) return;

        /*
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method) &&
            !HttpMethods.IsPut(context.HttpContext.Request.Method)) return;

        if (context.ActionArguments.Count == context.ActionDescriptor.Parameters.Count) //parameters are not null
            return;
            */

        if (cont.ModelState.IsValid) return;

        var error = new StringBuilder();
        foreach (var modelError in cont.ModelState.Values.SelectMany(model => model.Errors))
            error.AppendLine(modelError.ErrorMessage);

        context.Result = new JsonResult(ResponseInfo.FailedResult(error.ToString()))
        {
            StatusCode = StatusCodes.Status200OK
        };
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}