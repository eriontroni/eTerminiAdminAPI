using eTerminiAdminAPI.Application.DTOs.Tenants;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminTenantService
{
    Task<IEnumerable<TenantDto>> GetAllAsync();
    Task<TenantDto> CreateAsync(CreateTenantDto dto);
    Task DeleteAsync(Guid id);
}
