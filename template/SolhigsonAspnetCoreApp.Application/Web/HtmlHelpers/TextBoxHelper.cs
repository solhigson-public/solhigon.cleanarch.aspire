using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Solhigson.Framework.Utilities;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class TextBoxHelper
{
    public static HtmlString Render(IHtmlHelper helper, string name, string labelName, bool isRequired,
        string iconName,
        string type = "text", object attributes = null, string id = null, object value = null)
    {
        var prop = helper.ViewData.ModelExplorer.GetExplorerForProperty(name);
        var inputValue = prop?.Model;
        if (inputValue == null)
        {
            helper.ViewContext.ModelState.TryGetValue(name, out var modelStateEntry);
            inputValue = modelStateEntry?.RawValue;
        }

        if (string.IsNullOrEmpty($"{inputValue}")) inputValue = value;

        id ??= name;

        var sBuilder = new StringBuilder();
        if (string.IsNullOrWhiteSpace(type)) type = "text";

        var labelClass = "";
        if (!string.IsNullOrWhiteSpace(iconName))
        {
            sBuilder.Append($"<i class=\"material-icons prefix pt-2\">{iconName}</i>");
            labelClass = "pl-2";
        }

        sBuilder.Append($"<input type=\"{type}\" id=\"{id}\" name=\"{name}\" value=\"{inputValue}\" ");
        if (attributes != null)
            foreach (var obj in attributes.GetType().GetProperties())
                sBuilder.Append($"{obj.Name}=\"{obj.GetValue(attributes, null)}\" ");

        if (isRequired) sBuilder.Append(" required ");
        sBuilder.Append("> ");
        sBuilder.Append($"<label for=\"{name}\" class=\"{labelClass}\">");
        if (isRequired) sBuilder.Append("<span class=\"red-text bold\">*&nbsp;</span>");

        labelName ??= HelperFunctions.SeparatePascalCaseWords(name);
        sBuilder.Append($"{labelName}</label>");
        return new HtmlString(sBuilder.ToString());
    }
}