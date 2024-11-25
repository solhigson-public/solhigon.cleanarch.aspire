using Solhigson.Framework.Infrastructure.Dependency;

namespace SolhigsonAspnetCoreApp.Application.Services;

public partial class ServiceBase : IDependencyInject
{
    public ServicesWrapper ServicesWrapper { get; set; }
}