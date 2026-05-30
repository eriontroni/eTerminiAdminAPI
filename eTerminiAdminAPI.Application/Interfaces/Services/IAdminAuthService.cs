using eTerminiAdminAPI.Application.DTOs.Auth;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminAuthService
{
    Task<AdminLoginResultDto>            LoginAsync(AdminLoginDto dto);
    Task<AdminLoginResultDto>            RefreshTokenAsync(string refreshToken);
    Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
}
