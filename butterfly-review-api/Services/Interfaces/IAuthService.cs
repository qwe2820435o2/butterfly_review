using tennis_wave_api.Models.DTOs;

namespace tennis_wave_api.Services.Interfaces;

/// <summary>
/// Defines the contract for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user and returns an authentication token.
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

    /// <summary>
    /// Manually registers a user with an explicit role (admin tooling).
    /// </summary>
    Task<AuthResponseDto> RegisterWithRoleAsync(AdminRegisterDto registerDto);

    /// <summary>
    /// Authenticates a user and returns a token upon successful login.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
}