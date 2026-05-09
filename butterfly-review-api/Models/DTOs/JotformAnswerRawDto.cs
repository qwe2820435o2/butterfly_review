using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Raw Jotform answer model, with JSON value for deserialization and BSON value for MongoDB storage.
/// </summary>
public class JotformAnswerRawDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("order")]
    public string? Order { get; set; }
    
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    /// <summary>
    /// Raw JSON value from Jotform (not stored directly in MongoDB).
    /// </summary>
    [JsonPropertyName("answer")]
    [BsonIgnore]
    public JsonElement? AnswerJson { get; set; }
    
    /// <summary>
    /// BSON value used for MongoDB storage (what you see in Compass).
    /// </summary>
    [JsonIgnore]
    [BsonElement("answer")]
    public BsonValue? Answer { get; set; }
    
    [JsonPropertyName("cfname")]
    public string? Cfname { get; set; }
    
    [JsonPropertyName("prettyFormat")]
    public string? PrettyFormat { get; set; }

    [JsonPropertyName("sublabels")]
    public string? Sublabels { get; set; }
}