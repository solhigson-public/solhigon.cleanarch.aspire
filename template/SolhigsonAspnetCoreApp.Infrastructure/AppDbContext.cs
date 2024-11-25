using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Solhigson.Framework.EfCore;
using Solhigson.Framework.Identity;
using SolhigsonAspnetCoreApp.Domain.Entities;

namespace SolhigsonAspnetCoreApp.Infrastructure;

[AuditDbContext(Mode = AuditOptionMode.OptIn, IncludeEntityObjects = false, ExcludeTransactionId = true,
    ExcludeValidationResults = true, AuditEventType = "EfCore")]
public class AppDbContext : SolhigsonIdentityDbContext<AppUser>
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Country> Countries { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Institution> Institutions { get; set; }

    
    
    
    
    
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new CustomAuditSaveChangesInterceptor(), new EfCoreCachingSaveChangesInterceptor());
        base.OnConfiguring(optionsBuilder);
    }
    
    protected override void OnModelCreating(ModelBuilder modelbuilder)
    {
        foreach (var relationship in modelbuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
        }

        base.OnModelCreating(modelbuilder);
    }

    public override int SaveChanges()
    {
        UpdateDates();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateDates();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        UpdateDates();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
    {
        UpdateDates();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateDates()
    {
        var insertedEntries = this.ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Added)
            .Select(x => x.Entity);
        foreach (var insertedEntry in insertedEntries)
        {
            if (insertedEntry is EntityBase auditableEntity)
            {
                auditableEntity.Created = auditableEntity.Updated = DateTime.UtcNow;
            }
        }
        
        var modifiedEntries = this.ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Modified)
            .Select(x => x.Entity);

        foreach (var modifiedEntry in modifiedEntries)
        {
            if (modifiedEntry is EntityBase auditableEntity)
            {
                auditableEntity.Updated = DateTime.UtcNow;
            }
        }
    }

}