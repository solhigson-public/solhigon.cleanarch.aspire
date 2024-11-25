using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Solhigson.Framework.Dto;
using Solhigson.Framework.Extensions;

namespace SolhigsonAspnetCoreApp.Application.Web.Filters;

public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        try
        {
            if (context.Exception == null) return;

            this.ELogError(context.Exception);
            if (context.HttpContext.IsApiController())
                context.Result = new JsonResult(ResponseInfo.FailedResult("Internal Server Error"))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            else
                context.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/_500.cshtml"
                };
            context.ExceptionHandled = true;
        }
        catch (Exception e)
        {
            this.ELogError(e, $"Exception thrown in {typeof(HttpResponseExceptionFilter).FullName}");
        }
    }

    public int Order { get; } = int.MaxValue - 10;
}