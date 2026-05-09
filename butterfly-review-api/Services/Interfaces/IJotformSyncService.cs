using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Services.Interfaces;

/// <summary>
/// Service for syncing Jotform data into MongoDB.
/// </summary>
public interface IJotformSyncService
{
    Task<int> SyncReleaseSubmissionsAsync(string releaseFormId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);

    Task<int> SyncSightingSubmissionsAsync(string sightFormId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
}