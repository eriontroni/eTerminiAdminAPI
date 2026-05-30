using System.Text.RegularExpressions;
using eTerminiAdminAPI.Application.DTOs.Tenants;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Entities;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminTenantService : IAdminTenantService
{
    private readonly IUnitOfWork _uow;

    public AdminTenantService(IUnitOfWork uow) => _uow = uow;   

    public async Task<IEnumerable<TenantDto>> GetAllAsync()
    {
        var tenants = await _uow.Tenants.GetAllAsync();
        return tenants
            .OrderBy(t => t.Name)
            .Select(t => new TenantDto
            {
                Id       = t.Id,
                Name     = t.Name,
                Slug     = t.Slug,
                IsActive = t.IsActive
            });
    }

    public async Task<TenantDto> CreateAsync(CreateTenantDto dto)
    {
        var slug = GenerateSlug(dto.Name);

        var existing = await _uow.Tenants.FindAsync(t => t.Slug == slug);
        if (existing.Any())
            throw new InvalidOperationException($"Qyteti '{dto.Name}' ekziston tashmë.");

        var tenant = new Tenant
        {
            Id        = Guid.NewGuid(),
            Name      = dto.Name.Trim(),
            Slug      = slug,
            IsActive  = true,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Tenants.AddAsync(tenant);
        await _uow.SaveChangesAsync();

        return new TenantDto { Id = tenant.Id, Name = tenant.Name, Slug = tenant.Slug, IsActive = tenant.IsActive };
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await _uow.Tenants.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Qyteti me ID {id} nuk u gjet.");

        var linked = await _uow.Institutions.FindAsync(i => i.TenantId == id);
        if (linked.Any())
            throw new InvalidOperationException("Ky qytet nuk mund të fshihet sepse ka institucione të lidhura me të.");

        // Soft delete: shëno si i fshirë (global query filter e përjashton nga listat).
        tenant.IsDeleted = true;
        tenant.IsActive  = false;
        tenant.UpdatedAt = DateTime.UtcNow;

        _uow.Tenants.Update(tenant);
        await _uow.SaveChangesAsync();
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.Trim().ToLower()
            .Replace('ë', 'e').Replace('ç', 'c')
            .Replace(' ', '-').Replace('_', '-');
        return Regex.Replace(slug, @"[^a-z0-9-]", "");
    }
}
