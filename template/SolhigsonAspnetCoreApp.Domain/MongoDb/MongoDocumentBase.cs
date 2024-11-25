using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Solhigson.Framework.Utilities;

namespace SolhigsonAspnetCoreApp.Domain.MongoDb;

public record MongoDocumentBase : IMongoDocumentBase
{
    public string? Id { get; set; }
        
    [JsonPropertyName("_ttl")]
    [JsonProperty("_ttl")]
    public DateTime Ttl { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public double TimeToLive
    {
        get => Ttl.ToUnixTimestamp();
        set => Ttl = value.FromUnixTimestamp();
    }
}