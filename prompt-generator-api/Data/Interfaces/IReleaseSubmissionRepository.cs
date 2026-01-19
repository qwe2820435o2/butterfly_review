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

    Task<IReadOnlyList<ReleaseSubmission>> GetByEmailAsync(string email);

    Task<IReadOnlyList<ReleaseSubmission>> GetByTagNumberAsync(string tagNumber);

    Task<IReadOnlyList<ReleaseSubmission>> GetAllWithCoordinatesAsync();

    Task DeleteByIdAsync(string id);

    Task InsertAsync(ReleaseSubmission entity);
}