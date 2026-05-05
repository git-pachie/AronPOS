using Microsoft.AspNetCore.Identity;

namespace RestaurantAdmin.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsSuspended { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SuspendedAt { get; set; }
    public string? SuspendReason { get; set; }
}
