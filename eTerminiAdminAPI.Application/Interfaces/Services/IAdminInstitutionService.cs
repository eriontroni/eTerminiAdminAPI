using eTerminiAdminAPI.Application.DTOs.Institutions;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminInstitutionService
{
    Task<IEnumerable<InstitutionAdminDto>> GetAllAsync();
    Task<InstitutionAdminDto>              GetByIdAsync(Guid id);
    Task<InstitutionAdminDto>              CreateAsync(CreateInstitutionDto dto);
    Task<InstitutionAdminDto>              UpdateAsync(Guid id, UpdateInstitutionDto dto);
    Task<InstitutionAdminDto>              ToggleActiveAsync(Guid id);
    Task                                   DeleteAsync(Guid id);
}
