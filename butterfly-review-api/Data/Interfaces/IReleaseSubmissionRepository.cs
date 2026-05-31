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

    Task UpsertByTagNumberAsync(ReleaseSubmission entity);

    Task<IReadOnlyList<ReleaseSubmission>> GetByCreatedRangeAsync(
        DateTime? startUtc,
        DateTime? endUtc);

    Task<IReadOnlyList<ReleaseSubmission>> GetByEmailAsync(string email);

    Task<IReadOnlyList<ReleaseSubmission>> GetByTagNumberAsync(string tagNumber);

    /// <summary>
    /// Same as <see cref="GetByTagNumberAsync"/> but includes submissions with status DELETED (e.g. webhook processing).
    /// </summary>
    Task<IReadOnlyList<ReleaseSubmission>> GetByTagNumberIncludingDeletedAsync(string tagNumber);

    Task<IReadOnlyList<ReleaseSubmission>> GetAllWithCoordinatesAsync();

    Task DeleteByIdAsync(string id);

    Task InsertAsync(ReleaseSubmission entity);

    /// <summary>
    /// Admin: paginated list of non-soft-deleted submissions, ordered by creation time.
    /// </summary>
    Task<(List<ReleaseSubmission> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, bool sortDescending = true, string? search = null);

    /// <summary>
    /// Admin: soft-delete a submission by setting its status to "deleted".
    /// </summary>
    Task<bool> SoftDeleteByIdAsync(string id);

    /// <summary>
    /// Admin: replace an existing submission by id. Throws if it does not exist.
    /// </summary>
    Task UpdateAsync(ReleaseSubmission entity);
}