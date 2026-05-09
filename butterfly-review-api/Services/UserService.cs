using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Extensions;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Models.Entities;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }


    public async Task<UserDto> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            return _mapper.Map<UserDto>(user);
        }
        catch (KeyNotFoundException)
        {
            throw new BusinessException($"User {userId} is not exist", "USER_NOT_FOUND");
        }
    }

    public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        
        // Update properties
        if (!string.IsNullOrEmpty(updateUserDto.UserName))
            existingUser.UserName = updateUserDto.UserName;
        if (!string.IsNullOrEmpty(updateUserDto.Avatar))
            existingUser.Avatar = updateUserDto.Avatar;
        
        existingUser.UpdatedAt = DateTime.UtcNow;
        
        var updatedUser = await _userRepository.UpdateUserAsync(existingUser);
        return _mapper.Map<UserDto>(updatedUser);
    }

    public async Task<UserDto> DeleteUserAsync(string userId)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        
        // Update properties
        existingUser.IsActive = false;
        existingUser.UpdatedAt = DateTime.UtcNow;
        
        var updatedUser = await _userRepository.UpdateUserAsync(existingUser);
        return _mapper.Map<UserDto>(updatedUser);
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        
        // Hash the current password for comparison
        using var sha256 = SHA256.Create();
        var currentPasswordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(changePasswordDto.CurrentPassword)));
        
        if (user.PasswordHash != currentPasswordHash)
        {
            throw new BusinessException("Current password is incorrect", "INVALID_PASSWORD");
        }
        
        // Hash the new password
        var newPasswordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(changePasswordDto.NewPassword)));
        user.PasswordHash = newPasswordHash;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _userRepository.UpdateUserAsync(user);
        return true;
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null)
    {
        return await _userRepository.IsEmailUniqueAsync(email, excludeUserId);
    }

    // Pagination methods
    public async Task<PaginatedResultDto<UserDto>> GetUsersWithPaginationAsync(int page, int pageSize, string? sortBy = null, bool sortDescending = false)
    {
        var (users, totalCount) = await _userRepository.GetUsersWithPaginationAsync(page, pageSize, sortBy, sortDescending);
        var userDtos = _mapper.Map<List<UserDto>>(users);
        
        var result = new PaginatedResultDto<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
            HasPreviousPage = page > 1
        };
        

        return result;
    }

}