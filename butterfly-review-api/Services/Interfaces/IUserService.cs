using System.Collections.Generic;
using System.Threading.Tasks;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Services.Interfaces;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(string userId);
    Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);
    Task<UserDto> DeleteUserAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);
    
    // Pagination methods
    Task<PaginatedResultDto<UserDto>> GetUsersWithPaginationAsync(int page, int pageSize, string? sortBy = null,
        bool sortDescending = false);
}