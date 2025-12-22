using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Data.Interfaces;

/// <summary>
/// Repository for sighting form submissions.
/// </summary>
public interface ISightingSubmissionRepository
{
    Task<SightingSubmission?> GetByIdAsync(string id);

    Task<SightingSubmission?> GetBySubmissionIdAsync(string submissionId);

    Task UpsertBySubmissionIdAsync(SightingSubmission entity);

    Task<IReadOnlyList<SightingSubmission>> GetByCreatedRangeAsync(
        DateTime? startUtc,
        DateTime? endUtc);

    Task<IReadOnlyList<SightingSubmission>> GetByEmailAsync(string email);

    Task<IReadOnlyList<SightingSubmission>> GetByTagNumberAsync(string tagNumber);

    Task<IReadOnlyList<SightingSubmission>> GetAllWithCoordinatesAsync();

    Task DeleteByIdAsync(string id);
}