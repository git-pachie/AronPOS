using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantAdmin.Data;
using RestaurantAdmin.Models;
using RestaurantAdmin.ViewModels;

namespace RestaurantAdmin.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Users
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.OrderBy(u => u.FullName).ToList();
        var viewModels = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            viewModels.Add(new UserListViewModel
            {
                Id               = user.Id,
                FullName         = user.FullName,
                Email            = user.Email ?? string.Empty,
                IsSuspended      = user.IsSuspended,
                CreatedAt        = user.CreatedAt,
                Roles            = roles.ToList(),
                ProfileImagePath = user.ProfileImagePath
            });
        }

        return View(viewModels);
    }

    // GET: Users/Details/id
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var vm = new UserDetailsViewModel
        {
            Id               = user.Id,
            FullName         = user.FullName,
            Email            = user.Email ?? string.Empty,
            PhoneNumber      = user.PhoneNumber,
            IsSuspended      = user.IsSuspended,
            CreatedAt        = user.CreatedAt,
            SuspendedAt      = user.SuspendedAt,
            SuspendReason    = user.SuspendReason,
            EmailConfirmed   = user.EmailConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            AccessFailedCount = user.AccessFailedCount,
            LockoutEnd       = user.LockoutEnd,
            Roles            = roles.ToList(),
            // Profile
            FirstName        = user.FirstName,
            LastName         = user.LastName,
            Address          = user.Address,
            City             = user.City,
            Country          = user.Country,
            ProfileNotes     = user.ProfileNotes,
            DateOfBirth      = user.DateOfBirth,
            Gender           = user.Gender,
            Department       = user.Department,
            Position         = user.Position,
            LastLoginAt      = user.LastLoginAt,
            ProfileImagePath = user.ProfileImagePath
        };

        return View(vm);
    }

    // GET: Users/EditProfile/id
    public async Task<IActionResult> EditProfile(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var vm = new EditProfileViewModel
        {
            Id               = user.Id,
            FullName         = user.FullName,
            FirstName        = user.FirstName,
            LastName         = user.LastName,
            PhoneNumber      = user.PhoneNumber,
            Gender           = user.Gender,
            DateOfBirth      = user.DateOfBirth,
            Address          = user.Address,
            City             = user.City,
            Country          = user.Country,
            Department       = user.Department,
            Position         = user.Position,
            ProfileNotes     = user.ProfileNotes,
            ProfileImagePath = user.ProfileImagePath
        };

        return View(vm);
    }

    // POST: Users/EditProfile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        // Remove IFormFile from validation — handled separately
        ModelState.Remove("ProfileImage");

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        user.FullName     = model.FullName;
        user.FirstName    = model.FirstName;
        user.LastName     = model.LastName;
        user.PhoneNumber  = model.PhoneNumber;
        user.Gender       = model.Gender;
        user.DateOfBirth  = model.DateOfBirth;
        user.Address      = model.Address;
        user.City         = model.City;
        user.Country      = model.Country;
        user.Department   = model.Department;
        user.Position     = model.Position;
        user.ProfileNotes = model.ProfileNotes;

        // Handle image upload
        if (model.ProfileImage != null && model.ProfileImage.Length > 0)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError("ProfileImage", "Only JPG, PNG, GIF or WEBP images are allowed.");
                return View(model);
            }
            // No file size limit enforced — up to 1 GB allowed by server config

            // Delete old image
            if (!string.IsNullOrEmpty(user.ProfileImagePath))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfileImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // Save new image
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ProfileImage.CopyToAsync(stream);

            user.ProfileImagePath = $"/uploads/profiles/{fileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        TempData["Success"] = $"Profile for '{user.FullName}' updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    // POST: Users/RemovePhoto/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePhoto(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (!string.IsNullOrEmpty(user.ProfileImagePath))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                user.ProfileImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            user.ProfileImagePath = null;
            await _userManager.UpdateAsync(user);
        }

        TempData["Success"] = "Profile photo removed.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Users/Create
    public async Task<IActionResult> Create()
    {
        var vm = new CreateUserViewModel
        {
            AvailableRoles = _roleManager.Roles
                .Select(r => new RoleCheckboxItem { RoleName = r.Name! })
                .ToList()
        };
        return View(vm);
    }

    // POST: Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = _roleManager.Roles
                .Select(r => new RoleCheckboxItem { RoleName = r.Name! })
                .ToList();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            model.AvailableRoles = _roleManager.Roles
                .Select(r => new RoleCheckboxItem { RoleName = r.Name! })
                .ToList();
            return View(model);
        }

        if (model.SelectedRoles.Any())
            await _userManager.AddToRolesAsync(user, model.SelectedRoles);

        TempData["Success"] = $"User '{user.FullName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Users/Edit/id
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.ToList();

        var vm = new EditUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            IsSuspended = user.IsSuspended,
            SelectedRoles = userRoles.ToList(),
            AvailableRoles = allRoles.Select(r => new RoleCheckboxItem
            {
                RoleName = r.Name!,
                IsSelected = userRoles.Contains(r.Name!)
            }).ToList()
        };

        return View(vm);
    }

    // POST: Users/Edit/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = _roleManager.Roles
                .Select(r => new RoleCheckboxItem
                {
                    RoleName = r.Name!,
                    IsSelected = model.SelectedRoles.Contains(r.Name!)
                }).ToList();
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        TempData["Success"] = $"User '{user.FullName}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Users/AssignRoles/id
    public async Task<IActionResult> AssignRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.ToList();

        var vm = new AssignRolesViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty,
            FullName = user.FullName,
            Roles = allRoles.Select(r => new RoleCheckboxItem
            {
                RoleName = r.Name!,
                IsSelected = userRoles.Contains(r.Name!)
            }).ToList()
        };

        return View(vm);
    }

    // POST: Users/AssignRoles
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRoles(AssignRolesViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = model.Roles
            .Where(r => r.IsSelected)
            .Select(r => r.RoleName)
            .ToList();

        var toRemove = currentRoles.Except(selectedRoles).ToList();
        var toAdd = selectedRoles.Except(currentRoles).ToList();

        if (toRemove.Any())
            await _userManager.RemoveFromRolesAsync(user, toRemove);

        if (toAdd.Any())
            await _userManager.AddToRolesAsync(user, toAdd);

        TempData["Success"] = $"Roles updated for '{user.FullName}'.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Users/Suspend/id
    public async Task<IActionResult> Suspend(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (user.IsSuspended)
        {
            TempData["Info"] = "User is already suspended.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new SuspendUserViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty,
            FullName = user.FullName
        };

        return View(vm);
    }

    // POST: Users/Suspend
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(SuspendUserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        // Prevent suspending yourself
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Id == user.Id)
        {
            TempData["Error"] = "You cannot suspend your own account.";
            return RedirectToAction(nameof(Index));
        }

        user.IsSuspended = true;
        user.SuspendedAt = DateTime.UtcNow;
        user.SuspendReason = model.Reason;

        await _userManager.UpdateAsync(user);

        // Force sign out the suspended user by updating security stamp
        await _userManager.UpdateSecurityStampAsync(user);

        TempData["Success"] = $"User '{user.FullName}' has been suspended.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Users/Unsuspend/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsuspend(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.IsSuspended = false;
        user.SuspendedAt = null;
        user.SuspendReason = null;

        await _userManager.UpdateAsync(user);

        TempData["Success"] = $"User '{user.FullName}' has been unsuspended.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Users/ResetPassword/id
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var vm = new ResetPasswordViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty
        };

        return View(vm);
    }

    // POST: Users/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        TempData["Success"] = $"Password for '{user.Email}' has been reset successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Users/Delete/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Id == user.Id)
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.DeleteAsync(user);
        TempData["Success"] = $"User '{user.FullName}' has been deleted.";
        return RedirectToAction(nameof(Index));
    }
}
