namespace WebWorkNew.Models;

public class UserAvatar
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AvatarPath { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}

