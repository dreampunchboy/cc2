using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace CC2.Launcher;

public sealed class GameServer
{
    private readonly SteamBridge _steam;
    private readonly string _gameFolder;
    private readonly GameEngine _game;
    private WebApplication? _app;

    public int Port { get; private set; }
    public string BaseUrl => $"http://localhost:{Port}";
    public DateTime LastHeartbeat { get; private set; } = DateTime.UtcNow;

    public GameServer(SteamBridge steam, string gameFolder)
    {
        _steam = steam;
        _gameFolder = Path.GetFullPath(gameFolder);
        _game = new GameEngine();
        _game.Load();
    }

    public async Task StartAsync()
    {
        Port = FindFreePort();
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddCors();
        builder.WebHost.UseUrls($"http://localhost:{Port}");
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);

        _app = builder.Build();
        _app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

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

        MapApiRoutes(_app);
        Console.WriteLine($"[Server] Starting on {BaseUrl}  (serving from {_gameFolder})");
        await _app.StartAsync();
    }

    public void SaveGame() => _game.Save();

    public async Task StopAsync()
    {
        if (_app is not null)
        {
            Console.WriteLine("[Server] Shutting down...");
            SaveGame();
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    private void MapApiRoutes(WebApplication app)
    {
        app.MapGet("/api/steam/status", () =>
        {
            LastHeartbeat = DateTime.UtcNow;
            return Results.Json(new { ok = true, steamConnected = _steam.IsInitialized, timestamp = DateTime.UtcNow });
        });

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

        app.MapPost("/api/steam/achievement", async (HttpRequest request) =>
        {
            LastHeartbeat = DateTime.UtcNow;
            try
            {
                using var doc = await JsonDocument.ParseAsync(request.Body);
                var id = doc.RootElement.GetProperty("id").GetString();
                if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest(new { error = "Missing 'id' field." });
                var success = _steam.UnlockAchievement(id);
                return Results.Json(new { success, achievementId = id });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapGet("/api/game/state", () =>
        {
            LastHeartbeat = DateTime.UtcNow;
            var state = _game.GetState();
            return Results.Json(state, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        });

        app.MapPost("/api/game/click", () =>
        {
            LastHeartbeat = DateTime.UtcNow;
            _game.Click();
            return Results.Ok();
        });

        app.MapPost("/api/game/buy-sheet", async (HttpRequest request) =>
        {
            LastHeartbeat = DateTime.UtcNow;
            try
            {
                using var doc = await JsonDocument.ParseAsync(request.Body);
                var sheetId = doc.RootElement.GetProperty("sheetId").GetInt32();
                var amount = doc.RootElement.TryGetProperty("amount", out var a) ? a.GetInt32() : 1;
                var (success, error) = _game.BuySheet(sheetId, amount);
                return success ? Results.Ok() : Results.BadRequest(new { error });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapPost("/api/game/buy-upgrade", async (HttpRequest request) =>
        {
            LastHeartbeat = DateTime.UtcNow;
            try
            {
                using var doc = await JsonDocument.ParseAsync(request.Body);
                var upgradeId = doc.RootElement.GetProperty("upgradeId").GetInt32();
                var (success, error) = _game.BuyUpgrade(upgradeId);
                return success ? Results.Ok() : Results.BadRequest(new { error });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapPost("/api/game/save", () =>
        {
            LastHeartbeat = DateTime.UtcNow;
            _game.Save();
            return Results.Ok();
        });
    }

    private static int FindFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
