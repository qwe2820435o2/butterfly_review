using MongoDB.Driver;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Data;

public class UserRepository : IUserRepository
{
    private readonly MongoDbHelper _dbHelper;

    public UserRepository(MongoDbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        // Generate ObjectId if not provided
        if (string.IsNullOrEmpty(user.Id))
        {
            user.Id = MongoDbHelper.GenerateObjectId();
        }
        
        var users = _dbHelper.GetConfiguredCollection<User>();
        await users.InsertOneAsync(user);
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        var users = _dbHelper.GetConfiguredCollection<User>();
        var filter = MongoDbHelper.GetByIdFilter<User>(user.Id, u => u.Id);
        var result = await users.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = false });
        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException($"User with ID {user.Id} not found");
        }
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var users = _dbHelper.GetConfiguredCollection<User>();
        var filter = Builders<User>.Filter.Eq(u => u.Email, email.ToLowerInvariant());
        var user = await users.Find(filter).FirstOrDefaultAsync();
        return user;
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null)
    {
        var users = _dbHelper.GetConfiguredCollection<User>();
        var filter = Builders<User>.Filter.Eq(u => u.Email, email.ToLowerInvariant());
        
        if (excludeUserId.HasValue)
        {
            filter = Builders<User>.Filter.And(
                filter,
                Builders<User>.Filter.Ne(u => u.Id, excludeUserId.Value.ToString())
            );
        }

        return !await _dbHelper.DocumentExistsAsync(users, filter);
    }

    public async Task<bool> ExistsAsync(string userId)
    {
        var users = _dbHelper.GetConfiguredCollection<User>();
        var filter = MongoDbHelper.GetByIdFilter<User>(userId, u => u.Id);
        return await _dbHelper.DocumentExistsAsync(users, filter);
    }

    public async Task<User> GetUserByIdAsync(string userId)
    {
        var users = _dbHelper.GetConfiguredCollection<User>();
        var filter = MongoDbHelper.GetByIdFilter<User>(userId, u => u.Id);
        var user = await users.Find(filter).FirstOrDefaultAsync();
        return user ?? throw new KeyNotFoundException($"User with ID {userId} not found");
    }
    
    // Pagination methods
    public async Task<(List<User> Users, int TotalCount)> GetUsersWithPaginationAsync(int page, int pageSize, string? sortBy = null, bool sortDescending = false)
    {
        var users = _dbHelper.GetConfiguredCollection<User>();
        var filter = Builders<User>.Filter.Empty;
        
        // Create sort definition
        var sort = sortBy?.ToLowerInvariant() switch
        {
            "username" => sortDescending ? Builders<User>.Sort.Descending(u => u.UserName) : Builders<User>.Sort.Ascending(u => u.UserName),
            "email" => sortDescending ? Builders<User>.Sort.Descending(u => u.Email) : Builders<User>.Sort.Ascending(u => u.Email),
            "createdat" => sortDescending ? Builders<User>.Sort.Descending(u => u.CreatedAt) : Builders<User>.Sort.Ascending(u => u.CreatedAt),
            "updatedat" => sortDescending ? Builders<User>.Sort.Descending(u => u.UpdatedAt) : Builders<User>.Sort.Ascending(u => u.UpdatedAt),
            _ => Builders<User>.Sort.Descending(u => u.CreatedAt)
        };
        
        var (items, totalCount) = await _dbHelper.ExecutePaginatedQueryAsync(
            users, filter, sort, page, pageSize);
        
        return (items, (int)totalCount);
    }
    


}