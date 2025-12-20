namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Result of Jotform sync operation.
/// </summary>
public class JotformSyncResultDto
{
    /// <summary>
    /// Total count of submissions in Jotform (from API).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Number of records actually upserted to MongoDB (within time range).
    /// </summary>
    public int UpsertedCount { get; set; }
}

