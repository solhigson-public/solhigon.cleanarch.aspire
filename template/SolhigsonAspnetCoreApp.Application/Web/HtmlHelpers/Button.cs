using System.Text;
using Microsoft.AspNetCore.Html;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

internal enum ButtonType
{
    Submit,
    Link,
    Span
}

public static class Button
{
    internal static HtmlString Render(ButtonType buttonType, string text, string name = null,
        string style = null, string id = null,
        object attributes = null, string type = null,
        string icon = null, string cssColor = null, bool isIconPositionedOnRightSide = true,
        string href = null, string additionalClassAttributes = null, string dataLoaderMessage = null)
    {
        var sBuilder = new StringBuilder();
        if (string.IsNullOrWhiteSpace(cssColor))
        {
            //              cssColor = "light-blue lighten-3";
        }

        if (!string.IsNullOrWhiteSpace(dataLoaderMessage))
            dataLoaderMessage = $"data-loader-msg=\"{dataLoaderMessage}\"";

        if (string.IsNullOrWhiteSpace(name)) name = text.Replace(" ", "").ToLowerInvariant();

        var elementType = "button";
        switch (buttonType)
        {
            case ButtonType.Link:
                elementType = "a";
                break;
            case ButtonType.Span:
                elementType = "span";
                break;
        }

        var elementId = id ?? name;

        sBuilder.Append($"<{elementType} ");

        if (buttonType == ButtonType.Link)
        {
            href ??= "javascript:void(0);";
            sBuilder.Append($"href=\"{href}\" ");
        }

        if (buttonType == ButtonType.Submit) sBuilder.Append("type=\"submit\" ");
        sBuilder.Append($"name =\"{name}\" id=\"{elementId}\" value=\"{name}\" ");

        if (!string.IsNullOrWhiteSpace(style)) sBuilder.Append($"style=\"{style}\" ");

        if (attributes != null)
            foreach (var obj in attributes.GetType().GetProperties())
                sBuilder.Append($"{obj.Name}=\"{obj.GetValue(attributes, null)}\" ");

        sBuilder.Append(
            $"class=\"btn waves-effect waves-light {cssColor} {type} {additionalClassAttributes}\" {dataLoaderMessage}>{text}");

        if (icon != null)
        {
            var position = isIconPositionedOnRightSide ? "right" : "left";
            sBuilder.Append($"<i class=\"material-icons {position}\">{icon}</i>");
        }

        sBuilder.Append($"</{elementType}>");
        return new HtmlString(sBuilder.ToString());
    }
}