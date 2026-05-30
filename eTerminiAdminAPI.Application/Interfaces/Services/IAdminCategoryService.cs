using eTerminiAdminAPI.Application.DTOs.Categories;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminCategoryService
{
    Task<IEnumerable<CategoryDto>>                            GetAllAsync();
    Task<CategoryDto?>                                        GetByIdAsync(Guid id);
    Task<(bool Success, string Message, CategoryDto? Cat)>    CreateAsync(CreateCategoryDto dto);
    Task<(bool Success, string Message, CategoryDto? Cat)>    UpdateAsync(Guid id, UpdateCategoryDto dto);
    Task<(bool Success, string Message)>                      DeleteAsync(Guid id);
}
