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

    Task InsertAsync(SightingSubmission entity);

    Task<IReadOnlyList<SightingSubmission>> GetByCreatedRangeAsync(
        DateTime? startUtc,
        DateTime? endUtc);

    Task<IReadOnlyList<SightingSubmission>> GetByEmailAsync(string email);

    Task<IReadOnlyList<SightingSubmission>> GetByTagNumberAsync(string tagNumber);

    Task<IReadOnlyList<SightingSubmission>> GetAllWithCoordinatesAsync();

    Task DeleteByIdAsync(string id);

    /// <summary>
    /// Admin: paginated list of non-deleted submissions, ordered by creation time.
    /// </summary>
    Task<(List<SightingSubmission> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, bool sortDescending = true, string? search = null);

    /// <summary>
    /// Admin: soft-delete a submission by setting its status to "DELETED".
    /// </summary>
    Task<bool> SoftDeleteByIdAsync(string id);

    /// <summary>
    /// Admin: replace an existing submission by id. Throws if it does not exist.
    /// </summary>
    Task UpdateAsync(SightingSubmission entity);
}