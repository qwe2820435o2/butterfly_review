// Controllers/AuthController.cs

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tennis_wave_api.Extensions;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Controllers;

/// <summary>
/// Exposes endpoints for user registration and login.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        try
        {
            var response = await _authService.RegisterAsync(registerDto);
            return Ok(ApiResponseHelper.Success(response, "Registration successful"));
        }
        catch (BusinessException ex)
        {
            return BadRequest(ApiResponseHelper.Fail<AuthResponseDto>(ex.Message));
        }
    }

    /// <summary>
    /// Authenticates a user and provides a JWT.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        try
        {
            var response = await _authService.LoginAsync(loginDto);
            return Ok(ApiResponseHelper.Success(response, "Login successful"));
        }
        catch (BusinessException ex)
        {
            return BadRequest(ApiResponseHelper.Fail<AuthResponseDto>(ex.Message));
        }
    }
}