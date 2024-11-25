using System.Text;
using Microsoft.AspNetCore.Html;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class LinkHelper
{
    public static HtmlString Render(string text, string href = null, string onclick = null,
        object attributes = null)
    {
        if (!string.IsNullOrWhiteSpace(onclick)) onclick = $"onclick=\"{onclick}\"";

        href = string.IsNullOrWhiteSpace(href)
            ? "href=\"javascript:void(0);\""
            : $"href=\"{href}\"";
        var sBuilder = new StringBuilder();
        sBuilder.Append($"<a {href} {onclick}\" ");
        if (attributes != null)
            foreach (var obj in attributes.GetType().GetProperties())
                sBuilder.Append($"{obj.Name}=\"{obj.GetValue(attributes, null)}\" ");

        sBuilder.Append($">{text}</a>");
        return new HtmlString(sBuilder.ToString());
    }
}