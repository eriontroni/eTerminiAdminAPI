using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using eTerminiAdminAPI.Application.DTOs.Auth;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.DTOs.Auth;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Entities;
using eTerminiAPI.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly IUnitOfWork    _uow;
    private readonly IConfiguration _config;

    public AdminAuthService(IUnitOfWork uow, IConfiguration config)
    {
        _uow    = uow;
        _config = config;
    }

    public async Task<AuthResponseDto> LoginAsync(AdminLoginDto dto)
    {
        var users = await _uow.Users.FindAsync(u => u.Email == dto.Email.ToLower().Trim());
        var user  = users.FirstOrDefault()
            ?? throw new UnauthorizedAccessException("Email ose fjalëkalimi është i gabuar.");

        if (user.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("Qasja e refuzuar. Vetëm SuperAdmin mund të hyjë.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Llogaria është çaktivizuar.");

        if (!VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email ose fjalëkalimi është i gabuar.");

        var accessToken  = GenerateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);
        var expiry       = DateTime.UtcNow.AddMinutes(GetConfigInt("Jwt:AccessTokenExpiryMinutes", 60));

        return new AuthResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt    = expiry,
            Email        = user.Email,
            FullName     = $"{user.FirstName} {user.LastName}",
            Role         = user.Role.ToString()
        };
    }

    private string GenerateAccessToken(User user)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(GetConfigInt("Jwt:AccessTokenExpiryMinutes", 60));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim("tenantId",                    user.TenantId.ToString()),
            new Claim("fullName",                    $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> CreateRefreshTokenAsync(Guid userId)
    {
        var tokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await _uow.RefreshTokens.AddAsync(new RefreshToken
        {
            Id        = Guid.NewGuid(),
            UserId    = userId,
            Token     = tokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(GetConfigInt("Jwt:RefreshTokenExpiryDays", 7)),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _uow.SaveChangesAsync();
        return tokenString;
    }

    private string HashPassword(string password)
    {
        var saltBytes = Encoding.UTF8.GetBytes(_config["Auth:PasswordSalt"]!);
        using var hmac = new HMACSHA256(saltBytes);
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    private bool VerifyPassword(string password, string stored) => HashPassword(password) == stored;

    private int GetConfigInt(string key, int fallback)
        => int.TryParse(_config[key], out var v) ? v : fallback;
}
