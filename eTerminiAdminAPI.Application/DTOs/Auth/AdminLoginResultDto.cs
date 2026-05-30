namespace eTerminiAdminAPI.Application.DTOs.Auth;

public class AdminLoginResultDto
{
    public bool      IsSuccess    { get; set; }
    public string    Message      { get; set; } = string.Empty;
    public string?   AccessToken  { get; set; }
    public string?   RefreshToken { get; set; }
    public DateTime? ExpiresAt    { get; set; }
    public string?   Email        { get; set; }
    public string?   FullName     { get; set; }
    public string?   Role         { get; set; }
}
