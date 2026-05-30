using eTerminiAdminAPI.Application.DTOs.Roles;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Authorization;
using eTerminiAPI.Domain.Entities;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminRoleService : IAdminRoleService
{
    private readonly IUnitOfWork _uow;

    public AdminRoleService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        var roles = (await _uow.AdminRoles.GetAllAsync()).ToList();
        var users = (await _uow.Users.GetAllAsync()).ToList();

        return roles
            .OrderByDescending(r => r.IsSystem)
            .ThenBy(r => r.Name)
            .Select(r => MapToDto(r, users.Count(u => u.AdminRoleId == r.Id)));
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id)
    {
        var role = await _uow.AdminRoles.GetByIdAsync(id);
        if (role == null) return null;

        var users = await _uow.Users.FindAsync(u => u.AdminRoleId == id);
        return MapToDto(role, users.Count());
    }

    public async Task<(bool Success, string Message, RoleDto? Role)> CreateAsync(CreateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Emri i rolit është i detyrueshëm.", null);

        var existing = await _uow.AdminRoles.FindAsync(r => r.Name == dto.Name.Trim());
        if (existing.Any())
            return (false, $"Roli '{dto.Name}' ekziston tashmë.", null);

        var permissions = SanitizePermissions(dto.Permissions);

        var role = new AdminRole
        {
            Id          = Guid.NewGuid(),
            TenantId    = await GetSystemTenantIdAsync(),
            Name        = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsSystem    = false,
            Permissions = permissions,
            CreatedAt   = DateTime.UtcNow
        };

        await _uow.AdminRoles.AddAsync(role);
        await _uow.SaveChangesAsync();

        return (true, "Roli u krijua me sukses.", MapToDto(role, 0));
    }

    public async Task<(bool Success, string Message, RoleDto? Role)> UpdateAsync(Guid id, UpdateRoleDto dto)
    {
        var role = await _uow.AdminRoles.GetByIdAsync(id);
        if (role == null)
            return (false, "Roli nuk u gjet.", null);

        if (role.IsSystem)
            return (false, "Rolet e sistemit nuk mund të editohen.", null);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Emri i rolit është i detyrueshëm.", null);

        var dup = await _uow.AdminRoles.FindAsync(r => r.Name == dto.Name.Trim() && r.Id != id);
        if (dup.Any())
            return (false, $"Roli '{dto.Name}' ekziston tashmë.", null);

        role.Name        = dto.Name.Trim();
        role.Description = dto.Description?.Trim();
        role.Permissions = SanitizePermissions(dto.Permissions);
        role.UpdatedAt   = DateTime.UtcNow;

        _uow.AdminRoles.Update(role);
        await _uow.SaveChangesAsync();

        var count = (await _uow.Users.FindAsync(u => u.AdminRoleId == id)).Count();
        return (true, "Roli u përditësua me sukses.", MapToDto(role, count));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var role = await _uow.AdminRoles.GetByIdAsync(id);
        if (role == null)
            return (false, "Roli nuk u gjet.");

        if (role.IsSystem)
            return (false, "Rolet e sistemit nuk mund të fshihen.");

        var assigned = await _uow.Users.FindAsync(u => u.AdminRoleId == id);
        if (assigned.Any())
            return (false, "Ky rol nuk mund të fshihet sepse ka administratorë të caktuar.");

        _uow.AdminRoles.Delete(role);
        await _uow.SaveChangesAsync();

        return (true, "Roli u fshi me sukses.");
    }

    public IEnumerable<PermissionCatalogModuleDto> GetPermissionCatalog() =>
        Permissions.Catalog.Select(m => new PermissionCatalogModuleDto
        {
            Key     = m.Key,
            Label   = m.Label,
            Actions = m.Actions.Select(a => new PermissionCatalogActionDto
            {
                Code  = a.Code,
                Label = a.Label
            }).ToList()
        });

    private static List<string> SanitizePermissions(IEnumerable<string> codes) =>
        codes.Where(Permissions.IsValid).Distinct().ToList();

    private async Task<Guid> GetSystemTenantIdAsync()
    {
        var tenants = await _uow.Tenants.GetAllAsync();
        return tenants.FirstOrDefault()?.Id ?? Guid.Empty;
    }

    private static RoleDto MapToDto(AdminRole r, int adminCount) => new()
    {
        Id          = r.Id,
        Name        = r.Name,
        Description = r.Description,
        IsSystem    = r.IsSystem,
        Permissions = r.Permissions.ToList(),
        AdminCount  = adminCount,
        CreatedAt   = r.CreatedAt
    };
}
