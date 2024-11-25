using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolhigsonAspnetCoreApp.Domain;
using SolhigsonAspnetCoreApp.Infrastructure;

namespace SolhigsonAspnetCoreApp.Application;

public static class ApplicationAssemblyReference
{
    private static string? _currentVersion;

    public static string CurrentVersion(Assembly webAssembly)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_currentVersion))
            {
                return _currentVersion;
            }
            var contractsVersion = Assembly.GetAssembly(typeof(Constants))?.GetName().Version;
            var persistenceVersion = Assembly.GetAssembly(typeof(InfrastructureAssemblyReference))?.GetName().Version;
            var coreVersion = Assembly.GetAssembly(typeof(ApplicationAssemblyReference))?.GetName().Version;
            var webVersion = webAssembly?.GetName().Version;

            var sb = new StringBuilder();
            sb.Append(contractsVersion?.Major + persistenceVersion?.Major + coreVersion?.Major + webVersion?.Major);
            sb.Append('.');
            sb.Append(contractsVersion?.Minor + persistenceVersion?.Minor + coreVersion?.Minor + webVersion?.Minor);
            sb.Append('.');
            sb.Append(contractsVersion?.Build + persistenceVersion?.Build + coreVersion?.Build + webVersion?.Build);
            sb.Append('.');
            sb.Append(contractsVersion?.Revision + persistenceVersion?.Revision + coreVersion?.Revision + webVersion?.Revision);
            _currentVersion = $"[{sb}]";
            return _currentVersion;
        }
        catch
        {
            _currentVersion = "*.*.*.*";
        }
        return _currentVersion;
    }
}