using System.Security.Cryptography;
using System.Text;
using eTerminiAdminAPI.Application.DTOs.Administrators;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Entities;
using eTerminiAPI.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminAdministratorService : IAdminAdministratorService
{
    private readonly IUnitOfWork    _uow;
    private readonly IConfiguration _config;

    public AdminAdministratorService(IUnitOfWork uow, IConfiguration config)
    {
        _uow    = uow;
        _config = config;
    }

    public async Task<IEnumerable<AdministratorDto>> GetAllAsync()
    {
        var users = (await _uow.Users.GetAllAsync())
            .Where(u => u.Role == UserRole.SuperAdmin || u.AdminRoleId != null)
            .ToList();

        var roles = (await _uow.AdminRoles.GetAllAsync()).ToDictionary(r => r.Id);

        return users.Select(u =>
        {
            var isSuper = u.Role == UserRole.SuperAdmin;
            string roleName = isSuper
                ? "SuperAdmin"
                : (u.AdminRoleId != null && roles.TryGetValue(u.AdminRoleId.Value, out var r) ? r.Name : "—");

            return new AdministratorDto
            {
                Id           = u.Id,
                FullName     = $"{u.FirstName} {u.LastName}",
                Email        = u.Email,
                PhoneNumber  = u.PhoneNumber,
                IsActive     = u.IsActive,
                IsSuperAdmin = isSuper,
                RoleId       = u.AdminRoleId,
                RoleName     = roleName,
                CreatedAt    = u.CreatedAt
            };
        }).OrderByDescending(a => a.IsSuperAdmin).ThenBy(a => a.FullName);
    }

    public async Task<(bool Success, string Message, AdministratorDto? Admin)> CreateAsync(CreateAdministratorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return (false, "Emri dhe mbiemri janë të detyrueshëm.", null);

        if (string.IsNullOrWhiteSpace(dto.Email))
            return (false, "Email-i është i detyrueshëm.", null);

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return (false, "Fjalëkalimi duhet të ketë të paktën 6 karaktere.", null);

        var role = await _uow.AdminRoles.GetByIdAsync(dto.RoleId);
        if (role == null)
            return (false, "Roli i zgjedhur nuk u gjet.", null);

        var email    = dto.Email.ToLower().Trim();
        var existing = await _uow.Users.FindAsync(u => u.Email == email);
        if (existing.Any())
            return (false, "Ky email është tashmë i regjistruar.", null);

        var user = new User
        {
            Id           = Guid.NewGuid(),
            TenantId     = role.TenantId,
            FirstName    = dto.FirstName.Trim(),
            LastName     = dto.LastName.Trim(),
            Email        = email,
            PasswordHash = HashPassword(dto.Password),
            PhoneNumber  = dto.PhoneNumber,
            Role         = UserRole.Staff,
            AdminRoleId  = role.Id,
            IsActive     = true,
            IsDeleted    = false,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        var admin = new AdministratorDto
        {
            Id           = user.Id,
            FullName     = $"{user.FirstName} {user.LastName}",
            Email        = user.Email,
            PhoneNumber  = user.PhoneNumber,
            IsActive     = user.IsActive,
            IsSuperAdmin = false,
            RoleId       = role.Id,
            RoleName     = role.Name,
            CreatedAt    = user.CreatedAt
        };

        return (true, "Administratori u krijua me sukses.", admin);
    }

    public async Task<(bool Success, string Message)> ToggleActiveAsync(Guid id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null)
            return (false, "Administratori nuk u gjet.");

        if (user.Role == UserRole.SuperAdmin)
            return (false, "SuperAdmin nuk mund të çaktivizohet.");

        user.IsActive  = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return (true, user.IsActive ? "Administratori u aktivizua." : "Administratori u çaktivizua.");
    }

    private string HashPassword(string password)
    {
        var saltBytes = Encoding.UTF8.GetBytes(_config["Auth:PasswordSalt"]!);
        using var hmac = new HMACSHA256(saltBytes);
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }
}
