using Solhigson.Framework.Infrastructure.Dependency;
using SolhigsonAspnetCoreApp.Application.Services;
using SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions;

namespace SolhigsonAspnetCoreApp.Application.Tasks;

public abstract class TaskBase : IDependencyInject
{
    public TaskBase(ServicesWrapper servicesWrapper, IRepositoryWrapper repositoryWrapper)
    {
        ServicesWrapper = servicesWrapper;
        RepositoryWrapper = repositoryWrapper;
    }

    protected ServicesWrapper ServicesWrapper { get; }
    protected IRepositoryWrapper RepositoryWrapper { get; }
}