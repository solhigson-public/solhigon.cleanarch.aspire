using Hangfire;
using SolhigsonAspnetCoreApp.Application.Services;
using SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions;

namespace SolhigsonAspnetCoreApp.Application.Tasks.NonRecurring;

[AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
public abstract class NonRecurringBackgroundTask : TaskBase
{
    protected NonRecurringBackgroundTask(ServicesWrapper servicesWrapper, IRepositoryWrapper repositoryWrapper) : base(
        servicesWrapper, repositoryWrapper)
    {
    }
}