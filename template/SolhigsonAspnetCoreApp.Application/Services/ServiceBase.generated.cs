#nullable enable

namespace SolhigsonAspnetCoreApp.Application.Services
{
    /*
     * Generated by: Solhigson.Framework.efcoretool
     *
     * https://github.com/solhigson-public/solhigson.framework
     * https://www.nuget.org/packages/solhigson.framework.efcoretool
     *
     * This file is ALWAYS overwritten, DO NOT place custom code here
     */
    public abstract partial class ServiceBase : SolhigsonAspnetCoreApp.Application.Services.Abstractions.IServiceBase
    {
        protected SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.IRepositoryWrapper RepositoryWrapper { get; }

        public ServiceBase(SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.IRepositoryWrapper repositoryWrapper)
        {
            RepositoryWrapper = repositoryWrapper;
        }
    }
}