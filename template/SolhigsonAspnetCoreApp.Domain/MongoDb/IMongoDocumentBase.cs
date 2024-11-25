namespace SolhigsonAspnetCoreApp.Domain.MongoDb;

public interface IMongoDocumentBase
{
    string? Id { get; set; }
    DateTime Ttl { get; set; }

}