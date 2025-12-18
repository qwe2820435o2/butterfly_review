namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Request body for triggering Jotform data sync.
/// </summary>
public class JotformSyncRequestDto
{
    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }
}