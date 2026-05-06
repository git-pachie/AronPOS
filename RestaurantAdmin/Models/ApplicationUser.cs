using Microsoft.AspNetCore.Identity;

namespace RestaurantAdmin.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsSuspended { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SuspendedAt { get; set; }
    public string? SuspendReason { get; set; }

    // ── Profile fields ────────────────────────────────────────────────────────
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfileNotes { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
