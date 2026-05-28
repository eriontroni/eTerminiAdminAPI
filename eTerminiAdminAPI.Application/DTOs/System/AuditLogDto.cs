namespace eTerminiAdminAPI.Application.DTOs.System;

public class AuditLogDto
{
    public Guid     Id         { get; set; }
    public Guid?    UserId     { get; set; }
    public string   Action     { get; set; } = string.Empty;
    public string   EntityName { get; set; } = string.Empty;
    public string?  EntityId   { get; set; }
    public string?  OldValues  { get; set; }
    public string?  NewValues  { get; set; }
    public string?  IpAddress  { get; set; }
    public DateTime CreatedAt  { get; set; }
}
