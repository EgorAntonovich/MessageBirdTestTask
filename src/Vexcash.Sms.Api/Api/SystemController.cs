using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vexcash.Sms.Api.Infrastructure;

namespace Vexcash.Sms.Api.Api;

[ApiController]
[Route("api/system")]
[Produces("application/json")]
public sealed class SystemController(IOptions<MessageBirdOptions> options) : ControllerBase
{
    /// <summary>Returns the active SMS provider mode without exposing credentials.</summary>
    [HttpGet("status")]
    public IActionResult Status()
    {
        var settings = options.Value;
        return Ok(new
        {
            provider = "MessageBird",
            mode = settings.UseMock ? "Mock" : "Live",
            configured = settings.UseMock || !string.IsNullOrWhiteSpace(settings.AccessKey)
        });
    }
}
