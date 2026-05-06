using System.ComponentModel.DataAnnotations;

namespace RestaurantAdmin.ViewModels;

public class CreateUserViewModel
{
    [Required, MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Roles")]
    public List<string> SelectedRoles { get; set; } = new();

    public List<RoleCheckboxItem> AvailableRoles { get; set; } = new();
}

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Roles")]
    public List<string> SelectedRoles { get; set; } = new();

    public List<RoleCheckboxItem> AvailableRoles { get; set; } = new();
    public bool IsSuspended { get; set; }
}

public class UserListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsSuspended { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? ProfileImagePath { get; set; }
}

public class ResetPasswordViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    [Required, MinLength(6)]
    [Display(Name = "New Password")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm New Password")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class SuspendUserViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Reason for Suspension")]
    public string? Reason { get; set; }
}

public class RoleCheckboxItem
{
    public string RoleName { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class AssignRolesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<RoleCheckboxItem> Roles { get; set; } = new();
}

public class UserDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsSuspended { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public string? SuspendReason { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public List<string> Roles { get; set; } = new();

    // Profile fields
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
    public string? ProfileImagePath { get; set; }
}

public class EditProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Required, MaxLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [MaxLength(20)]
    [Display(Name = "Gender")]
    public string? Gender { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [MaxLength(200)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [MaxLength(100)]
    [Display(Name = "Country")]
    public string? Country { get; set; }

    [MaxLength(100)]
    [Display(Name = "Department")]
    public string? Department { get; set; }

    [MaxLength(100)]
    [Display(Name = "Position / Job Title")]
    public string? Position { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Notes")]
    public string? ProfileNotes { get; set; }

    // Current image path (for display)
    public string? ProfileImagePath { get; set; }

    // New image upload
    [Display(Name = "Profile Photo")]
    public IFormFile? ProfileImage { get; set; }
}
