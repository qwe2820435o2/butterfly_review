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
    /// Tag number field (q47_tagNumber47)
    /// </summary>
    [JsonPropertyName("q47_tagNumber47")]
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
    /// Email field (q48_email48)
    /// </summary>
    [JsonPropertyName("q48_email48")]
    public string? Email { get; set; }

    /// <summary>
    /// Name field (q19_yourName) - object with first and last
    /// </summary>
    [JsonPropertyName("q19_yourName")]
    public JotFormNameDto? Name { get; set; }

    /// <summary>
    /// Phone number field (q20_phoneNumber) - object with full
    /// </summary>
    [JsonPropertyName("q20_phoneNumber")]
    public JotFormPhoneDto? Phone { get; set; }

    /// <summary>
    /// Condition field (q24_whatWas24)
    /// </summary>
    [JsonPropertyName("q24_whatWas24")]
    public string? Condition { get; set; }

    /// <summary>
    /// Dead or alive field (q28_deadOr)
    /// </summary>
    [JsonPropertyName("q28_deadOr")]
    public string? DeadOrAlive { get; set; }

    /// <summary>
    /// How sunny field (q31_howSunny)
    /// </summary>
    [JsonPropertyName("q31_howSunny")]
    public string? HowSunny { get; set; }

    /// <summary>
    /// How windy field (q32_wasIt32)
    /// </summary>
    [JsonPropertyName("q32_wasIt32")]
    public string? HowWindy { get; set; }

    /// <summary>
    /// Nearby butterflies field (q27_wereThere27)
    /// </summary>
    [JsonPropertyName("q27_wereThere27")]
    public string? NearbyButterflies { get; set; }

    /// <summary>
    /// Nearby plants field (q14_whatKind14)
    /// </summary>
    [JsonPropertyName("q14_whatKind14")]
    public string? NearbyPlants { get; set; }

    /// <summary>
    /// Additional fields that may be present in the request
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}

/// <summary>
/// JotForm name structure
/// </summary>
public class JotFormNameDto
{
    [JsonPropertyName("first")]
    public string? First { get; set; }

    [JsonPropertyName("last")]
    public string? Last { get; set; }
}

/// <summary>
/// JotForm phone structure
/// </summary>
public class JotFormPhoneDto
{
    [JsonPropertyName("full")]
    public string? Full { get; set; }
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
