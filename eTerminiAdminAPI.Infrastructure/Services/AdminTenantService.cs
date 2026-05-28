using eTerminiAdminAPI.Application.DTOs.Tenants;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminTenantService : IAdminTenantService
{
    private readonly IUnitOfWork _uow;

    public AdminTenantService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<TenantDto>> GetAllAsync()
    {
        var tenants = await _uow.Tenants.GetAllAsync();
        return tenants
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new TenantDto
            {
                Id       = t.Id,
                Name     = t.Name,
                Slug     = t.Slug,
                IsActive = t.IsActive
            });
    }
}
