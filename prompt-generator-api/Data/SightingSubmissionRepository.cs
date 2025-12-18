using MongoDB.Driver;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Data;

/// <summary>
/// MongoDB repository for sighting form submissions.
/// </summary>
public class SightingSubmissionRepository : ISightingSubmissionRepository
{
    private readonly IMongoCollection<SightingSubmission> _collection;

    public SightingSubmissionRepository(MongoDbHelper dbHelper)
    {
        _collection = dbHelper.GetConfiguredCollection<SightingSubmission>();
    }

    public async Task<SightingSubmission?> GetByIdAsync(string id)
    {
        return await _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<SightingSubmission?> GetBySubmissionIdAsync(string submissionId)
    {
        return await _collection
            .Find(x => x.SubmissionId == submissionId)
            .FirstOrDefaultAsync();
    }

    public async Task UpsertBySubmissionIdAsync(SightingSubmission entity)
    {
        var filter = Builders<SightingSubmission>.Filter.Eq(x => x.SubmissionId, entity.SubmissionId);
        await _collection.ReplaceOneAsync(
            filter,
            entity,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<IReadOnlyList<SightingSubmission>> GetByCreatedRangeAsync(
        DateTime? startUtc,
        DateTime? endUtc)
    {
        var filter = Builders<SightingSubmission>.Filter.Empty;

        if (startUtc.HasValue)
        {
            filter &= Builders<SightingSubmission>.Filter.Gte(x => x.CreatedAtUtc, startUtc.Value);
        }

        if (endUtc.HasValue)
        {
            filter &= Builders<SightingSubmission>.Filter.Lte(x => x.CreatedAtUtc, endUtc.Value);
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