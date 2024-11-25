using System.Text.Json.Serialization;
using Audit.EntityFramework;
using Mapster;
using Solhigson.Framework.Auditing;
using Solhigson.Framework.Utilities;
using SolhigsonAspnetCoreApp.Domain.Interfaces;

namespace SolhigsonAspnetCoreApp.Domain.Dto;

public record AuditLog : IAuditLog
{ 
    public string PartitionKey => Id;
    [JsonPropertyName("id")]
    public string Id { get; set; }

    public DateTime Ttl { get; set; }
    public double Timestamp { get; set; }
    public int? TimeToLive { get; set; }
    public string User { get; set; }
    public string InstitutionId { get; set; }
    public string Institution { get; set; }
    public DateTime StartDate { get; set; }
    public string EventType { get; set; }
    public EntityFrameworkEvent EntityFrameworkEvent { get; set; }
    public IList<AuditEntry> Entries { get; set; }

    public string GetEventType()
    {
        if (EntityFrameworkEvent?.Entries == null || !EntityFrameworkEvent.Entries.Any())
        {
            return EventType;
        }
        if (EntityFrameworkEvent.Entries.Count > 1)
        {
            EventType = "Various";
        }
        else
        {
            var entry = EntityFrameworkEvent.Entries.First();
            EventType = $"{entry.Action} {HelperFunctions.SeparatePascalCaseWords(entry.Name)}";
        }

        return EventType;
    }

    public void SyncEfCoreChanges()
    {
        if (EntityFrameworkEvent is null)
        {
            return;
        }

        Entries = new List<AuditEntry>();
        foreach (var efEntry in EntityFrameworkEvent.Entries)
        {
            string id = null;
            if (efEntry.PrimaryKey.ContainsKey("id"))
            {
                id = efEntry.PrimaryKey["id"].ToString();
            }else if (efEntry.PrimaryKey.ContainsKey("Id"))
            {
                id = efEntry.PrimaryKey["Id"].ToString();
            }

            var auditEntry = new AuditEntry
            {
                Table = efEntry.Table,
                //Table = $"{HelperFunctions.SeparatePascalCaseWords(efEntry.Name)} ({efEntry.Action})",
                PrimaryKey = id
            };
            if (efEntry.Changes?.Any() == true)
            {
                auditEntry.Changes = efEntry.Changes.Adapt<List<AuditChange>>();
            }
            else if(efEntry.ColumnValues?.Any() == true)
            {
                auditEntry.Changes = new List<AuditChange>();
                foreach (var (key, value) in efEntry.ColumnValues)
                {
                    auditEntry.Changes.Add(new AuditChange
                    {
                        ColumnName = key,
                        NewValue = value?.ToString(),
                    });
                }
            }
            Entries.Add(auditEntry);
        }
    }
    
    public void HideSensitiveFields(List<string> protectedFields)
    {
        if (Entries is null || !Entries.Any())
        {
            return;
        }

        foreach (var entry in Entries)
        {
            if (entry.Changes is null || !entry.Changes.Any())
            {
                continue;
            }

            foreach (var change in entry.Changes.Where(change => protectedFields.Contains(change.ColumnName, StringComparer.OrdinalIgnoreCase)))
            {
                change.OriginalValue = "***sensitive***";
                change.NewValue = "***sensitive***(changed)";
            }
        }
    }

}
