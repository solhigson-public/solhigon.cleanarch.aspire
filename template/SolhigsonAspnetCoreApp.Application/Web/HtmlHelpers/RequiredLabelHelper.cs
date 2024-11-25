using System.Text;
using Microsoft.AspNetCore.Html;
using Solhigson.Framework.Utilities;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class RequiredLabelHelper
{
    public static HtmlString Render(string text, string labelName = null, bool isRequired = true)
    {
        var sBuilder = new StringBuilder();
        sBuilder.Append($"<label class=\"dd-label\"for=\"{text}\">");
        if (isRequired) sBuilder.Append("<span class=\"red-text bold\">*&nbsp;</span>");

        labelName ??= HelperFunctions.SeparatePascalCaseWords(text);
        sBuilder.Append($"{labelName}</label>");

        return new HtmlString(sBuilder.ToString());
    }
}