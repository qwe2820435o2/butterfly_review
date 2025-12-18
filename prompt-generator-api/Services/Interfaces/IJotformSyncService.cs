using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Services.Interfaces;

/// <summary>
/// Service for syncing Jotform data into MongoDB.
/// </summary>
public interface IJotformSyncService
{
    Task<int> SyncReleaseSubmissionsAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);

    Task<int> SyncSightingSubmissionsAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
}