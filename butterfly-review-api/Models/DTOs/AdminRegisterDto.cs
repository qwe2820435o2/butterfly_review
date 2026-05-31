using System.ComponentModel.DataAnnotations;

namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Payload for the manual (admin) user-registration endpoint. Unlike the public
/// <see cref="RegisterDto"/>, this allows assigning the new user's role.
/// </summary>
public class AdminRegisterDto
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

    /// <summary>"Admin" or "User" (defaults to "User").</summary>
    public string? Role { get; set; }
}
