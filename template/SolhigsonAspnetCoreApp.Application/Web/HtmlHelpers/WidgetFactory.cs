using Microsoft.AspNetCore.Mvc.Rendering;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class WidgetFactory
{
    public static HelperFactory CustomHelper(this IHtmlHelper helper)
    {
        return new HelperFactory(helper);
    }
}