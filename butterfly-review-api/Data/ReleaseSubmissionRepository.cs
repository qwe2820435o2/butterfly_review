using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Data;

/// <summary>
/// MongoDB repository for release form submissions.
/// </summary>
public class ReleaseSubmissionRepository : IReleaseSubmissionRepository
{
    private readonly IMongoCollection<ReleaseSubmission> _collection;
    private readonly MongoDbHelper _dbHelper;

    public ReleaseSubmissionRepository(MongoDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
        _collection = dbHelper.GetConfiguredCollection<ReleaseSubmission>();
        
        // Initialize indexes asynchronously (fire and forget)
        _ = InitializeIndexesAsync();
    }

    /// <summary>
    /// Initialize indexes for optimal query performance
    /// </summary>
    private async Task InitializeIndexesAsync()
    {
        try
        {
            var indexes = new List<CreateIndexModel<ReleaseSubmission>>
            {
                // Index for CreatedAtUtc range queries (YearInReview API)
                new CreateIndexModel<ReleaseSubmission>(
                    Builders<ReleaseSubmission>.IndexKeys.Ascending(x => x.CreatedAtUtc),
                    new CreateIndexOptions { Name = "idx_createdAtUtc" }),

                // Sparse compound index for coordinates query with sorting (Trajectories/all API)
                // Sparse index only indexes documents where Latitude and Longitude exist (not null)
                // This is much more efficient for queries filtering by coordinates
                new CreateIndexModel<ReleaseSubmission>(
                    Builders<ReleaseSubmission>.IndexKeys
                        .Ascending(x => x.Latitude)
                        .Ascending(x => x.Longitude)
                        .Descending(x => x.CreatedAtUtc),
                    new CreateIndexOptions 
                    { 
                        Name = "idx_lat_lng_createdAt",
                        Sparse = true // Only index documents where these fields exist
                    }),

                // Index for Email queries
                new CreateIndexModel<ReleaseSubmission>(
                    Builders<ReleaseSubmission>.IndexKeys.Ascending(x => x.Email),
                    new CreateIndexOptions { Name = "idx_email" }),

                // Index for TagNumber queries
                new CreateIndexModel<ReleaseSubmission>(
                    Builders<ReleaseSubmission>.IndexKeys.Ascending(x => x.TagNumber),
                    new CreateIndexOptions { Name = "idx_tagNumber" }),

                // Unique index for SubmissionId (if not already exists)
                new CreateIndexModel<ReleaseSubmission>(
                    Builders<ReleaseSubmission>.IndexKeys.Ascending(x => x.SubmissionId),
                    new CreateIndexOptions { Name = "idx_submissionId_unique", Unique = true })
            };

            await _dbHelper.CreateIndexesIfNotExistAsync(_collection, indexes.ToArray());
        }
        catch (Exception ex)
        {
            // Log error but don't throw - app should still work without indexes
            Console.WriteLine($"Warning: Failed to create indexes for ReleaseSubmission: {ex.Message}");
        }
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

    /// <summary>
    /// Insert release submission by tag number.
    /// If a document with the same tag number already exists, keep existing data and ignore new data (do not overwrite).
    /// If no document exists, insert the new document.
    /// </summary>
    public async Task UpsertByTagNumberAsync(ReleaseSubmission entity)
    {
        // TagNumber must not be empty
        if (string.IsNullOrWhiteSpace(entity.TagNumber))
        {
            throw new ArgumentException("TagNumber cannot be null or empty for UpsertByTagNumberAsync", nameof(entity));
        }

        var filter = Builders<ReleaseSubmission>.Filter.Eq(x => x.TagNumber, entity.TagNumber);
        
        // Check if document exists
        var existing = await _collection.Find(filter).FirstOrDefaultAsync();
        if (existing != null)
        {
            // Document exists: keep existing data, ignore new data (do not overwrite)
            return;
        }
        
        // Document does not exist: insert new document
        // Generate new ObjectId if Id is null
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = ObjectId.GenerateNewId().ToString();
        }
        
        await _collection.InsertOneAsync(entity);
    }

