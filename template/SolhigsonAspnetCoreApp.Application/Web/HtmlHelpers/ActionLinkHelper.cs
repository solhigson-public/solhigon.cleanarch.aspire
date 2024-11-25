using System.Text;
using Microsoft.AspNetCore.Html;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class ActionLinkHelper
{
    public static HtmlString Render(bool isFixed, bool isButton, string text, string icon, string href = null,
        string onclick = null,
        string bgColor = null, string textColor = null,
        object divAttributes = null, object iAttributes = null)
    {
        var divFixedClass = isFixed ? "fixed-action-btn" : "";
        var buttonLarge = isFixed ? "btn-large" : "";
        var element = isButton ? "button" : "a";
        var type = isButton ? "type=\"submit\"" : "";

        if (!string.IsNullOrWhiteSpace(onclick)) onclick = $"onclick=\"{onclick}\"";

        href = string.IsNullOrWhiteSpace(href)
            ? "href=\"javascript:void(0);\""
            : $"href=\"{href}\"";
        var sBuilder = new StringBuilder();
        sBuilder.Append(
            $"<div class=\"{divFixedClass} tooltipped col\" data-position=\"top\" data-delay=\"50\" data-tooltip=\"{text}\" ");
        if (divAttributes != null)
            foreach (var obj in divAttributes.GetType().GetProperties())
                sBuilder.Append($"{obj.Name}=\"{obj.GetValue(divAttributes, null)}\" ");
        sBuilder.Append(
            $"\"><{element} {type} class=\"{buttonLarge} btn-floating waves-effect waves-light {bgColor}\" {href} {onclick}> ");

        sBuilder.Append($"<i class=\"material-icons {textColor}\" "); // +
        if (iAttributes != null)
            foreach (var obj in iAttributes.GetType().GetProperties())
                sBuilder.Append($"{obj.Name}=\"{obj.GetValue(iAttributes, null)}\" ");
        sBuilder.Append($">{icon}</i></{element}></div>");
        return new HtmlString(sBuilder.ToString());
    }
}