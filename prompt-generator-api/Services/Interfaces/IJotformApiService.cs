using tennis_wave_api.Models.DTOs;

namespace tennis_wave_api.Services.Interfaces;

/// <summary>
/// Jotform API client service
/// </summary>
public interface IJotformApiService
{
    Task<JotformApiResponseDto> GetReleaseSubmissionsPageAsync(int offset, int limit, CancellationToken cancellationToken = default);

    Task<JotformApiResponseDto> GetSightingSubmissionsPageAsync(int offset, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find a submission by tag number in Release form.
    /// </summary>
    Task<JotformSubmissionRawDto?> FindReleaseSubmissionByTagNumberAsync(string tagNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find a submission by tag number in Sighting form.
    /// </summary>
    Task<JotformSubmissionRawDto?> FindSightingSubmissionByTagNumberAsync(string tagNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a submission field in JotForm.
    /// </summary>
    Task UpdateSubmissionFieldAsync(string submissionId, string fieldId, string value, CancellationToken cancellationToken = default);
}