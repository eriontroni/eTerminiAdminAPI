using eTerminiAdminAPI.Application.DTOs.Services;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminServiceService
{
    Task<IEnumerable<ServiceAdminDto>> GetByInstitutionAsync(Guid institutionId, bool includeInactive = false);
    Task<ServiceAdminDto> GetByIdAsync(Guid id);
    Task<(bool Success, string Message, ServiceAdminDto? Service)> CreateAsync(CreateServiceDto dto);
    Task<(bool Success, string Message, ServiceAdminDto? Service)> UpdateAsync(Guid id, UpdateServiceDto dto);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
    Task<(bool Success, string Message, ServiceAdminDto? Service)> ToggleActiveAsync(Guid id);
}
