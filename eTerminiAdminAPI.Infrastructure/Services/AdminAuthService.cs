using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using eTerminiAdminAPI.Application.DTOs.Auth;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Authorization;
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

    public async Task<AdminLoginResultDto> LoginAsync(AdminLoginDto dto)
    {
        var users = await _uow.Users.FindAsync(u => u.Email == dto.Email.ToLower().Trim());
        var user  = users.FirstOrDefault();

        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            return Fail("Email ose fjalëkalimi është i gabuar.");

        var isSuperAdmin = user.Role == UserRole.SuperAdmin;

        // Lejohet vetëm SuperAdmin ose përdorues me një rol admin të caktuar.
        if (!isSuperAdmin && user.AdminRoleId == null)
            return Fail("Qasja e refuzuar. Nuk keni një rol administrativ.");

        if (!user.IsActive)
            return Fail("Llogaria është çaktivizuar.");

        // Llogarit lejet + emrin e rolit.
        List<string> permissions;
        string roleName;
        if (isSuperAdmin)
        {
            permissions = Permissions.All.ToList();
            roleName    = "SuperAdmin";
        }
        else
        {
            var adminRole = await _uow.AdminRoles.GetByIdAsync(user.AdminRoleId!.Value);
            if (adminRole == null)
                return Fail("Roli administrativ nuk u gjet.");

            permissions = adminRole.Permissions.ToList();
            roleName    = adminRole.Name;
        }

        var accessToken  = GenerateAccessToken(user, permissions);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);
        var expiry       = DateTime.UtcNow.AddMinutes(GetConfigInt("Jwt:AccessTokenExpiryMinutes", 60));

        return new AdminLoginResultDto
        {
            IsSuccess    = true,
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt    = expiry,
            Email        = user.Email,
            FullName     = $"{user.FirstName} {user.LastName}",
            Role         = user.Role.ToString(),
            RoleName     = roleName,
            Permissions  = permissions,
            AdminRoleId  = user.AdminRoleId
        };
    }

    public async Task<AdminLoginResultDto> RefreshTokenAsync(string refreshToken)
    {
        var tokens = await _uow.RefreshTokens.FindAsync(
            t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

        var token = tokens.FirstOrDefault();
        if (token == null)
            return Fail("Token i pavlefshëm ose i skaduar.");

        token.IsRevoked = true;
        _uow.RefreshTokens.Update(token);

        var users = await _uow.Users.FindAsync(u => u.Id == token.UserId);
        var user  = users.FirstOrDefault();
        if (user == null)
            return Fail("Përdoruesi nuk u gjet.");

        if (!user.IsActive)
            return Fail("Llogaria është çaktivizuar.");

        var isSuperAdmin = user.Role == UserRole.SuperAdmin;
        List<string> permissions;
        string roleName;

        if (isSuperAdmin)
        {
            permissions = Permissions.All.ToList();
            roleName    = "SuperAdmin";
        }
        else
        {
            if (user.AdminRoleId == null)
                return Fail("Qasja e refuzuar. Nuk keni një rol administrativ.");

            var adminRole = await _uow.AdminRoles.GetByIdAsync(user.AdminRoleId.Value);
            if (adminRole == null)
                return Fail("Roli administrativ nuk u gjet.");

            permissions = adminRole.Permissions.ToList();
            roleName    = adminRole.Name;
        }

        var newAccessToken  = GenerateAccessToken(user, permissions);
        var newRefreshToken = await CreateRefreshTokenAsync(user.Id);
        var expiry          = DateTime.UtcNow.AddMinutes(GetConfigInt("Jwt:AccessTokenExpiryMinutes", 60));

        await _uow.SaveChangesAsync();

        return new AdminLoginResultDto
        {
            IsSuccess    = true,
            AccessToken  = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt    = expiry,
            Email        = user.Email,
            FullName     = $"{user.FirstName} {user.LastName}",
            Role         = user.Role.ToString(),
            RoleName     = roleName,
            Permissions  = permissions,
            AdminRoleId  = user.AdminRoleId
        };
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null)
            return (false, "Përdoruesi nuk u gjet.");

        if (!VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            return (false, "Fjalëkalimi aktual është i gabuar.");

        user.PasswordHash = HashPassword(dto.NewPassword);
        user.UpdatedAt    = DateTime.UtcNow;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return (true, "Fjalëkalimi u ndryshua me sukses.");
    }

    private static AdminLoginResultDto Fail(string message) =>
        new() { IsSuccess = false, Message = message };

    private string GenerateAccessToken(User user, IEnumerable<string> permissions)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(GetConfigInt("Jwt:AccessTokenExpiryMinutes", 60));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role,               user.Role.ToString()),
            new("tenantId",                    user.TenantId.ToString()),
            new("fullName",                    $"{user.FirstName} {user.LastName}"),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        // Claim-e leje (lexohen nga PermissionAuthorizationHandler).
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

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
