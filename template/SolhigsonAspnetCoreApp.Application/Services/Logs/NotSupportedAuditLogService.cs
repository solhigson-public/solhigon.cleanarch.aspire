using System.Threading.Tasks;
using Solhigson.Framework.Data;
using Solhigson.Framework.Dto;
using SolhigsonAspnetCoreApp.Application.Services.Abstractions;
using SolhigsonAspnetCoreApp.Domain.Interfaces;

namespace SolhigsonAspnetCoreApp.Application.Services.Logs;

public class NotSupportedAuditLogService : IAuditLogService
{
    public async Task<ResponseInfo<PagedList<IAuditLog>>> Search(PagedSearchParameters parameters, string searchText)
    {
        return await Task.FromResult(ResponseInfo.FailedResult<PagedList<IAuditLog>>("Not Supported"));
    }

    public async Task<ResponseInfo<IAuditLog>> Get(string id)
    {
        return await Task.FromResult(ResponseInfo.FailedResult<IAuditLog>("Not Supported"));
    }
}