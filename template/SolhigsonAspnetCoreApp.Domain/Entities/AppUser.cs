using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Solhigson.Framework.Identity;
using SolhigsonAspnetCoreApp.Domain.Interfaces;

namespace SolhigsonAspnetCoreApp.Domain.Entities;

[Index(nameof(Firstname), nameof(Lastname))]
[Index(nameof(InstitutionId))]
public class AppUser : SolhigsonUser, IInstitutionEntity
{
    [Required]
    [StringLength(256)]
    [Column(TypeName = "VARCHAR")]
    public string Firstname { get; set; }

    [Required]
    [StringLength(256)]
    [Column(TypeName = "VARCHAR")]
    public string Lastname { get; set; }

    [StringLength(256)]
    [Column(TypeName = "VARCHAR")]
    public string OtherNames { get; set; }

    [Required]
    [StringLength(450)]
    [Column(TypeName = "VARCHAR")]
    public string InstitutionId { get; set; }
}