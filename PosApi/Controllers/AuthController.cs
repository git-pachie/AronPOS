using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PosApi.Data;

namespace PosApi.Controllers;

/// <summary>
/// Authentication endpoints for the Android POS app.
/// Validates credentials against the shared AIPOS database (AspNetUsers table).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Login with username/email and password.
    /// Returns user info and roles on success.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username and password are required." });

        var conn = _context.Database.GetDbConnection();
        await conn.OpenAsync();

        try
        {
            // Find user by username or email
            string? userId = null, userName = null, email = null,
                    passwordHash = null, fullName = null;
            bool isSuspended = false;

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT Id, UserName, Email, PasswordHash, FullName, IsSuspended
                    FROM AspNetUsers
                    WHERE UserName = @u OR Email = @u";
                var p = cmd.CreateParameter();
                p.ParameterName = "@u";
                p.Value = request.Username;
                cmd.Parameters.Add(p);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userId       = reader.GetString(0);
                    userName     = reader.GetString(1);
                    email        = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    passwordHash = reader.IsDBNull(3) ? null : reader.GetString(3);
                    fullName     = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    isSuspended  = reader.GetBoolean(5);
                }
            }

            if (userId == null || passwordHash == null)
                return Unauthorized(new { message = "Invalid username or password." });

            if (isSuspended)
                return Unauthorized(new { message = "Your account has been suspended. Contact the administrator." });

            // Verify password using ASP.NET Identity's PasswordHasher
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<object>();
            var verifyResult = hasher.VerifyHashedPassword(new object(), passwordHash, request.Password);

            if (verifyResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid username or password." });

            // Get roles
            var roles = new List<string>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT r.Name
                    FROM AspNetRoles r
                    INNER JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
                    WHERE ur.UserId = @uid";
                var p = cmd.CreateParameter();
                p.ParameterName = "@uid";
                p.Value = userId;
                cmd.Parameters.Add(p);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    roles.Add(reader.GetString(0));
            }

            return Ok(new LoginResponse
            {
                UserId   = userId,
                Username = userName ?? string.Empty,
                Email    = email    ?? string.Empty,
                FullName = fullName ?? string.Empty,
                Roles    = roles,
                Message  = "Login successful"
            });
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string UserId   { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string Message  { get; set; } = string.Empty;
}
