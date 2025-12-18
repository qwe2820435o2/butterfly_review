using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Data.Interfaces;

/// <summary>
/// Repository for release form submissions.
/// </summary>
public interface IReleaseSubmissionRepository
{
    Task<ReleaseSubmission?> GetByIdAsync(string id);

    Task<ReleaseSubmission?> GetBySubmissionIdAsync(string submissionId);

    Task UpsertBySubmissionIdAsync(ReleaseSubmission entity);

    Task<IReadOnlyList<ReleaseSubmission>> GetByCreatedRangeAsync(
        DateTime? startUtc,
        DateTime? endUtc);

    Task DeleteByIdAsync(string id);
}