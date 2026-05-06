using Microsoft.AspNetCore.Http;

namespace RestaurantAdmin.Helpers;

public static class ImageHelper
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    /// <summary>
    /// Validates, saves the uploaded file and returns the relative URL.
    /// Returns null if no file provided.
    /// Throws ArgumentException on validation failure.
    /// </summary>
    public static async Task<string?> SaveProductImageAsync(IFormFile? file, string? oldPath = null)
    {
        if (file == null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new ArgumentException("Only JPG, PNG, GIF or WEBP images are allowed.");

        if (file.Length > MaxBytes)
            throw new ArgumentException("Image must be smaller than 5 MB.");

        // Delete old image
        DeleteImage(oldPath);

        // Save new image
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"product_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/products/{fileName}";
    }

    public static void DeleteImage(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        var full = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
            relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(full)) File.Delete(full);
    }
}
