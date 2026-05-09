using System.Text.Json.Serialization;

namespace tennis_wave_api.Models.DTOs;

public class JotformSubmissionRawDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("form_id")]
    public string FormId { get; set; } = string.Empty;

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("answers")]
    public Dictionary<string, JotformAnswerRawDto> Answers { get; set; } = new();
}