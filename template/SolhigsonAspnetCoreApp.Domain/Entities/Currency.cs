using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Solhigson.Framework.Data.Attributes;
using Solhigson.Framework.Data.Caching;

namespace SolhigsonAspnetCoreApp.Domain.Entities;

[AuditInclude]
[Table("Currencies")]
[Index(nameof(AlphabeticCode), IsUnique = true)]
[Index(nameof(NumericCode), IsUnique = true)]
public record Currency : EntityBase, ICachedEntity
{
    [StringLength(50)]
    [CachedProperty]
    [Column(TypeName = "VARCHAR")]
    [Required(ErrorMessage = "Currency name is required")]
    public string Name { get; set; }

    [StringLength(10)]
    [CachedProperty]
    [Required(ErrorMessage = "Currency Alphabetic Code is required")]
    [Column(TypeName = "VARCHAR")]
    public string AlphabeticCode { get; set; }

    [StringLength(10)]
    [CachedProperty]
    [Column(TypeName = "VARCHAR")]
    public string NumericCode { get; set; }

    [StringLength(10)]
    [CachedProperty]
    [Column(TypeName = "NVARCHAR")]
    [Required(ErrorMessage = "Currency Symbol is required")]
    public string Symbol { get; set; }
}