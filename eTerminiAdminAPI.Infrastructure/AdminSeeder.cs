using System.Security.Cryptography;
using System.Text;
using eTerminiAPI.Domain.Entities;
using eTerminiAPI.Domain.Enums;
using eTerminiAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eTerminiAdminAPI.Infrastructure;

public static class AdminSeeder
{
    // Kredencialet default — ndrysho pas hyrjes së parë
    private const string DefaultEmail     = "admin@etermini.com";
    private const string DefaultPassword  = "Admin@2026";
    private const string DefaultFirstName = "Super";
    private const string DefaultLastName  = "Admin";

    public static async Task SeedSuperAdminAsync(
        AppDbContext      db,
        IConfiguration    config,
        ILogger           logger)
    {
        var exists = await db.Users
            .AnyAsync(u => u.Role == UserRole.SuperAdmin && !u.IsDeleted);

        if (exists)
        {
            logger.LogInformation("SuperAdmin ekziston tashmë — seed u anashkalua.");
            return;
        }

        // Merr TenantId-in e parë ekzistues (ose krijon një të ri)
        var tenant = await db.Tenants.FirstOrDefaultAsync();
        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id        = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name      = "eTermini System",
                Slug      = "system",
                IsActive  = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        var passwordHash = HashPassword(DefaultPassword, config["Auth:PasswordSalt"]!);

        var admin = new User
        {
            Id           = Guid.NewGuid(),
            TenantId     = tenant.Id,
            FirstName    = DefaultFirstName,
            LastName     = DefaultLastName,
            Email        = DefaultEmail,
            PasswordHash = passwordHash,
            Role         = UserRole.SuperAdmin,
            IsActive     = true,
            IsDeleted    = false,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow,
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "SuperAdmin u krijua me sukses. Email: {Email} | Fjalëkalimi: {Password}",
            DefaultEmail, DefaultPassword);
    }

    private static string HashPassword(string password, string salt)
    {
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        using var hmac = new HMACSHA256(saltBytes);
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }
}
