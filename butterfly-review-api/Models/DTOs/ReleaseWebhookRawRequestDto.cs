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
    /// Email field (q43_email43)
    /// </summary>
    [JsonPropertyName("q43_email43")]
    public string? Email { get; set; }

    /// <summary>
    /// Tag number field (q44_tagNo44)
    /// </summary>
    [JsonPropertyName("q44_tagNo44")]
    public string? TagNumber { get; set; }

    /// <summary>
    /// Release date/time field (q20_whatDay)
    /// </summary>
    [JsonPropertyName("q20_whatDay")]
    public JotFormDateDto? Date { get; set; }

    /// <summary>
    /// Notes field (q22_notesYou)
    /// </summary>
    [JsonPropertyName("q22_notesYou")]
    public string? Notes { get; set; }

    /// <summary>
    /// Wind field (q32_estimateThe32)
    /// </summary>
    [JsonPropertyName("q32_estimateThe32")]
    public string? Wind { get; set; }

    /// <summary>
    /// Sex field (q33_malefemale)
    /// </summary>
    [JsonPropertyName("q33_malefemale")]
    public string? Sex { get; set; }

    /// <summary>
    /// Sun field (q34_wasIt34)
    /// </summary>
    [JsonPropertyName("q34_wasIt34")]
    public string? Sun { get; set; }

    /// <summary>
    /// Map Locator field (q35_typeA35)
    /// </summary>
    [JsonPropertyName("q35_typeA35")]
    public string? MapLocator { get; set; }

    /// <summary>
    /// GPS Location field (q36_typeA)
    /// </summary>
    [JsonPropertyName("q36_typeA")]
    public string? GpsLocation { get; set; }

    /// <summary>
    /// Additional fields that may be present in the request
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}
