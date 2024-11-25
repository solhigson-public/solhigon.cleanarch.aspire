using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Solhigson.Framework.Dto;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class NavBarHelper
{
    private static string GetName(string name)
    {
        if (name.Length > 21) name = $"{name[..18]}...";

        return name;
    }

    public static HtmlString Render(IHtmlHelper htmlHelper, IEnumerable<SolhigsonPermissionDto> collection,
        HelperFactory.UrlBuilderDelegate urlBuilder = null,
        string dashboardUrl = "~/start")
    {
        var url = "";
        if (htmlHelper.ViewContext.HttpContext?.Request?.GetDisplayUrl() != null)
            url = htmlHelper.ViewContext.HttpContext.Request.GetDisplayUrl().ToLower();

        var sBuilder = new StringBuilder();

        sBuilder.Append(
            "<ul class=\"sidenav sidenav-collapsible leftside-navigation collapsible sidenav-fixed menu-shadow\" " +
            "id=\"slide-out\" data-menu=\"menu-navigation\" " +
            "data-collapsible=\"menu-accordion\">");

        sBuilder.Append(
            $"<li class=\"bold\"><a class=\"waves-effect\" href=\"{HelperFactory.GetUrl("~/logout", urlBuilder)}\">" +
            "<i class=\"material-icons\">keyboard_tab</i><span class=\"menu-title\" data-i18n=\"Logout\">Logout</span></a></li>");

        sBuilder.Append(
            "<li class=\"bold\"><a class=\"waves-effect\" href=\"javascript:void(0)\" onclick=\"changePassword()\">" +
            "<i class=\"material-icons\">security</i><span class=\"menu-title\" data-i18n=\"Change Password\">Change Password</span></a></li>");

        var active = "";
        if (url.EndsWith("/start")) active = "active";

        sBuilder.Append(
            $"<li><a class=\" {active} waves-effect\" href=\"{HelperFactory.GetUrl(dashboardUrl, urlBuilder)}\">" +
            "<i class=\"material-icons\">dashboard</i><span class=\"menu-title\" data-i18n=\"Dashboard\">Dashboard</span></a></li>");

        var index = 0;
        foreach (var function in collection)
        {
            var href = " href=\"Javascript:void(0)\"";
            var classValue = "collapsible-header";
            var onclick = "";
            var children = function.Children.Where(t => t.IsMenu).ToList();
            if (!children.Any())
            {
                classValue = "";
                if (!string.IsNullOrWhiteSpace(function.OnClickFunction))
                {
                    onclick = $"onclick=\"{function.OnClickFunction}\"";
                }
                else if (!string.IsNullOrWhiteSpace(function.Url))
                {
                    var value = HelperFactory.GetUrl(function.Url, urlBuilder);
                    href = $"href=\"{value}\"";
                }
            }

            var activeClass = "";
            if (children.Any(t =>
                    !string.IsNullOrWhiteSpace(t.Url) && url.Contains(t.Url.Replace("~", "").ToLower())))
                activeClass = "active open";

            if (!string.IsNullOrWhiteSpace(function.Url) &&
                url.Contains(function.Url.Replace("~", "").ToLower()))
                active = "active gradient-45deg-green-teal";
            else
                active = "";

            sBuilder.Append(
                $"<li class={activeClass}><a class=\"{active} {classValue} waves-effect\" {href} {onclick}\">" +
                $"<i class=\"material-icons\">{function.Icon}</i><span class=\"menu-title\" data-i18n=\"{function.Description}\">{GetName(function.Description)}</span></a>");

            if (children.Any())
            {
                sBuilder.Append("<div class=\"collapsible-body\">");
                sBuilder.Append("<ul class=\"collapsible collapsible-sub\" data-collapsible=\"accordion\">");
                foreach (var child in children)
                {
                    var childClass = "";
                    var color = "";
                    if (!string.IsNullOrWhiteSpace(child.Url) &&
                        url.Contains(child.Url.Replace("~", "").ToLower()))
                    {
                        childClass = "active";
                        color = "gradient-45deg-green-teal";
                    }

                    var value = string.IsNullOrWhiteSpace(child.OnClickFunction)
                        ? $"href=\"{HelperFactory.GetUrl(child.Url, urlBuilder)}\""
                        : $"onclick=\"{child.OnClickFunction}\"";

                    sBuilder.Append($"<li><a class=\"{childClass} {color}\" {value}>" +
                                    $"<i class=\"material-icons\">radio_button_unchecked</i><span data-i18n=\"{child.Description}\">{GetName(child.Description)}</span></a></li>");
                }

                sBuilder.Append("</ul>");
                sBuilder.Append("</div>");
            }

            index++;

            sBuilder.Append("</li>");
        }

        sBuilder.Append("</ul>");

        return new HtmlString(sBuilder.ToString());
    }

    private static string GetOnClickFunction(string function)
    {
        if (string.IsNullOrWhiteSpace(function)) return string.Empty;

        return $"onclick=\"{function}\"";
    }
}