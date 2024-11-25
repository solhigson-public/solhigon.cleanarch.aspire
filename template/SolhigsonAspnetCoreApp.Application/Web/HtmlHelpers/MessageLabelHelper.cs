using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Web;
using Encoder = Microsoft.Security.Application.Encoder;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public class MessageLabelHelper
{
    public static HtmlString Render(IHtmlHelper helper, bool renderPlain = false)
    {
        /*
        var systemMessageService =
            helper.ViewContext.HttpContext.RequestServices.GetService<SystemMessageService>();
        */
        var pMessage = helper.ViewContext.TempData.GetDisplayMessages();
        var message = helper.ViewContext.HttpContext.Request.Query["message"];
        if (!string.IsNullOrEmpty(message))
        {
            var type = helper.ViewContext.HttpContext.Request.Query["type"];
            var pType = type.ToString() switch
            {
                "1" => PageMessageType.SystemMessage,
                "3" => PageMessageType.Info,
                _ => PageMessageType.Error
            };

            pMessage.Add(new PageMessage
            {
                Message = HttpUtility.UrlDecode(message),
                Type = pType
            });
        }

        if (renderPlain)
        {
            var pBuilder = new StringBuilder();
            foreach (var pm in pMessage) pBuilder.Append($"{Encoder.HtmlEncode(pm.Message)}<br />");

            return new HtmlString(pBuilder.ToString());
        }

        /*
         *  Display system messages
         */
        /*
        var sysMsgs = systemMessageService?.GetAllCached();
        foreach (var sysMsg in sysMsgs)
        {
            pMessage.Add(new PageMessage
            {
                Message = sysMsg.Message,
                Type = PageMessageType.SystemMessage
            });
        }
        */

        if (pMessage.Count == 0) return null;

        pMessage = pMessage.OrderBy(t => t.Type).ToList();

        var sBuilder = new StringBuilder();


        //< div id = "card-alert" class="card green lighten-5">
        //          <div class="card-content green-text">
        //            <p>SUCCESS : The page has been added.</p>
        //          </div>
        //          <button type = "button" class="close green-text" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">×</span></button>
        //        </div>

        //sBuilder.Append("<div id=\"ui-alert\" class=\"col\">");
        foreach (var obj in pMessage)
            sBuilder.Append(
                $"<div class=\"card-alert card {GetCssTextClass(obj.Type)} lighten-5\">" +
                $"<div class=\"card-content {GetCssTextClass(obj.Type)}-text\">" +
                $"<p><i class=\"material-icons\">{GetIconClass(obj.Type)}</i>&nbsp;&nbsp;" +
                $"{GetMessage(obj)}</p></div>" +
                $"<button type=\"button\" class=\"close {GetCssTextClass(obj.Type)}-text\" data-dismiss=\"alert\" aria-label=\"Close\">" +
                "<span aria-hidden=\"true\">×</span></button>" +
                "</div>");

        //sBuilder.Append("</div>");
        //var script = new StringBuilder();
        //script.Append("<script type=\"text/javascript\">");
        //script.Append("(function(){");
        //script.AppendFormat($"$(\".modal:visible\").prepend('{sBuilder}');");
        //script.Append("})();");
        //script.Append("</script>");
        return new HtmlString(sBuilder.ToString());
        //            return MvcHtmlString.Create(sBuilder.ToString());
    }

    private static string GetMessage(PageMessage obj)
    {
        return obj.EncodeHtml ? Encoder.HtmlEncode(obj.Message) : obj.Message;
    }

    private static string GetIconClass(PageMessageType type)
    {
        var css = "info_outline";
        switch (type)
        {
            case PageMessageType.Info:
                css = "check";
                break;
            case PageMessageType.Error:
                css = "error";
                break;
            case PageMessageType.SystemMessage:
                css = "info_outline";
                break;
        }

        return css;
    }

    private static string GetCssTextClass(PageMessageType type)
    {
        var css = "blue";
        switch (type)
        {
            case PageMessageType.Info:
                css = "green";
                break;
            case PageMessageType.Error:
                css = "red";
                break;
            case PageMessageType.SystemMessage:
                css = "blue";
                break;
        }

        return css;
    }
}