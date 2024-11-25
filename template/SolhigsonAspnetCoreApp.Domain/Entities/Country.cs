using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Solhigson.Framework.Data.Attributes;
using Solhigson.Framework.Data.Caching;

namespace SolhigsonAspnetCoreApp.Domain.Entities;

[AuditInclude]
[Table("Countries")]
[Index(nameof(Code), IsUnique = true)]
public record Country : EntityBase, ICachedEntity
{
    [StringLength(10)]
    [Required(ErrorMessage = "Country Code is required")]
    [CachedProperty]
    [Column(TypeName = "VARCHAR")]
    public string Code { get; set; }

    [StringLength(450)]
    [Required(ErrorMessage = "Country name is required")]
    [CachedProperty]
    [Column(TypeName = "VARCHAR")]
    public string Name { get; set; }

    [Required] public bool Enabled { get; set; }
}