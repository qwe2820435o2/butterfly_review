using MongoDB.Bson.Serialization.Attributes;

namespace tennis_wave_api.Models.Entities;

/// <summary>
/// Butterfly sighting submission
/// </summary>
public class SightingSubmission : JotformSubmissionBase
{
    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("tagNumber")]
    public string TagNumber { get; set; } = string.Empty;

    [BsonElement("sightingDateTimeUtc")]
    public DateTime? SightingDateTimeUtc { get; set; }

    [BsonElement("sightingDatePretty")]
    public string? SightingDatePretty { get; set; }

    [BsonElement("condition")]
    public string? Condition { get; set; }

    [BsonElement("deadOrAlive")]
    public string? DeadOrAlive { get; set; }

    [BsonElement("nearbyButterflies")]
    public string? NearbyButterflies { get; set; }

    [BsonElement("nearbyPlants")]
    public string? NearbyPlants { get; set; }

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
    
    [BsonElement("howSunny")]
    public string? HowSunny { get; set; }

    [BsonElement("howWindy")]
    public string? HowWindy { get; set; }
}