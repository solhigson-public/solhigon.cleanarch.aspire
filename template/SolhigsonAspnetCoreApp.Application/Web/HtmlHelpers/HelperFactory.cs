using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Solhigson.Framework.Dto;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public class HelperFactory
{
    public delegate string UrlBuilderDelegate(string url);

    private readonly IHtmlHelper _helper;

    public HelperFactory(IHtmlHelper helper)
    {
        _helper = helper;
    }

    public static string GetUrl(string url, UrlBuilderDelegate urlBuilder = null)
    {
        if (string.IsNullOrWhiteSpace(url)) return "#";

        return urlBuilder != null
            ? urlBuilder(url)
            : url;
    }

    public HtmlString MessageLabel(bool renderPlain = false)
    {
        return MessageLabelHelper.Render(_helper, renderPlain);
    }

    public HtmlString NavBar(IList<SolhigsonPermissionDto> collection,
        UrlBuilderDelegate urlBuilder = null,
        string dashboardUrl = "~/start")
    {
        return NavBarHelper.Render(_helper, collection, urlBuilder, dashboardUrl);
    }

    public HtmlString SubmitButton(string text, string name = null,
        string style = null, string id = null,
        object attributes = null, string type = null,
        string icon = null, string cssColor = null, bool isIconPositionedOnRightSide = true,
        string additionalClassAttributes = null, string dataLoaderMessage = null)
    {
        return Button.Render(ButtonType.Submit, text, name, style, id,
            attributes, type, icon, cssColor, isIconPositionedOnRightSide, null, additionalClassAttributes,
            dataLoaderMessage);
    }

    public HtmlString SearchButton()
    {
        return Button.Render(ButtonType.Submit, "Search", icon: "search", isIconPositionedOnRightSide: false);
    }

    public HtmlString ModalSubmitButton(string text = "Save", string name = null,
        string style = null, string id = null,
        object attributes = null, string type = null,
        string icon = null, string cssColor = null, bool isIconPositionedOnRightSide = true,
        string dataLoaderMessage = null)
    {
        return Button.Render(ButtonType.Submit, text, name, style, id,
            attributes, type, icon, cssColor, isIconPositionedOnRightSide, null, "modal-action suppress-loader",
            dataLoaderMessage);
    }

    public HtmlString LinkButton(string text, string href = null, string name = null,
        string style = null, string id = null,
        object attributes = null, string type = null,
        string icon = null, string cssColor = null, bool isIconPositionedOnRightSide = true,
        string additionalClassAttributes = null, string dataLoaderMessage = null)
    {
        return Button.Render(ButtonType.Link, text, name, style, id,
            attributes, type, icon, cssColor, isIconPositionedOnRightSide, href, additionalClassAttributes,
            dataLoaderMessage);
    }

    public HtmlString SpanButton(string text, string href = null, string name = null,
        string style = null, string id = null,
        object attributes = null, string type = null,
        string icon = null, string cssColor = null, bool isIconPositionedOnRightSide = true,
        string additionalClassAttributes = null, string dataLoaderMessage = null)
    {
        return Button.Render(ButtonType.Span, text, name, style, id,
            attributes, type, icon, cssColor, isIconPositionedOnRightSide, href, additionalClassAttributes,
            dataLoaderMessage);
    }

    public HtmlString ModalCloseButton(string text = "Cancel", string href = null, string name = null,
        string style = null, string id = null,
        object attributes = null, string type = null,
        string icon = null, string cssColor = null, bool isIconPositionedOnRightSide = true,
        string dataLoaderMessage = null)
    {
        return Button.Render(ButtonType.Span, text, name, style, id,
            attributes, type, icon, cssColor, isIconPositionedOnRightSide, href,
            "modal-action modal-close left btn-inverted",
            dataLoaderMessage);
    }

    public HtmlString TextBox(string name, string labelName = null, bool isRequired = false, string iconName = null,
        object attributes = null, string id = null, object value = null)
    {
        return TextBoxHelper.Render(_helper, name, labelName, isRequired, iconName, attributes: attributes, id: id,
            value: value);
    }

    public HtmlString DropDownList(string name, IEnumerable<SelectListItem> items, bool isRequired = false,
        string labelName = null)
    {
        var sBuilder = new StringBuilder();
        if (string.IsNullOrWhiteSpace(labelName)) labelName = name;

        var @class = "browser-default";
        if (isRequired) @class += " required";

        using (var writer = new StringWriter())
        {
            _helper.DropDownList(name, items, new { @class }).WriteTo(writer, HtmlEncoder.Default);
            sBuilder.Append(writer);
        }

        sBuilder.Append(RequiredLabel(name, labelName, isRequired));
        return new HtmlString(sBuilder.ToString());
    }

    public HtmlString Email(string name, string labelName = null, bool isRequired = false, string iconName = null,
        object attributes = null, string id = null, object value = null)
    {
        return TextBoxHelper.Render(_helper, name, labelName, isRequired, iconName, attributes: attributes, id: id,
            value: value, type: "email");
    }

    public HtmlString Url(string name, string labelName = null, bool isRequired = false, string iconName = null,
        object attributes = null, string id = null, object value = null)
    {
        return TextBoxHelper.Render(_helper, name, labelName, isRequired, iconName, attributes: attributes, id: id,
            value: value, type: "url");
    }

    public HtmlString Password(string name, string labelName = null, string iconName = null,
        object attributes = null)
    {
        return TextBoxHelper.Render(_helper, name, labelName, true, iconName, "password", attributes);
    }

    public HtmlString Number(string name, string labelName = null, bool isRequired = false, string iconName = null,
        object attributes = null, string id = null, object value = null)
    {
        return TextBoxHelper.Render(_helper, name, labelName, isRequired, iconName, attributes: attributes, id: id,
            value: value, type: "number");
    }


    public HtmlString RequiredLabel(string name, string labelName = null, bool isRequired = true)
    {
        return RequiredLabelHelper.Render(name, labelName, isRequired);
    }

    public HtmlString Date(string name, string label = null)
    {
        return TextBoxHelper.Render(_helper, name, label ?? name,
            false, null,
            attributes: new { @class = "datepicker" });
    }

    public HtmlString FromDate(string label = "From", string type = "datepicker")
    {
        return TextBoxHelper.Render(_helper, "fromdate", label,
            false, null,
            attributes: new { @class = type, placeholder = DateTime.UtcNow.Date.ToString("dd/MM/yyyy") });
    }

    public HtmlString ToDate(string label = "To", string type = "datepicker")
    {
        return TextBoxHelper.Render(_helper, "todate", label,
            false, null,
            attributes: new
            {
                @class = type
                //placeholder = DateTime.UtcNow.Date.AddDays(1).AddMilliseconds(-1).ToString("dd/MM/yyyy")
            });
    }

    public HtmlString FromToDate(string colWidthClass = "s6", string type = "datepicker")
    {
        return new HtmlString($"<div class=\"input-field col {colWidthClass}\">" +
                              FromDate(type: type) +
                              $"</div><div class=\"input-field col {colWidthClass}\">" +
                              ToDate(type: type) +
                              "</div>");
    }

    public HtmlString CheckBox(string name, string labelName = null, string value = null, object attributes = null,
        bool? isChecked = false)
    {
        var setValue = !string.IsNullOrEmpty(value);
        return CheckBoxHelper.Render(_helper, false, name, null, value, labelName, isChecked, attributes, setValue);
    }

    public HtmlString RadioButton(string name, string id = null, string value = null, string labelName = null,
        object attributes = null, bool isChecked = false)
    {
        return CheckBoxHelper.Render(_helper, true, name, id, value, labelName, isChecked, attributes);
    }

    public HtmlString Link(string text, string href = null, string onclick = null, object attributes = null)
    {
        return LinkHelper.Render(text, href, onclick, attributes);
    }

    public HtmlString ActionLink(string text, string icon, string onclick = null, string bgColor = "white",
        string textColor = "grey-text text-darken-2",
        string href = null,
        object attributes = null)
    {
        return ActionLinkHelper.Render(false, false, text, icon, href, onclick, bgColor, textColor, attributes);
    }

    public HtmlString FilterIndicator(string text = "Filter by:", string toolTip = "Show Filter",
        bool iconWidthInherit = true)
    {
        return FilterIndicatorHelper.Render(text, toolTip, iconWidthInherit);
    }

    public HtmlString EditActionLink(string onclick = null, string href = null, string text = "Edit",
        string icon = "mode_edit",
        string bgColor = "white", string textColor = "grey-text text-darken-2",
        object attributes = null)
    {
        return ActionLinkHelper.Render(false, false, text, icon, href, onclick, bgColor, textColor, attributes);
    }

    public HtmlString DeleteActionLink(string onclick = null, string href = null, string bgColor = "white",
        string textColor = "red-text",
        object attributes = null)
    {
        return ActionLinkHelper.Render(false, false, "Delete", "clear", href, onclick, bgColor,
            textColor, attributes);
    }

    public HtmlString ActionLinkFixed(string text, string icon, string onclick = null, string color = null,
        string href = null,
        object attributes = null)
    {
        return ActionLinkHelper.Render(true, false, text, icon, href, onclick, color, null, attributes);
    }

    public HtmlString AddActionLinkFixed(string text, string onclick = null, string color = null,
        string href = null,
        object attributes = null)
    {
        return ActionLinkHelper.Render(true, false, text, "add", href, onclick, color, null, attributes);
    }

    public HtmlString AddActionLinkDouble(string text, string onclick = null, string color = null,
        string href = null,
        object attributes = null)
    {
        var builder = new StringBuilder();
        builder.Append(ActionLinkHelper.Render(true, false, text, "add", href, onclick, color, null, attributes));
        builder.Append(ActionLinkHelper.Render(false, false, text, "add", href, onclick, "grey darken-2", null,
            attributes));
        return new HtmlString(builder.ToString());
    }

    public HtmlString ButtonActionLinkFixed(string text, string icon, string onclick = null, string color = null,
        string href = null,
        object attributes = null)
    {
        return ActionLinkHelper.Render(true, true, text, icon, href, onclick, color, null, attributes);
    }

    public HtmlString TextWithToolTip(string text, string tooltip = null, string color = "#000000",
        int maxLength = 0)
    {
        return TextWithToolTipHelper.Render(text, tooltip, color, maxLength);
    }

    public HtmlString CheckBoxList(IList<SelectListItem> items, string namePrefix, string colStyle = "s12 m4 l3",
        string className = "")
    {
        return CheckBoxListHelper.Render(_helper, items, namePrefix, colStyle, className);
    }

    public HtmlString Pagination(short[] pageSizes = null,
        string recordsName = "Records")
    {
        return PaginationHelper.Render(_helper, pageSizes, recordsName);
    }

    public HtmlString DownloadReportLink(UrlBuilderDelegate urlBuilder, string reportName)
    {
        return DownloadReportHelper.Render(_helper, urlBuilder, reportName);
    }

    /*
    public async Task<IHtmlContent> RenderPartialAsync(ServicesWrapper servicesWrapper, ClaimsPrincipal user,
        string permission,
        string viewName)
    {
        if (!servicesWrapper.IdentityManager.PermissionManager.VerifyPermission(permission, user).IsSuccessful)
        {
            return null;
        }

        return await _helper.PartialAsync(viewName);
    }
    */

    public HtmlString RenderScript(string path, UrlBuilderDelegate urlBuilder, bool includeVersion = true,
        string integrity = null, string crossOrigin = null)
    {
        return CssJsLinkHelper.Render(_helper, path, urlBuilder, includeVersion, integrity, crossOrigin);
    }

    public HtmlString AccordionHeader(string text)
    {
        return FilterIndicatorHelper.RenderAccordionHeader(text);
    }
}