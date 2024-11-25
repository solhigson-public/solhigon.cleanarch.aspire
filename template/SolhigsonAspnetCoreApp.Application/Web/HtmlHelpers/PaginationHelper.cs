using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Solhigson.Framework.Dto;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Infrastructure;
using Solhigson.Framework.Utilities;
using Solhigson.Framework.Utilities.Pluralization;

namespace SolhigsonAspnetCoreApp.Application.Web.HtmlHelpers;

public static class PaginationHelper
{
    private const string PageSize = "pageSize";
    private const string FromDate = "fromdate";
    private const string ToDate = "todate";

    private static readonly EnglishPluralizationService EnglishPluralizationService = new();

    internal static HtmlString Render(IHtmlHelper htmlHelper,
        IEnumerable<short> pageSizes = null, string recordsName = "Record")
    {
        if (htmlHelper?.ViewContext?.HttpContext?.Request?.GetDisplayUrl() is null) return null;

        var pageParamJson = htmlHelper.TempData[Constants.PaginationParameters] as string;
        if (string.IsNullOrWhiteSpace(pageParamJson)) return null;

        if (pageParamJson?.DeserializeFromJson<PagedSearchParameters>() is not
            { TotalRecords: > 0 } searchParameters) return null;

        pageSizes ??= new short[] { 10, 20, 50, 75, 100 };

        var pageSize = searchParameters.PageSize;
        var currentPage = searchParameters.Page;
        var totalRecords = searchParameters.TotalRecords;
        if (totalRecords > 1)
        {
            if (!EnglishPluralizationService.IsPlural(recordsName))
                recordsName = EnglishPluralizationService.Pluralize(recordsName);
        }
        else
        {
            if (!EnglishPluralizationService.IsSingular(recordsName))
                recordsName = EnglishPluralizationService.Singularize(recordsName);
        }

        var totalPages = totalRecords / pageSize;
        if (totalRecords % pageSize > 0) totalPages++;

        var sBuilder = new StringBuilder();
        sBuilder.Append(
            "<div class=\"card-panel pager row valign-wrapper hoverable\" style=\"padding: 10px !important; margin-top: 10px; margin-bottom: 10px;\">");


        var maxNoOfPagesToDisplay = 5;
        var disablePrev = currentPage == 1;
        var startPage = currentPage - maxNoOfPagesToDisplay;
        long endPage = currentPage + maxNoOfPagesToDisplay;
        var disableNext = currentPage == totalPages;
        if (startPage <= 1) startPage = 1;
        //disablePrev = true;
        if (endPage >= totalPages) endPage = totalPages;
        //disableNext = true;

        #region Page size selector

        sBuilder.Append(
            $"<div class=\"col cs-bold\"><a style=\"text-align: center;\" class=\"btn-floating dropdown-trigger waves-effect waves-light\" href=\"javascript:void(0)\" data-target=\"page_selector_dropdown\"> {pageSize} <i class=\"material-icons right\">arrow_drop_down</i></a>&nbsp;&nbsp;records per page");
        sBuilder.Append(
            "<ul id=\"page_selector_dropdown\" class=\"dropdown-content\">");
        foreach (var size in pageSizes)
            sBuilder.AppendFormat(
                $"<li><a href=\"{GetUrl(htmlHelper, 1, size, searchParameters)}\">{size}</a></li>");

        sBuilder.Append("</ul></div>");

        #endregion


        if (pageSize < totalRecords)
        {
            #region Pager

            sBuilder.Append(
                "<div class=\"col\"><ul class=\"pagination\" style=\"margin: 0px;\">");
            sBuilder.Append($"<li class=\"{(disablePrev ? "disabled" : "waves-effect")}\">");
            if (disablePrev)
                sBuilder.Append("<a href=\"#!\"><i class=\"material-icons\">chevron_left</i></i></a></li>");
            else
                sBuilder.Append(
                    $"<a href=\"{GetUrl(htmlHelper, currentPage - 1, pageSize, searchParameters)}\"><i class=\"material-icons\">chevron_left</i></i></a></li>");

            if (startPage > 1)
                sBuilder.Append(
                    $"<li class=\"waves-effect\"><a href=\"{GetUrl(htmlHelper, 1, pageSize, searchParameters)}\">...</a></li>");

            for (var pageNum = startPage; pageNum <= totalPages; pageNum++)
            {
                if (pageNum > endPage)
                {
                    sBuilder.Append(
                        $"<li class=\"waves-effect\"><a href=\"{GetUrl(htmlHelper, totalPages, pageSize, searchParameters)}\">...</a></li>");
                    break;
                }

                if (pageNum == currentPage)
                    sBuilder.Append($"<li class=\"number active\"><a>{pageNum}</a></li>");
                else
                    sBuilder.Append(
                        $"<li class=\"waves-effect number\"><a href=\"{GetUrl(htmlHelper, pageNum, pageSize, searchParameters)}\">{pageNum}</a></li>");
            }

            sBuilder.Append($"<li class=\"{(disableNext ? "disabled" : "waves-effect")}\">");
            if (disableNext)
                sBuilder.Append(
                    "<a  href=\"#!\" style=\"margin-left:-10px\"><i class=\"material-icons\">chevron_right</i></i></a></li>");
            else
                sBuilder.Append(
                    $"<a href=\"{GetUrl(htmlHelper, currentPage + 1, pageSize, searchParameters)}\" style=\"margin-left:2px\"><i class=\"material-icons\">chevron_right</i></a></li>");

            sBuilder.AppendFormat("</ul></div>");
            var startIndex = (currentPage - 1) * pageSize + 1;
            long endIndex = startIndex + (pageSize - 1);
            if (endIndex > totalRecords) endIndex = totalRecords;

            sBuilder.Append(
                $"<div  class=\"col\">Showing&nbsp;<b>{startIndex} to {endIndex}</b>&nbsp;of&nbsp;<b>{totalRecords}</b>&nbsp;entries</div>");

            #endregion
        }
        else
        {
            sBuilder.Append(
                $"<div  class=\"col valign-wrapper\">&nbsp; (<b>{totalRecords}</b>&nbsp;{recordsName})</div>");
        }

        sBuilder.Append("</div>");

        return new HtmlString(sBuilder.ToString());
    }


    private static string GetUrl(IHtmlHelper htmlHelper, long page, int pageSize,
        PagedSearchParameters searchParameters)
    {
        var uri = htmlHelper.ViewContext.HttpContext.Request.GetDisplayUrl();
        var queryString = htmlHelper.ViewContext.HttpContext.Request.QueryString.ToString();
        if (!string.IsNullOrWhiteSpace(queryString)) uri = uri.Replace(queryString, "");

        var queryStrings = new Dictionary<string, string>
        {
            [PageSize] = $"{pageSize}",
            [Constants.PaginationPage] = $"{page}"
        };

        queryStrings.Add(FromDate, searchParameters.FromDate.ToClientTime(DateUtils.DefaultDateFormat));

        queryStrings.Add(ToDate, searchParameters.ToDate.ToClientTime(DateUtils.DefaultDateFormat));

        foreach (var key in searchParameters.Keys) queryStrings.TryAdd(key, searchParameters[key]);

        return QueryHelpers.AddQueryString(uri, queryStrings);
    }
}