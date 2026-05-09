using System.ComponentModel.DataAnnotations;

namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Defines the required data for a new user registration.
/// </summary>
public class RegisterDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public string? Avatar { get; set; }
}