using Microsoft.AspNetCore.Mvc;

namespace PosApi.Controllers;

/// <summary>
/// Store information and health check endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StoreController : ControllerBase
{
    private readonly IConfiguration _config;

    public StoreController(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Get store name and API version.
    /// </summary>
    /// <returns>Store name and version string.</returns>
    [HttpGet("info")]
    [ProducesResponseType(200)]
    public IActionResult GetStoreInfo()
    {
        return Ok(new
        {
            name    = _config["StoreName"] ?? "ARON MINI MART",
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Health check — confirms the API is running.
    /// </summary>
    /// <returns>Status ok with current UTC timestamp.</returns>
    [HttpGet("health")]
    [ProducesResponseType(200)]
    public IActionResult Health() =>
        Ok(new { status = "ok", timestamp = DateTime.UtcNow });
}
