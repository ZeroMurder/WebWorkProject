namespace WebWorkNew.Models;

public class UserAuditLog
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty; // e.g. Register, AvatarUpload, ProfileUpdated, Snapshot10m
    public string Entity { get; set; } = string.Empty; // e.g. User, Profile, Avatar
    public int? EntityId { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

