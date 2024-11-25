using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Solhigson.Framework.Data.Attributes;
using Solhigson.Framework.Data.Caching;

namespace SolhigsonAspnetCoreApp.Domain.Entities;

[AuditInclude]
[Table("Institutions")]
[Index(nameof(Name), IsUnique = true)]
public record Institution : EntityBase, ICachedEntity
{
    [CachedProperty]
    [StringLength(450)]
    [Column(TypeName = "VARCHAR")]
    public string Name { get; set; }

    [CachedProperty] public bool EnablePortalAccess { get; set; }
}