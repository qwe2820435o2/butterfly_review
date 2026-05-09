namespace tennis_wave_api.Services.Interfaces;

/// <summary>
/// Service for normalizing tag numbers (converting lowercase to uppercase) in MongoDB collections.
/// </summary>
public interface ITagNumberNormalizationService
{
    /// <summary>
    /// Normalize tag numbers in Release submissions collection.
    /// Converts lowercase tag numbers to uppercase.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of documents updated</returns>
    Task<int> NormalizeReleaseTagNumbersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Normalize tag numbers in Sighting submissions collection.
    /// Converts lowercase tag numbers to uppercase.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of documents updated</returns>
    Task<int> NormalizeSightingTagNumbersAsync(CancellationToken cancellationToken = default);
}
