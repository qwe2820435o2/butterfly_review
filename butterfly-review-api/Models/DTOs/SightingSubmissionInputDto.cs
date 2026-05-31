using System.ComponentModel.DataAnnotations;

namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Editable business fields for a sighting submission (admin create/update).
/// The raw Jotform <c>answers</c> map is intentionally not editable.
/// </summary>
public class SightingSubmissionInputDto
{
    [Required]
    public string TagNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public DateTime? SightingDateTimeUtc { get; set; }

    public string? Condition { get; set; }

    public string? DeadOrAlive { get; set; }

    public string? NearbyButterflies { get; set; }

    public string? NearbyPlants { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Address { get; set; }

    public string? HowSunny { get; set; }

    public string? HowWindy { get; set; }
}
