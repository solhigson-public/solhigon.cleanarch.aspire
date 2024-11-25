using System.Threading;
using System.Threading.Tasks;
using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SolhigsonAspnetCoreApp.Infrastructure;

public class CustomAuditSaveChangesInterceptor : AuditSaveChangesInterceptor
{
    public static bool LogChanges { get; set; } = true;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (LogChanges)
        {
            return base.SavingChanges(eventData, result);
        }

        return new InterceptionResult<int>();
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (LogChanges)
        {
            return base.SavedChanges(eventData, result);
        }

        return 0;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (LogChanges)
        {
            base.SaveChangesFailed(eventData);
        }
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (LogChanges)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        return await Task.FromResult(new InterceptionResult<int>());
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (LogChanges)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        return await ValueTask.FromResult(0);
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (LogChanges)
        {
            await base.SaveChangesFailedAsync(eventData, cancellationToken);
        }
    }
}