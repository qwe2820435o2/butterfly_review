// Controllers/AuthController.cs

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using tennis_wave_api.Extensions;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models;
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
    private readonly ICaptchaService _captchaService;
    private readonly AdminSettings _adminSettings;

    public AuthController(IAuthService authService, ICaptchaService captchaService, IOptions<AdminSettings> adminSettings)
    {
        _authService = authService;
        _captchaService = captchaService;
        _adminSettings = adminSettings.Value;
    }

    /// <summary>
    /// Generates a math captcha used by the admin login form.
    /// </summary>
    [HttpGet("captcha")]
    public IActionResult GetCaptcha()
    {
        var (captchaId, question) = _captchaService.Generate();
        var dto = new CaptchaDto { CaptchaId = captchaId, Question = question };
        return Ok(ApiResponseHelper.Success(dto));
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
    /// Manually registers a user with an explicit role (admin tooling, called via Swagger/curl).
    /// When <c>AdminSettings:SetupKey</c> is configured, callers must send a matching
    /// <c>X-Setup-Key</c> header.
    /// </summary>
    [HttpPost("admin-register")]
    public async Task<IActionResult> AdminRegister([FromBody] AdminRegisterDto registerDto)
    {
        if (!string.IsNullOrEmpty(_adminSettings.SetupKey))
        {
            var providedKey = Request.Headers["X-Setup-Key"].ToString();
            if (!string.Equals(providedKey, _adminSettings.SetupKey, StringComparison.Ordinal))
            {
                return StatusCode(403, ApiResponseHelper.Fail<AuthResponseDto>("Invalid or missing setup key"));
            }
        }

        try
        {
            var response = await _authService.RegisterWithRoleAsync(registerDto);
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
        if (!_captchaService.Verify(loginDto.CaptchaId, loginDto.CaptchaCode))
        {
            return BadRequest(ApiResponseHelper.Fail<AuthResponseDto>("Invalid or expired captcha"));
        }

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