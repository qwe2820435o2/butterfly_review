using MongoDB.Bson.Serialization.Attributes;

namespace tennis_wave_api.Models.Entities;

/// <summary>
/// Tagged butterfly release submission
/// </summary>
public class ReleaseSubmission : JotformSubmissionBase
{
    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("tagNumber")]
    public string TagNumber { get; set; } = string.Empty;

    [BsonElement("releaseDateTimeUtc")]
    public DateTime? ReleaseDateTimeUtc { get; set; }

    [BsonElement("releaseDatePretty")]
    public string? ReleaseDatePretty { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("wind")]
    public string? Wind { get; set; }

    [BsonElement("sex")]
    public string? Sex { get; set; }

    [BsonElement("sun")]
    public string? Sun { get; set; }

    [BsonElement("latitude")]
    public double? Latitude { get; set; }

    [BsonElement("longitude")]
    public double? Longitude { get; set; }

    [BsonElement("address")]
    public string? Address { get; set; }

    [BsonElement("mapLocatorRaw")]
    public string? MapLocatorRaw { get; set; }

    [BsonElement("gpsLocationRaw")]
    public string? GpsLocationRaw { get; set; }
}