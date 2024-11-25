using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Solhigson.Framework.Data.Attributes;

namespace SolhigsonAspnetCoreApp.Domain.Entities;

[Index(nameof(InstitutionId))]
public record InstitutionBase : EntityBase
{
    [CachedProperty]
    [StringLength(450)]
    [Column(TypeName = "VARCHAR")]
    public string InstitutionId { get; set; }

    [ForeignKey(nameof(InstitutionId))] public Institution Institution { get; set; }
}