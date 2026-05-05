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
