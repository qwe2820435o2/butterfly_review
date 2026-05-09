using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Services.Interfaces;
using AutoMapper;

namespace tennis_wave_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UserController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get users with pagination
    /// </summary>
    [HttpGet("paginated")]
    [AllowAnonymous] 
    public async Task<ActionResult<ApiResponse<PaginatedResultDto<UserDto>>>> GetUsersWithPagination(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var result = await _userService.GetUsersWithPaginationAsync(page, pageSize, sortBy, sortDescending);
            return Ok(ApiResponseHelper.Success(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseHelper.Fail<PaginatedResultDto<UserDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User profile information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(ApiResponseHelper.Success(user));
        }
        catch (KeyNotFoundException)
        {
            return Ok(ApiResponseHelper.Fail<UserDto>("User not found", 404));
        }
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, UpdateUserDto updateUserDto)
    {
        try
        {
            var updatedUser = await _userService.UpdateUserAsync(id, updateUserDto);
            return Ok(ApiResponseHelper.Success(updatedUser, "User updated successfully"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponseHelper.Fail<UserDto>("User not found"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseHelper.Fail<UserDto>(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return Ok(ApiResponseHelper.Success<object>(null, "User deleted successfully"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponseHelper.Fail<object>("User not found"));
        }
    }

    [HttpPost("{id}/change-password")]
    public async Task<ActionResult> ChangePassword(string id, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var success = await _userService.ChangePasswordAsync(id, changePasswordDto);
            if (success)
            {
                return Ok(ApiResponseHelper.Success<object>(null, "Password changed successfully"));
            }
            return BadRequest(ApiResponseHelper.Fail<object>("Failed to change password"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseHelper.Fail<object>(ex.Message));
        }
    }

    [HttpGet("check-email")]
    public async Task<ActionResult<bool>> CheckEmailUnique([FromQuery] string email, [FromQuery] int? excludeUserId = null)
    {
        var isUnique = await _userService.IsEmailUniqueAsync(email, excludeUserId);
        return Ok(ApiResponseHelper.Success(isUnique));
    }



}