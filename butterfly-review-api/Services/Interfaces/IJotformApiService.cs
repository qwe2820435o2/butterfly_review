using tennis_wave_api.Models.DTOs;

namespace tennis_wave_api.Services.Interfaces;

/// <summary>
/// Jotform API client service
/// </summary>
public interface IJotformApiService
{
    Task<JotformApiResponseDto> GetReleaseSubmissionsPageAsync(int offset, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a page of release form submissions for the specified form ID.
    /// </summary>
    Task<JotformApiResponseDto> GetReleaseSubmissionsPageAsync(string releaseFormId, int offset, int limit, CancellationToken cancellationToken = default);

    Task<JotformApiResponseDto> GetSightingSubmissionsPageAsync(int offset, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a page of sighting form submissions for the specified form ID.
    /// </summary>
    Task<JotformApiResponseDto> GetSightingSubmissionsPageAsync(string sightFormId, int offset, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find a submission by tag number in Release form.
    /// </summary>
    Task<JotformSubmissionRawDto?> FindReleaseSubmissionByTagNumberAsync(string tagNumber, CancellationToken cancellationToken = default);

}