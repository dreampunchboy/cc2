using Microsoft.AspNetCore.Mvc;

namespace BG.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShutdownController : ControllerBase
{
    private readonly IHostApplicationLifetime _lifetime;

    public ShutdownController(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    [HttpPost]
    public IActionResult Post()
    {
        AppShutdown.OnShutdown?.Invoke();
        return Ok();
    }
}

/// <summary>Callback registered by Program.Main to hide tray and exit the WinForms app.</summary>
public static class AppShutdown
{
    public static Action? OnShutdown { get; set; }
}
