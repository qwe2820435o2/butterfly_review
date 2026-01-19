using System.Text.Json;
using System.Text.Json.Serialization;

namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Raw request data from Release Form JotForm webhook.
/// Contains the parsed JSON data from the rawRequest field.
/// </summary>
public class ReleaseWebhookRawRequestDto
{
    /// <summary>
    /// Email field (q17_email)
    /// </summary>
    [JsonPropertyName("q17_email")]
    public string? Email { get; set; }

    /// <summary>
    /// Tag number field (q27_tagNo)
    /// </summary>
    [JsonPropertyName("q27_tagNo")]
    public string? TagNumber { get; set; }

    /// <summary>
    /// Release date/time field (q20_date)
    /// </summary>
    [JsonPropertyName("q20_date")]
    public JotFormDateDto? Date { get; set; }

    /// <summary>
    /// Notes field (q22_notes)
    /// </summary>
    [JsonPropertyName("q22_notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Wind field (q32_wind)
    /// </summary>
    [JsonPropertyName("q32_wind")]
    public string? Wind { get; set; }

    /// <summary>
    /// Sex field (q33_sex)
    /// </summary>
    [JsonPropertyName("q33_sex")]
    public string? Sex { get; set; }

    /// <summary>
    /// Sun field (q34_sun)
    /// </summary>
    [JsonPropertyName("q34_sun")]
    public string? Sun { get; set; }

    /// <summary>
    /// Map Locator field (q35_mapLocator)
    /// </summary>
    [JsonPropertyName("q35_mapLocator")]
    public string? MapLocator { get; set; }

    /// <summary>
    /// GPS Location field (q36_gpsLocation)
    /// </summary>
    [JsonPropertyName("q36_gpsLocation")]
    public string? GpsLocation { get; set; }

    /// <summary>
    /// Additional fields that may be present in the request
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}
