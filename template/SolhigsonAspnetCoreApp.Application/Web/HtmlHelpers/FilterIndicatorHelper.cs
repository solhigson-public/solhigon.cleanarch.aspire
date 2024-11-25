using System.Text;
using Microsoft.AspNetCore.Html;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class FilterIndicatorHelper
{
    public static HtmlString Render(string text, string toolTip, bool iconWidthInherit)
    {
        var builder = new StringBuilder();
        builder.Append($"<div class=\"col\" style=\"font-weight: bold; width: 100px;\">{text}</div>");
        object attributes = iconWidthInherit
            ? new { style = "width: inherit;" }
            : null;
        builder.Append(ActionLinkHelper.Render(false, false, toolTip, "filter_list",
            divAttributes: new { style = "margin-left: -25px;" }, iAttributes: attributes));
        return new HtmlString(builder.ToString());
    }

    public static HtmlString RenderAccordionHeader(string text)
    {
        return new HtmlString($"<div class=\"collapsible-header panel valign-wrapper\">" +
                              $"<i class=\"material-icons\" style=\"font-size: 30px; width: 20px;\">arrow_drop_down</i>{text}</div>");
    }
}