using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using SolhigsonAspnetCoreApp.Application.Services;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public class CssJsLinkHelper
{
    public static HtmlString Render(IHtmlHelper helper, string path, HelperFactory.UrlBuilderDelegate urlBuilder,
        bool includeVersion = true,
        string integrity = null, string crossOrigin = null)
    {
        var servicesWrapper = helper.ViewContext.HttpContext
            .RequestServices.GetRequiredService<ServicesWrapper>();
        if (string.IsNullOrWhiteSpace(path)) return null;

        var version = string.Empty;
        if (includeVersion) version = $"?v={servicesWrapper.AppSettings.StylesAndScriptsVersion}";

        if (!string.IsNullOrWhiteSpace(integrity)) integrity = $"integrity={integrity}";

        if (!string.IsNullOrWhiteSpace(crossOrigin)) crossOrigin = $"crossorigin={crossOrigin}";

        path = HelperFactory.GetUrl(path, urlBuilder);
        var isCss = path.EndsWith(".css");
        return isCss
            ? new HtmlString(
                $"<link rel=\"stylesheet\" type=\"text/css\" href=\"{path}{version} {integrity} {crossOrigin}\">")
            : new HtmlString($"<script src=\"{path}{version} {integrity} {crossOrigin}\"></script>");
    }
}