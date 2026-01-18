using System.Text.Json;
using System.Text.Json.Serialization;

namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Raw request data from JotForm webhook.
/// Contains the parsed JSON data from the rawRequest field.
/// </summary>
public class WebhookRawRequestDto
{
    /// <summary>
    /// Tag number field (q25_tagNumber)
    /// </summary>
    [JsonPropertyName("q25_tagNumber")]
    public string? TagNumber { get; set; }

    /// <summary>
    /// Date field (q30_date)
    /// </summary>
    [JsonPropertyName("q30_date")]
    public JotFormDateDto? Date { get; set; }

    /// <summary>
    /// Address field (q43_typeA43)
    /// </summary>
    [JsonPropertyName("q43_typeA43")]
    public string? Address { get; set; }

    /// <summary>
    /// Email field (q17_email)
    /// </summary>
    [JsonPropertyName("q17_email")]
    public string? Email { get; set; }

    /// <summary>
    /// Additional fields that may be present in the request
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}

/// <summary>
/// JotForm date structure
/// </summary>
public class JotFormDateDto
{
    [JsonPropertyName("day")]
    public string? Day { get; set; }

    [JsonPropertyName("month")]
    public string? Month { get; set; }

    [JsonPropertyName("year")]
    public string? Year { get; set; }

    [JsonPropertyName("timeInput")]
    public string? TimeInput { get; set; }

    [JsonPropertyName("ampm")]
    public string? AmPm { get; set; }
}
