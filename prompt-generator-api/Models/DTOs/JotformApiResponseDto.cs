using System.Text.Json.Serialization;

namespace tennis_wave_api.Models.DTOs;

public class JotformApiResponseDto
{
    [JsonPropertyName("responseCode")]
    public int ResponseCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public List<JotformSubmissionRawDto> Content { get; set; } = new();

    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("info")]
    public object? Info { get; set; }

    [JsonPropertyName("resultSet")]
    public JotformResultSetDto? ResultSet { get; set; }

    [JsonPropertyName("limit-left")]
    public int? LimitLeft { get; set; }
}