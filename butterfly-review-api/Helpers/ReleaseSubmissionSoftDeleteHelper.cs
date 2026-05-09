using MongoDB.Bson;
using MongoDB.Driver;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Helpers;

/// <summary>
/// Soft-delete uses MongoDB <c>status</c> set to "deleted" (any casing). Public queries must exclude these rows.
/// </summary>
public static class ReleaseSubmissionSoftDeleteHelper
{
    private static readonly BsonRegularExpression SoftDeletedStatusRegex = new(@"^\s*deleted\s*$", "i");

    /// <summary>
    /// Returns true when status is non-empty and equals "deleted" ignoring case and surrounding whitespace.
    /// </summary>
    public static bool IsSoftDeleted(string? status)
    {
        return !string.IsNullOrWhiteSpace(status)
               && string.Equals(status.Trim(), "deleted", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Filter for documents whose status is not soft-deleted (missing/empty/other values are kept).
    /// </summary>
    public static FilterDefinition<ReleaseSubmission> NotSoftDeletedFilter()
    {
        return Builders<ReleaseSubmission>.Filter.Not(
            Builders<ReleaseSubmission>.Filter.Regex(x => x.Status, SoftDeletedStatusRegex));
    }
}
