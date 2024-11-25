using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using SolhigsonAspnetCoreApp.Application.Reporting;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public class DownloadReportHelper
{
    public static HtmlString Render(IHtmlHelper helper, HelperFactory.UrlBuilderDelegate urlBuilder,
        string reportName)
    {
        var url = HelperFactory.GetUrl($"~/download-report-task/{reportName}", urlBuilder);

        return Button.Render(ButtonType.Link, "Download", icon: "file_download",
            attributes: new { onclick = $"downloadReport('{url}')" });
    }
}