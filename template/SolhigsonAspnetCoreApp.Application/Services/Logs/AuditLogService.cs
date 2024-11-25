using System;
using System.Linq;
using System.Threading.Tasks;
// using Microsoft.Azure.Cosmos;
// using Solhigson.Framework.AzureCosmosDb;
using Solhigson.Framework.Data;
using Solhigson.Framework.Dto;
using Solhigson.Framework.Utilities;
using SolhigsonAspnetCoreApp.Application.Services.Abstractions;
using SolhigsonAspnetCoreApp.Domain.Dto;
using SolhigsonAspnetCoreApp.Domain.Interfaces;

namespace SolhigsonAspnetCoreApp.Application.Services.Logs;

public class AuditLogService : IAuditLogService
{
    // private readonly CosmosDbService _cosmosDbService;
    //
    // public AuditLogService(CosmosClient dbClient, string databaseName, string containerName)
    // {
    //     _cosmosDbService = new CosmosDbService(dbClient, databaseName, containerName);
    // }

    public async Task<ResponseInfo<PagedList<IAuditLog>>> Search(PagedSearchParameters parameters, string searchText)
    {
        throw new NotImplementedException();
        // var (fromDate, toDate) = GetTimestamps(parameters, !string.IsNullOrWhiteSpace(searchText));
        // var query = "select TOP 100 * " +
        //             $"FROM logs c WHERE c._ts >= {fromDate} " +
        //             $"AND c._ts <= {toDate} " +
        //             " ORDER BY c._ts DESC";
        // var items = (await _cosmosDbService.GetItemsAsync<AuditLog>(query)).Items;
        //
        // var results = PagedList.Create(items.Cast<IAuditLog>().ToList(), items.Count,
        //     1, items.Count);
        //
        // return ResponseInfo.SuccessResult(results);
    }

    public async Task<ResponseInfo<IAuditLog>> Get(string id)
    {
        throw new NotImplementedException();
        // var result = await _cosmosDbService.GetItemAsync<AuditLog>(id, id)
        //              ?? new AuditLog
        //              {
        //                  //Description = $"Log with id of [{id}] was not found"
        //              };
        // result.SyncEfCoreChanges();
        // return ResponseInfo.SuccessResult((IAuditLog)result);
    }

    private static (double fromDate, double toDate) GetTimestamps(PagedSearchParameters pagedSearchParameters,
        bool limitToOneDay = false)
    {
        var fromDate = pagedSearchParameters.FromDate.ToUnixTimestamp();
        var toDate = limitToOneDay //limit search to one day in order to minimise cloud resource use
            ? pagedSearchParameters.FromDate.AddDays(1).AddMilliseconds(-1).ToUnixTimestamp()
            : pagedSearchParameters.ToDate.ToUnixTimestamp();

        return (fromDate, toDate);
    }
}