using MongoDB.Driver;
using MongoDB.Bson;
using tennis_wave_api.Models;

namespace tennis_wave_api.Data;

/// <summary>
/// Generic MongoDB database operations helper
/// Provides core CRUD operations for any collection
/// </summary>
public class MongoDbHelper
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbHelper(IMongoClient mongoClient, MongoDbSettings settings)
    {
        _database = mongoClient.GetDatabase(settings.DatabaseName);
        _settings = settings;
    }

    /// <summary>
    /// Get collection with proper typing
    /// </summary>
    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    /// <summary>
    /// Get collection name from configuration
    /// </summary>
    public string GetCollectionName<T>()
    {
        return typeof(T).Name.ToLower() switch
        {
            "user" => _settings.Collections.Users,
            _ => typeof(T).Name.ToLower() + "s" // Default: pluralize the type name
        };
    }

    /// <summary>
    /// Get collection with configured name
    /// </summary>
    public IMongoCollection<T> GetConfiguredCollection<T>()
    {
        var collectionName = GetCollectionName<T>();
        return _database.GetCollection<T>(collectionName);
    }

    /// <summary>
    /// Generate new ObjectId as string
    /// </summary>
    public static string GenerateObjectId()
    {
        return ObjectId.GenerateNewId().ToString();
    }


    /// <summary>
    /// Create filter for document by ID (generic)
    /// </summary>
    public static FilterDefinition<T> GetByIdFilter<T>(string id, System.Linq.Expressions.Expression<Func<T, string>> idProperty)
    {
        return Builders<T>.Filter.Eq(idProperty, id);
    }

    /// <summary>
    /// Execute paginated query (generic)
    /// </summary>
    public async Task<(List<T> Items, long TotalCount)> ExecutePaginatedQueryAsync<T>(
        IMongoCollection<T> collection,
        FilterDefinition<T> filter,
        SortDefinition<T> sort,
        int page,
        int pageSize)
    {
        var totalCount = await collection.CountDocumentsAsync(filter);
        var items = await collection
            .Find(filter)
            .Sort(sort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Check if document exists (generic)
    /// </summary>
    public async Task<bool> DocumentExistsAsync<T>(
        IMongoCollection<T> collection,
        FilterDefinition<T> filter)
    {
        var count = await collection.CountDocumentsAsync(filter);
        return count > 0;
    }

    /// <summary>
    /// Create indexes for a collection (generic)
    /// </summary>
    public async Task CreateIndexesAsync<T>(
        IMongoCollection<T> collection,
        params CreateIndexModel<T>[] indexModels)
    {
        if (indexModels.Length > 0)
        {
            await collection.Indexes.CreateManyAsync(indexModels);
        }
    }

    /// <summary>
    /// Create indexes only if they don't exist (generic)
    /// </summary>
    public async Task CreateIndexesIfNotExistAsync<T>(
        IMongoCollection<T> collection,
        params CreateIndexModel<T>[] indexModels)
    {
        if (indexModels.Length == 0) return;

        // Get existing indexes
        var existingIndexes = await collection.Indexes.ListAsync();
        var existingIndexNames = new HashSet<string>();
        
        await existingIndexes.ForEachAsync(index => 
        {
            if (index.Contains("name"))
            {
                existingIndexNames.Add(index["name"].AsString);
            }
        });

        // Filter out indexes that already exist
        var indexesToCreate = new List<CreateIndexModel<T>>();
        
        foreach (var indexModel in indexModels)
        {
            // Generate index name from the model
            var indexName = GenerateIndexName(indexModel);
            
            if (!existingIndexNames.Contains(indexName))
            {
                indexesToCreate.Add(indexModel);
            }
        }

        // Create only the missing indexes
        if (indexesToCreate.Any())
        {
            await collection.Indexes.CreateManyAsync(indexesToCreate);
        }
    }

    /// <summary>
    /// Generate index name from index model
    /// </summary>
    private static string GenerateIndexName<T>(CreateIndexModel<T> indexModel)
    {
        // This is a simplified approach - in practice, you might want to use
        // the actual index definition to generate the name
        return indexModel.Options?.Name ?? "unknown";
    }

    /// <summary>
    /// Create unique index for a field
    /// </summary>
    public static CreateIndexModel<T> CreateUniqueIndex<T>(
        System.Linq.Expressions.Expression<Func<T, object>> field)
    {
        var indexKeys = Builders<T>.IndexKeys.Ascending(field);
        return new CreateIndexModel<T>(indexKeys, new CreateIndexOptions { Unique = true });
    }

    /// <summary>
    /// Create ascending index for a field
    /// </summary>
    public static CreateIndexModel<T> CreateAscendingIndex<T>(
        System.Linq.Expressions.Expression<Func<T, object>> field)
    {
        var indexKeys = Builders<T>.IndexKeys.Ascending(field);
        return new CreateIndexModel<T>(indexKeys);
    }

    /// <summary>
    /// Create descending index for a field
    /// </summary>
    public static CreateIndexModel<T> CreateDescendingIndex<T>(
        System.Linq.Expressions.Expression<Func<T, object>> field)
    {
        var indexKeys = Builders<T>.IndexKeys.Descending(field);
        return new CreateIndexModel<T>(indexKeys);
    }
}
