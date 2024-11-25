using Solhigson.Framework.Auditing;
using SolhigsonAspnetCoreApp.Domain.MongoDb;

namespace SolhigsonAspnetCoreApp.Domain.Interfaces;

public interface IAuditLog : IMongoDocumentBase
{
    IList<AuditEntry> Entries { get; set; }
    string User { get; set; }
    string InstitutionId { get; set; }
    string Institution { get; set; }
    DateTime StartDate { get; set; }
    string EventType { get; set; }
    string GetEventType();
    void HideSensitiveFields(List<string> protectedFields);
}