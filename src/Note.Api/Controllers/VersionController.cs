using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Note.Api.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("version")]
[Produces("application/json")]
public sealed class VersionController : ControllerBase
{
    public sealed record VersionOutput(string Version);

    [HttpGet]
    [ProducesResponseType(typeof(VersionOutput), StatusCodes.Status200OK)]
    public ActionResult<VersionOutput> GetVersion()
    {
        var version = typeof(VersionController).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";

        return Ok(new VersionOutput(version));
    }
}
