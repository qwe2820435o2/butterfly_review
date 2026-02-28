namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Request body for triggering Jotform data sync.
/// </summary>
public class JotformSyncRequestDto
{
    /// <summary>
    /// Jotform Release form ID to sync (e.g. 242685694427874).
    /// </summary>
    public string ReleaseFormId { get; set; } = string.Empty;

    /// <summary>
    /// Jotform Sighting form ID to sync.
    /// </summary>
    public string SightFormId { get; set; } = string.Empty;

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }
}