using System.ComponentModel.DataAnnotations;

namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Data Transfer Object for user login requests.
/// </summary>
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}