using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Audit.EntityFramework;
using MassTransit;
using Solhigson.Framework.Data.Attributes;

namespace SolhigsonAspnetCoreApp.Domain.Entities;

public record EntityBase
{
    protected EntityBase()
    {
        Id = NewId.NextSequentialGuid().ToString();
    }

    [Key]
    [AuditIgnore]
    [CachedProperty]
    [StringLength(450)]
    [Column(TypeName = "VARCHAR")]
    public string Id { get; set; }
    
    [AuditIgnore]
    [Column(TypeName = "DATETIME")]
    public DateTime Created { get; set; }
    
    [AuditIgnore]
    [Column(TypeName = "DATETIME")]
    public DateTime Updated { get; set; }

}