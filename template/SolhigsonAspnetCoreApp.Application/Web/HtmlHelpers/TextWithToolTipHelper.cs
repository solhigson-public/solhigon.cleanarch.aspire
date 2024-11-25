using System.Text;
using Microsoft.AspNetCore.Html;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class TextWithToolTipHelper
{
    public static HtmlString Render(string text, string tooltip = null, string color = "#000000", int maxLength = 0)
    {
        var sBuilder = new StringBuilder();
        if (string.IsNullOrWhiteSpace(tooltip)) tooltip = text;

        if (maxLength > 0)
        {
            //text = HelperFunctions.TruncateWithLeadingDots(text, maxLength);
        }

        sBuilder.Append($"<span title=\"{tooltip}\" style=\"color: {color}\">{text}</span>");
        return new HtmlString(sBuilder.ToString());
    }
}