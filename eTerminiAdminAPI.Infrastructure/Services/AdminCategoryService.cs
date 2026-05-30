using eTerminiAdminAPI.Application.DTOs.Categories;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Entities;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminCategoryService : IAdminCategoryService
{
    private readonly IUnitOfWork _uow;

    public AdminCategoryService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        // Katalog i përbashkët — deduplikohet sipas emrit (kategoritë mund të ekzistojnë për tenant të ndryshëm).
        var categories = await _uow.ServiceCategories.GetAllAsync();
        return categories
            .GroupBy(c => c.Name)
            .Select(g => g.First())
            .OrderBy(c => c.Name)
            .Select(MapToDto);
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var cat = await _uow.ServiceCategories.GetByIdAsync(id);
        return cat == null ? null : MapToDto(cat);
    }

    public async Task<(bool Success, string Message, CategoryDto? Cat)> CreateAsync(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Emri i kategorisë është i detyrueshëm.", null);

        var existing = await _uow.ServiceCategories.FindAsync(c => c.Name == dto.Name.Trim());
        if (existing.Any())
            return (false, $"Kategoria '{dto.Name}' ekziston tashmë.", null);

        var category = new ServiceCategory
        {
            Id          = Guid.NewGuid(),
            TenantId    = await GetSystemTenantIdAsync(),
            Name        = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt   = DateTime.UtcNow
        };

        await _uow.ServiceCategories.AddAsync(category);
        await _uow.SaveChangesAsync();

        return (true, "Kategoria u krijua me sukses.", MapToDto(category));
    }

    public async Task<(bool Success, string Message, CategoryDto? Cat)> UpdateAsync(Guid id, UpdateCategoryDto dto)
    {
        var category = await _uow.ServiceCategories.GetByIdAsync(id);
        if (category == null)
            return (false, "Kategoria nuk u gjet.", null);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Emri i kategorisë është i detyrueshëm.", null);

        var dup = await _uow.ServiceCategories.FindAsync(c => c.Name == dto.Name.Trim() && c.Id != id);
        if (dup.Any())
            return (false, $"Kategoria '{dto.Name}' ekziston tashmë.", null);

        category.Name        = dto.Name.Trim();
        category.Description = dto.Description?.Trim();
        category.UpdatedAt   = DateTime.UtcNow;

        _uow.ServiceCategories.Update(category);
        await _uow.SaveChangesAsync();

        return (true, "Kategoria u përditësua me sukses.", MapToDto(category));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var category = await _uow.ServiceCategories.GetByIdAsync(id);
        if (category == null)
            return (false, "Kategoria nuk u gjet.");

        var usedByServices = await _uow.PublicServices.FindAsync(s => s.CategoryId == id);
        if (usedByServices.Any())
            return (false, "Kjo kategori nuk mund të fshihet sepse ka shërbime të lidhura me të.");

        var usedByInstitutions = await _uow.Institutions.FindAsync(i => i.CategoryId == id);
        if (usedByInstitutions.Any())
            return (false, "Kjo kategori nuk mund të fshihet sepse ka institucione të lidhura me të.");

        _uow.ServiceCategories.Delete(category);
        await _uow.SaveChangesAsync();

        return (true, "Kategoria u fshi me sukses.");
    }

    private async Task<Guid> GetSystemTenantIdAsync()
    {
        var tenants = await _uow.Tenants.GetAllAsync();
        return tenants.FirstOrDefault()?.Id ?? Guid.Empty;
    }

    private static CategoryDto MapToDto(ServiceCategory c) => new()
    {
        Id          = c.Id,
        Name        = c.Name,
        Description = c.Description
    };
}
