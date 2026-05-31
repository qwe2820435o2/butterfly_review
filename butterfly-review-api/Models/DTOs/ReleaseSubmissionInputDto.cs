using System.ComponentModel.DataAnnotations;

namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Editable business fields for a release submission (admin create/update).
/// The raw Jotform <c>answers</c> map is intentionally not editable.
/// </summary>
public class ReleaseSubmissionInputDto
{
    [Required]
    public string TagNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime? ReleaseDateTimeUtc { get; set; }

    public string? Notes { get; set; }

    public string? Wind { get; set; }

    public string? Sex { get; set; }

    public string? Sun { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Address { get; set; }
}