    public async Task<IReadOnlyList<ReleaseSubmission>> GetByCreatedRangeAsync(
        DateTime? startUtc,
        DateTime? endUtc)
    {
        var filter = ReleaseSubmissionSoftDeleteHelper.NotSoftDeletedFilter();

        if (startUtc.HasValue)
        {
            filter &= Builders<ReleaseSubmission>.Filter.Gte(x => x.CreatedAtUtc, startUtc.Value);
        }

        if (endUtc.HasValue)
        {
            filter &= Builders<ReleaseSubmission>.Filter.Lte(x => x.CreatedAtUtc, endUtc.Value);
        }

        // Use projection to only return fields needed for YearInReview calculation
        var projection = Builders<ReleaseSubmission>.Projection
            .Include(x => x.Id)
            .Include(x => x.TagNumber)
            .Include(x => x.Email)
            .Include(x => x.Address)
            .Include(x => x.Latitude)
            .Include(x => x.Longitude)
            .Include(x => x.ReleaseDateTimeUtc)
            .Include(x => x.CreatedAtUtc);

        var list = await _collection
            .Find(filter)
            .Project<ReleaseSubmission>(projection)
            .SortBy(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task<IReadOnlyList<ReleaseSubmission>> GetByEmailAsync(string email)
    {
        var filter = Builders<ReleaseSubmission>.Filter.And(
            Builders<ReleaseSubmission>.Filter.Eq(x => x.Email, email),
            ReleaseSubmissionSoftDeleteHelper.NotSoftDeletedFilter());

        var list = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task<IReadOnlyList<ReleaseSubmission>> GetByTagNumberAsync(string tagNumber)
    {
        var filter = Builders<ReleaseSubmission>.Filter.And(
            Builders<ReleaseSubmission>.Filter.Eq(x => x.TagNumber, tagNumber),
            ReleaseSubmissionSoftDeleteHelper.NotSoftDeletedFilter());

        var list = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task<IReadOnlyList<ReleaseSubmission>> GetByTagNumberIncludingDeletedAsync(string tagNumber)
    {
        var filter = Builders<ReleaseSubmission>.Filter.Eq(x => x.TagNumber, tagNumber);
        var list = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task<IReadOnlyList<ReleaseSubmission>> GetAllWithCoordinatesAsync()
    {
        // Get all release submissions that have coordinates and are not deleted
        // Use $exists filter which works better with sparse indexes
        // $exists: true already excludes null values, so we don't need $ne: null
        var filter = Builders<ReleaseSubmission>.Filter.And(
            Builders<ReleaseSubmission>.Filter.Exists(x => x.Latitude),
            Builders<ReleaseSubmission>.Filter.Exists(x => x.Longitude),
            Builders<ReleaseSubmission>.Filter.Type(x => x.Latitude, BsonType.Double),
            Builders<ReleaseSubmission>.Filter.Type(x => x.Longitude, BsonType.Double),
            ReleaseSubmissionSoftDeleteHelper.NotSoftDeletedFilter()
        );

        // Use projection to only return fields needed for trajectory calculation
        // This significantly reduces data transfer and memory usage
        var projection = Builders<ReleaseSubmission>.Projection
            .Include(x => x.Id)
            .Include(x => x.TagNumber)
            .Include(x => x.Latitude)
            .Include(x => x.Longitude)
            .Include(x => x.Address)
            .Include(x => x.ReleaseDateTimeUtc)
            .Include(x => x.CreatedAtUtc);

        var list = await _collection
            .Find(filter)
            .Project<ReleaseSubmission>(projection)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task DeleteByIdAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task InsertAsync(ReleaseSubmission entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = ObjectId.GenerateNewId().ToString();
        }
        await _collection.InsertOneAsync(entity);
    }

    public async Task<(List<ReleaseSubmission> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, bool sortDescending = true, string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var filter = ReleaseSubmissionSoftDeleteHelper.NotSoftDeletedFilter();

        // Optional search: match email or tag number (case-insensitive, contains).
        if (!string.IsNullOrWhiteSpace(search))
        {
            var regex = new BsonRegularExpression(
                System.Text.RegularExpressions.Regex.Escape(search.Trim()), "i");
            var searchFilter = Builders<ReleaseSubmission>.Filter.Or(
                Builders<ReleaseSubmission>.Filter.Regex(x => x.Email, regex),
                Builders<ReleaseSubmission>.Filter.Regex(x => x.TagNumber, regex));
            filter = Builders<ReleaseSubmission>.Filter.And(filter, searchFilter);
        }

        var totalCount = (int)await _collection.CountDocumentsAsync(filter);

        var query = _collection.Find(filter);
        query = sortDescending
            ? query.SortByDescending(x => x.CreatedAtUtc)
            : query.SortBy(x => x.CreatedAtUtc);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> SoftDeleteByIdAsync(string id)
    {
        var update = Builders<ReleaseSubmission>.Update.Set(x => x.Status, "DELETED");
        var result = await _collection.UpdateOneAsync(x => x.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task UpdateAsync(ReleaseSubmission entity)
    {
        var result = await _collection.ReplaceOneAsync(
            x => x.Id == entity.Id, entity, new ReplaceOptions { IsUpsert = false });
        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException($"Release submission with ID {entity.Id} not found");
        }
    }
}