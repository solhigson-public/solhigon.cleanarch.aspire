using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Solhigson.Framework.Dto;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Utilities.Security;

namespace SolhigsonAspnetCoreApp.Application.Reporting;

[Serializable]
public class ReportInfo
{
    public ReportInfo()
    {
    }

    private ReportInfo(string userEmail, string reportName, PagedSearchParameters searchParameters)
    {
        ReportName = reportName;
        UserEmail = userEmail;
        SearchParameters = searchParameters;
    }

    public string? ReportName { get; set; }
    public string? UserEmail { get; set; }
    
    public PagedSearchParameters? SearchParameters { get; set; }

    public static ReportInfo? Create(string userEmail, string reportName,
        PagedSearchParameters searchParameters)
    {
        return !searchParameters.HasData
            ? null
            : new ReportInfo(userEmail, reportName, searchParameters);
    }
}
