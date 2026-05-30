using eTerminiAdminAPI.Application.DTOs.Auth;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminAuthService
{
    Task<AdminLoginResultDto> LoginAsync(AdminLoginDto dto);
}
