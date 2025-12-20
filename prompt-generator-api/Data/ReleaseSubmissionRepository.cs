using MongoDB.Bson;
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
        
        // Check if document exists
        var existing = await _collection.Find(filter).FirstOrDefaultAsync();
        if (existing != null)
        {
            // Document exists: MUST use existing Id (immutable field)
            if (!string.IsNullOrEmpty(existing.Id))
            {
                entity.Id = existing.Id;
            }
            else
            {
                // Existing document has null Id (corrupted data): delete and re-insert
                await _collection.DeleteOneAsync(filter);
                entity.Id = ObjectId.GenerateNewId().ToString();
            }
        }
        else
        {
            // New document: generate new ObjectId if Id is null
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = ObjectId.GenerateNewId().ToString();
            }
        }
        
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

    public async Task<IReadOnlyList<ReleaseSubmission>> GetByEmailAsync(string email)
    {
        var filter = Builders<ReleaseSubmission>.Filter.Eq(x => x.Email, email);
        var list = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task<IReadOnlyList<ReleaseSubmission>> GetByTagNumberAsync(string tagNumber)
    {
        var filter = Builders<ReleaseSubmission>.Filter.Eq(x => x.TagNumber, tagNumber);
        var list = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task DeleteByIdAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }
}