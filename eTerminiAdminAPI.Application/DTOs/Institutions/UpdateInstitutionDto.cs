namespace eTerminiAdminAPI.Application.DTOs.Institutions;

public class UpdateInstitutionDto
{
    public Guid?   CategoryId  { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string  City        { get; set; } = string.Empty;
    public string? Address     { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email       { get; set; }
}
