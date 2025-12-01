using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

[ApiController]
[Route("")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    [HttpHead("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }

    [HttpGet("health/ready")]
    [HttpHead("health/ready")]
    public IActionResult Ready()
    {
        return Ok(new { status = "Ready", timestamp = DateTime.UtcNow });
    }

    [HttpGet("health/live")]
    [HttpHead("health/live")]
    public IActionResult Live()
    {
        return Ok(new { status = "Live", timestamp = DateTime.UtcNow });
    }
}
