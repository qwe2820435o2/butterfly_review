using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Extensions;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Models.Entities;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Services;

/// <summary>
/// Handles core authentication logic like user registration, login, and token generation.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly AdminSettings _adminSettings;
    private readonly IMapper _mapper;

    public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtSettings, IOptions<AdminSettings> adminSettings, IMapper mapper)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
        _adminSettings = adminSettings.Value;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // First, ensure the email isn't already in use.
        if (await _userRepository.GetByEmailAsync(registerDto.Email) != null)
        {
            throw new BusinessException("Email is already taken.", "EMAIL_EXISTS");
        }

        var user = new User
        {
            UserName = registerDto.UserName,
            // Store email normalized (lowercase) to stay consistent with GetByEmailAsync lookups.
            Email = registerDto.Email.Trim().ToLowerInvariant(),
            // Hash the password securely using BCrypt before storing it.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Avatar = registerDto.Avatar ?? "avatar1.png" // Default avatar if not provided
        };
        
        var createdUser = await _userRepository.CreateUserAsync(user);

        // Use AutoMapper to map the user entity to the response DTO.
        var response = _mapper.Map<AuthResponseDto>(createdUser);

        // Generate the token and assign it to the response object.
        response.Token = GenerateJwtToken(createdUser);

        return response;
    }

    public async Task<AuthResponseDto> RegisterWithRoleAsync(AdminRegisterDto registerDto)
    {
        // Ensure the email isn't already in use.
        if (await _userRepository.GetByEmailAsync(registerDto.Email) != null)
        {
            throw new BusinessException("Email is already taken.", "EMAIL_EXISTS");
        }

        // Normalize the requested role; anything other than "Admin" falls back to "User".
        var role = string.Equals(registerDto.Role?.Trim(), "Admin", StringComparison.OrdinalIgnoreCase)
            ? "Admin"
            : "User";

        var user = new User
        {
            UserName = registerDto.UserName,
            // Store email normalized (lowercase) to stay consistent with GetByEmailAsync lookups.
            Email = registerDto.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Role = role,
            Avatar = registerDto.Avatar ?? "avatar1.png"
        };

        var createdUser = await _userRepository.CreateUserAsync(user);

        var response = _mapper.Map<AuthResponseDto>(createdUser);
        response.Token = GenerateJwtToken(createdUser);

        return response;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);

        // Verify the user exists and the provided password matches the stored hash.
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            // Use a generic error message to prevent account enumeration attacks.
            throw new BusinessException("Invalid credentials.", "INVALID_CREDENTIALS");
        }

        // Bootstrap admins: promote (and persist) any user whose email is in the configured allowlist.
        if (_adminSettings.AdminEmails.Any(e => string.Equals(e, user.Email, StringComparison.OrdinalIgnoreCase))
            && !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            user.Role = "Admin";
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);
        }

        var response = _mapper.Map<AuthResponseDto>(user);
        
        // Generate the token and assign it to the response object.
        response.Token = GenerateJwtToken(user);

        return response;
    }

    /// <summary>
    /// Generates a JWT for a given user.
    /// </summary>
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        // Define the claims that will be encoded in the token payload.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName),
            new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}