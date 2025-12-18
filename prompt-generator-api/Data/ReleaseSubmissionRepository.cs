using MongoDB.Driver;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Data;

/// <summary>
/// MongoDB repository for release form submissions.
/// </summary>
public class ReleaseSubmissionRepository : IReleaseSubmissionRepository
{
    private readonly IMongoCollection<ReleaseSubmission> _collection;

    public ReleaseSubmissionRepository(MongoDbHelper dbHelper)
    {
        _collection = dbHelper.GetConfiguredCollection<ReleaseSubmission>();
    }

    public async Task<ReleaseSubmission?> GetByIdAsync(string id)
    {
        return await _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<ReleaseSubmission?> GetBySubmissionIdAsync(string submissionId)
    {
        return await _collection
            .Find(x => x.SubmissionId == submissionId)
            .FirstOrDefaultAsync();
    }

    public async Task UpsertBySubmissionIdAsync(ReleaseSubmission entity)
    {
        var filter = Builders<ReleaseSubmission>.Filter.Eq(x => x.SubmissionId, entity.SubmissionId);
        await _collection.ReplaceOneAsync(
            filter,
            entity,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<IReadOnlyList<ReleaseSubmission>> GetByCreatedRangeAsync(
        DateTime? startUtc,
        DateTime? endUtc)
    {
        var filter = Builders<ReleaseSubmission>.Filter.Empty;

        if (startUtc.HasValue)
        {
            filter &= Builders<ReleaseSubmission>.Filter.Gte(x => x.CreatedAtUtc, startUtc.Value);
        }

        if (endUtc.HasValue)
        {
            filter &= Builders<ReleaseSubmission>.Filter.Lte(x => x.CreatedAtUtc, endUtc.Value);
        }

        var list = await _collection
            .Find(filter)
            .SortBy(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task DeleteByIdAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }
}