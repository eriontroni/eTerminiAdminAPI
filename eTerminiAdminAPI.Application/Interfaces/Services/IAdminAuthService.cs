using eTerminiAdminAPI.Application.DTOs.Auth;
using eTerminiAPI.Application.DTOs.Auth;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminAuthService
{
    Task<AuthResponseDto> LoginAsync(AdminLoginDto dto);
}
