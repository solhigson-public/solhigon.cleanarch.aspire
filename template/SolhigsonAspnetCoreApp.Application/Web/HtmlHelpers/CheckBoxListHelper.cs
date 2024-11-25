using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class CheckBoxListHelper
{
    public static IEnumerable<SelectListItem> GetCheckBoxItems(HttpRequest request, string namePrefix)
    {
        return from string key in request.Form.Keys
            where key.StartsWith(namePrefix)
            select new SelectListItem { Selected = true, Value = request.Form[key] };
    }

    public static IEnumerable<string> GetSelectedItems(HttpRequest request, string namePrefix)
    {
        return from string key in request.Form.Keys
            where key.StartsWith(namePrefix)
            select request.Form[key].ToString();
    }


    public static HtmlString Render(IHtmlHelper helper, IList<SelectListItem> items, string namePrefix,
        string colStyle = "s12 m4 l3", string className = "")
    {
        if (items == null) return null;

        if (helper.ViewData[namePrefix] != null)
            if (helper.ViewData[namePrefix] is IList<SelectListItem> selectedItems)
                foreach (var mainItem in items)
                foreach (var selectedItem in selectedItems)
                    if (string.Compare(selectedItem.Value, mainItem.Value, StringComparison.OrdinalIgnoreCase) == 0)
                        mainItem.Selected = true;

        var sBuilder = new StringBuilder();
        var id = 1;
        foreach (var item in items)
        {
            sBuilder.AppendFormat(
                "<div style=\"padding: 10px;\" class=\"col {5}\"><label><input type=\"checkbox\" class=\"{6}\" id=\"{4}{0}\" name=\"{4}{0}\" value=\"{2}\" {3}/><span>{1}</span></label></div>",
                id,
                item.Text,
                item.Value,
                item.Selected ? "checked=\"true\"" : "",
                namePrefix,
                colStyle,
                className);
            id++;
        }

        return new HtmlString(sBuilder.ToString());
    }
}