using System.Text.Json;
using System.Text.Json.Serialization;

namespace tennis_wave_api.Models.DTOs;

public class JotformAnswerRawDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;
    
    [JsonPropertyName("order")]
    public string? Order { get; set; }
    
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("answer")]
    public JsonElement? Answer { get; set; }
    
    [JsonPropertyName("cfname")]
    public string? Cfname { get; set; }
    
    [JsonPropertyName("prettyFormat")]
    public string? PrettyFormat { get; set; }

    [JsonPropertyName("sublabels")]
    public string? Sublabels { get; set; }
}