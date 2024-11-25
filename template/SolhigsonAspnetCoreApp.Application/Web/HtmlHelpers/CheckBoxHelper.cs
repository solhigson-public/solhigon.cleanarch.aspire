using System;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Solhigson.Framework.Utilities;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class CheckBoxHelper
{
    public static HtmlString Render(IHtmlHelper helper, bool isRadioButton, string name, string id = null,
        string value = null, string labelName = null,
        bool? @checked = false, object attributes = null, bool setValue = false)
    {
        string isChecked = null;
        id ??= name;
        value ??= id;
        var prop = helper.ViewData.ModelExplorer.GetExplorerForProperty(name);
        if (prop?.Model != null)
        {
            if ((prop.Model is bool chked && chked) || string.Compare(Convert.ToString(prop.Model), value,
                    StringComparison.OrdinalIgnoreCase) == 0)
                isChecked = "checked";
        }
        else
        {
            helper.ViewContext.ModelState.TryGetValue(name, out var modelStateEntry);
            var entryValue = modelStateEntry?.RawValue;
            if (entryValue != null)
            {
                bool.TryParse(Convert.ToString(entryValue), out var entryValueChecked);
                if (entryValueChecked || string.Compare(Convert.ToString(entryValue), value,
                        StringComparison.OrdinalIgnoreCase) == 0)
                    isChecked = "checked";
            }
        }

        if (isRadioButton) setValue = true;

        var type = isRadioButton ? "radio" : "checkbox";
        var sBuilder = new StringBuilder();

        var isCheckedFromId = Convert.ToString(helper.ViewData[id]);
        if (isCheckedFromId == "checked")
            isChecked = isCheckedFromId;
        else if (bool.TryParse(isCheckedFromId, out var ischk) && ischk) isChecked = "checked";

        if (string.IsNullOrWhiteSpace(isChecked) && @checked.GetValueOrDefault()) isChecked = "checked";

        sBuilder.Append($"<label><input type=\"{type}\" id=\"{id}\" name=\"{name}\" {isChecked} ");
        if (setValue) sBuilder.Append($"value=\"{value}\" ");

        if (attributes != null)
            foreach (var obj in attributes.GetType().GetProperties())
                sBuilder.Append($"{obj.Name}=\"{obj.GetValue(attributes, null)}\" ");

        sBuilder.Append(">");
        labelName ??= HelperFunctions.SeparatePascalCaseWords(name);
        sBuilder.Append($"<span>{labelName}</span></label>");

        return new HtmlString(sBuilder.ToString());
    }
}