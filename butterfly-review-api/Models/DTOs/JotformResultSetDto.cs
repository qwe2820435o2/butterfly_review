using System.Text.Json.Serialization;

namespace tennis_wave_api.Models.DTOs;

public class JotformResultSetDto
{
    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}