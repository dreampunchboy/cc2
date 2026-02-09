using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace CC2.Launcher;

/// <summary>
/// Configures and runs a Kestrel HTTP server that:
///   1. Serves the browser game as static files from the Game/ folder
///   2. Exposes REST API endpoints for the Steam bridge
/// </summary>
public sealed class GameServer
{
    private readonly SteamBridge _steam;
    private readonly string _gameFolder;
    private WebApplication? _app;

    /// <summary>The port the server is listening on.</summary>
    public int Port { get; private set; }

    /// <summary>The full base URL (e.g. http://localhost:12345).</summary>
    public string BaseUrl => $"http://localhost:{Port}";

    /// <summary>Tracks the last time the browser called any API endpoint (for heartbeat detection).</summary>
    public DateTime LastHeartbeat { get; private set; } = DateTime.UtcNow;

    public GameServer(SteamBridge steam, string gameFolder)
    {
        _steam = steam;
        _gameFolder = Path.GetFullPath(gameFolder);
    }

    /// <summary>
    /// Builds and starts the Kestrel server on a free localhost port.
    /// </summary>
    public async Task StartAsync()
    {
        Port = FindFreePort();

        var builder = WebApplication.CreateBuilder();

        // Register CORS services so the middleware can resolve ICorsService
        builder.Services.AddCors();

        // Bind to localhost only — no firewall prompts
        builder.WebHost.UseUrls($"http://localhost:{Port}");

        // Suppress default ASP.NET Core startup logging noise
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);

        _app = builder.Build();

        // --- CORS (allow the browser page on localhost to call the API) ---
        _app.UseCors(policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        // --- Static files: serve the Game/ folder at root ---
        if (Directory.Exists(_gameFolder))
        {
            var fileProvider = new PhysicalFileProvider(_gameFolder);
            _app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
            _app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
        }
        else
        {
            Console.WriteLine($"[Server] WARNING: Game folder not found at {_gameFolder}");
        }

        // --- API Routes ---
        MapApiRoutes(_app);

        Console.WriteLine($"[Server] Starting on {BaseUrl}  (serving from {_gameFolder})");
        await _app.StartAsync();
    }

    /// <summary>Gracefully stops the server.</summary>
    public async Task StopAsync()
    {
        if (_app is not null)
        {
            Console.WriteLine("[Server] Shutting down...");
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------ //
    //  API Endpoints
    // ------------------------------------------------------------------ //

    private void MapApiRoutes(WebApplication app)
    {
        // GET /api/steam/status — health check + Steam connection status
        app.MapGet("/api/steam/status", () =>
        {
            LastHeartbeat = DateTime.UtcNow;
            return Results.Json(new
            {
                ok = true,
                steamConnected = _steam.IsInitialized,
                timestamp = DateTime.UtcNow
            });
        });

        // GET /api/steam/user — current player info
        app.MapGet("/api/steam/user", () =>
        {
            LastHeartbeat = DateTime.UtcNow;
            return Results.Json(new
            {
                name = _steam.GetPlayerName(),
                steamId = _steam.GetSteamId(),
                isOnline = _steam.IsInitialized
            });
        });

        // POST /api/steam/achievement — unlock an achievement
        app.MapPost("/api/steam/achievement", async (HttpRequest request) =>
        {
            LastHeartbeat = DateTime.UtcNow;

            try
            {
                using var doc = await JsonDocument.ParseAsync(request.Body);
                var id = doc.RootElement.GetProperty("id").GetString();

                if (string.IsNullOrWhiteSpace(id))
                    return Results.BadRequest(new { error = "Missing 'id' field." });

                var success = _steam.UnlockAchievement(id);
                return Results.Json(new { success, achievementId = id });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //

    /// <summary>Finds a free TCP port on localhost.</summary>
    private static int FindFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
