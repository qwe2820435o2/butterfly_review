using System.Linq;
using MongoDB.Bson;
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
    private readonly MongoDbHelper _dbHelper;

    public SightingSubmissionRepository(MongoDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
        _collection = dbHelper.GetConfiguredCollection<SightingSubmission>();
        
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
            var indexes = new List<CreateIndexModel<SightingSubmission>>
            {
                // Index for CreatedAtUtc range queries (YearInReview API)
                new CreateIndexModel<SightingSubmission>(
                    Builders<SightingSubmission>.IndexKeys.Ascending(x => x.CreatedAtUtc),
                    new CreateIndexOptions { Name = "idx_createdAtUtc" }),

                // Sparse compound index for coordinates query with sorting (Trajectories/all API)
                // Sparse index only indexes documents where Latitude and Longitude exist (not null)
                // This is much more efficient for queries filtering by coordinates
                new CreateIndexModel<SightingSubmission>(
                    Builders<SightingSubmission>.IndexKeys
                        .Ascending(x => x.Latitude)
                        .Ascending(x => x.Longitude)
                        .Descending(x => x.CreatedAtUtc),
                    new CreateIndexOptions 
                    { 
                        Name = "idx_lat_lng_createdAt",
                        Sparse = true // Only index documents where these fields exist
                    }),

                // Index for Email queries
                new CreateIndexModel<SightingSubmission>(
                    Builders<SightingSubmission>.IndexKeys.Ascending(x => x.Email),
                    new CreateIndexOptions { Name = "idx_email" }),

                // Index for TagNumber queries
                new CreateIndexModel<SightingSubmission>(
                    Builders<SightingSubmission>.IndexKeys.Ascending(x => x.TagNumber),
                    new CreateIndexOptions { Name = "idx_tagNumber" }),

                // Unique index for SubmissionId (if not already exists)
                new CreateIndexModel<SightingSubmission>(
                    Builders<SightingSubmission>.IndexKeys.Ascending(x => x.SubmissionId),
                    new CreateIndexOptions { Name = "idx_submissionId_unique", Unique = true })
            };

            await _dbHelper.CreateIndexesIfNotExistAsync(_collection, indexes.ToArray());
        }
        catch (Exception ex)
        {
            // Log error but don't throw - app should still work without indexes
            Console.WriteLine($"Warning: Failed to create indexes for SightingSubmission: {ex.Message}");
        }
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

    public async Task InsertAsync(SightingSubmission entity)
    {
        // Generate new ObjectId if Id is null
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = ObjectId.GenerateNewId().ToString();
        }
        
        await _collection.InsertOneAsync(entity);
    }

    public async Task<IReadOnlyList<SightingSubmission>> GetByCreatedRangeAsync(
        DateTime? startUtc,
        DateTime? endUtc)
    {
        var filter = Builders<SightingSubmission>.Filter.Ne(x => x.Status, "DELETED");

        if (startUtc.HasValue)
        {
            filter &= Builders<SightingSubmission>.Filter.Gte(x => x.CreatedAtUtc, startUtc.Value);
        }

        if (endUtc.HasValue)
        {
            filter &= Builders<SightingSubmission>.Filter.Lte(x => x.CreatedAtUtc, endUtc.Value);
        }

        // Use projection to only return fields needed for YearInReview calculation
        var projection = Builders<SightingSubmission>.Projection
            .Include(x => x.Id)
            .Include(x => x.TagNumber)
            .Include(x => x.Email)
            .Include(x => x.Address)
            .Include(x => x.Latitude)
            .Include(x => x.Longitude)
            .Include(x => x.SightingDateTimeUtc)
            .Include(x => x.CreatedAtUtc);

        var list = await _collection
            .Find(filter)
            .Project<SightingSubmission>(projection)
            .SortBy(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task<IReadOnlyList<SightingSubmission>> GetByEmailAsync(string email)
    {
        var filter = Builders<SightingSubmission>.Filter.And(
            Builders<SightingSubmission>.Filter.Eq(x => x.Email, email),
            Builders<SightingSubmission>.Filter.Ne(x => x.Status, "DELETED")
        );
        var list = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task<IReadOnlyList<SightingSubmission>> GetByTagNumberAsync(string tagNumber)
    {
        var filter = Builders<SightingSubmission>.Filter.And(
            Builders<SightingSubmission>.Filter.Eq(x => x.TagNumber, tagNumber),
            Builders<SightingSubmission>.Filter.Ne(x => x.Status, "DELETED")
        );
        var list = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task<IReadOnlyList<SightingSubmission>> GetAllWithCoordinatesAsync()
    {
        // Get all sighting submissions that have coordinates and are not deleted
        // Use $exists filter which works better with sparse indexes
        // $exists: true already excludes null values, so we don't need $ne: null
        var filter = Builders<SightingSubmission>.Filter.And(
            Builders<SightingSubmission>.Filter.Exists(x => x.Latitude),
            Builders<SightingSubmission>.Filter.Exists(x => x.Longitude),
            Builders<SightingSubmission>.Filter.Type(x => x.Latitude, BsonType.Double),
            Builders<SightingSubmission>.Filter.Type(x => x.Longitude, BsonType.Double),
            Builders<SightingSubmission>.Filter.Ne(x => x.Status, "DELETED")
        );

        // Use projection to only return fields needed for trajectory calculation
        // This significantly reduces data transfer and memory usage
        var projection = Builders<SightingSubmission>.Projection
            .Include(x => x.Id)
            .Include(x => x.TagNumber)
            .Include(x => x.Latitude)
            .Include(x => x.Longitude)
            .Include(x => x.Address)
            .Include(x => x.SightingDateTimeUtc)
            .Include(x => x.CreatedAtUtc);

        var list = await _collection
            .Find(filter)
            .Project<SightingSubmission>(projection)
            .SortByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return list;
    }

    public async Task<IReadOnlyList<SightingSubmission>> GetAllAsync()
    {
        var filter = Builders<SightingSubmission>.Filter.Ne(x => x.Status, "DELETED");

        var list = await _collection
            .Find(filter)
            .SortBy(x => x.TagNumber)
            .ToListAsync();

        return list;
    }

    public async Task DeleteByIdAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<(List<SightingSubmission> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, bool sortDescending = true, string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var filter = NotDeletedFilter();

        // Optional search: match email, tag number or phone (case-insensitive, contains).
        if (!string.IsNullOrWhiteSpace(search))
        {
            var regex = new BsonRegularExpression(
                System.Text.RegularExpressions.Regex.Escape(search.Trim()), "i");
            var searchFilter = Builders<SightingSubmission>.Filter.Or(
                Builders<SightingSubmission>.Filter.Regex(x => x.Email, regex),
                Builders<SightingSubmission>.Filter.Regex(x => x.TagNumber, regex),
                Builders<SightingSubmission>.Filter.Regex(x => x.Phone, regex));
            filter = Builders<SightingSubmission>.Filter.And(filter, searchFilter);
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
        var update = Builders<SightingSubmission>.Update.Set(x => x.Status, "DELETED");
        var result = await _collection.UpdateOneAsync(x => x.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task UpdateAsync(SightingSubmission entity)
    {
        var result = await _collection.ReplaceOneAsync(
            x => x.Id == entity.Id, entity, new ReplaceOptions { IsUpsert = false });
        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException($"Sighting submission with ID {entity.Id} not found");
        }
    }

    private static readonly BsonRegularExpression DeletedStatusRegex = new(@"^\s*deleted\s*$", "i");

    private static FilterDefinition<SightingSubmission> NotDeletedFilter()
    {
        return Builders<SightingSubmission>.Filter.Not(
            Builders<SightingSubmission>.Filter.Regex(x => x.Status, DeletedStatusRegex));
    }
}