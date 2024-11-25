using System.Threading.Tasks;
using Solhigson.Framework.Data;
using Solhigson.Framework.Dto;
using SolhigsonAspnetCoreApp.Domain.Interfaces;

namespace SolhigsonAspnetCoreApp.Application.Services.Abstractions;

public interface IAuditLogService
{
    Task<ResponseInfo<PagedList<IAuditLog>>> Search(PagedSearchParameters parameters, string searchText);

    Task<ResponseInfo<IAuditLog>> Get(string id);
}