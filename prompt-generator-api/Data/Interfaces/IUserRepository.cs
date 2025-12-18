using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Data.Interfaces;

/// <summary>
/// Defines the contract for user data access operations.
/// </summary>
public interface IUserRepository
{
    Task<bool> ExistsAsync(string userId);
    Task<User> GetUserByIdAsync(string userId);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);
    
    // Pagination methods
    Task<(List<User> Users, int TotalCount)> GetUsersWithPaginationAsync(int page, int pageSize, string? sortBy = null, bool sortDescending = false);
}