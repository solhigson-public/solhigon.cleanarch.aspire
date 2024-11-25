using System;
using Solhigson.Framework.Dto;
using Microsoft.Extensions.DependencyInjection;
using SolhigsonAspnetCoreApp.Application.Services;

namespace SolhigsonAspnetCoreApp.Application.Reporting;

public static class ReportManager
{
    public static void SaveReportInfo(string userEmail, string reportName, PagedSearchParameters searchParameters, IServiceProvider serviceProvider)
    {
        if (!IsValid(userEmail, reportName, serviceProvider, out var cacheKey, out var cacheService))
        {
            return;
        }

        var reportInfo = ReportInfo.Create(userEmail, reportName, searchParameters);
        if (reportInfo is null)
        {
            return;
        }
        
        cacheService?.SetDataAsync(cacheKey, reportInfo, TimeSpan.FromMinutes(5)).Wait();
    }

    
    public static void ClearReportInfo(ReportInfo? reportInfo, IServiceProvider serviceProvider)
    {
        if (reportInfo is null || !IsValid(reportInfo.UserEmail, reportInfo.ReportName, serviceProvider,
                out var cacheKey, out var cacheService))
        {
            return;
        }
        cacheService?.DeleteKeyAsync(cacheKey).Wait();
    }


    public static ReportInfo? GetReportInfo(string email, string reportName, IServiceProvider serviceProvider)
    {
        if (!IsValid(email, reportName, serviceProvider, out var cacheKey, out var cacheService)
            || cacheService is null)
        {
            return null;
        }
        var result = cacheService.GetDataAsync<ReportInfo>(cacheKey).Result;
        return result is { IsSuccessful: true, Data.Value: not null } 
            ? result.Data.Value 
            : null;
    }
    
    private static bool IsValid(string? email, string? reportName, IServiceProvider serviceProvider,
        out string? cacheKey, out RedisCacheService? cacheService)
    {
        cacheKey = null;
        cacheService = null;
        
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(reportName))
        {
            return false;
        }

        cacheService = serviceProvider.GetService<RedisCacheService>();
        if (cacheService is null)
        {
            return false;
        }
        
        cacheKey = $"Report.{reportName}.Download.{email}";
        return true;
    }


}