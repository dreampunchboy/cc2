using System.Diagnostics;
using System.Runtime.InteropServices;
using CC2.Launcher;

// ================================================================
//  CC2 Launcher — Entry Point
//  Initializes Steam, starts the local web server, opens the
//  browser, and pumps Steam callbacks until shutdown.
// ================================================================

Console.WriteLine("=== CC2 Launcher ===");

// --- Resolve the Game/ folder (sibling to Launcher/) ---
string exeDir = AppContext.BaseDirectory;
string gameFolder = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", "..", "Game"));

// When running via `dotnet run` the exe is deep in bin/; also support
// a Game/ folder next to the exe (for published builds).
if (!Directory.Exists(gameFolder))
{
    gameFolder = Path.GetFullPath(Path.Combine(exeDir, "..", "Game"));
}
if (!Directory.Exists(gameFolder))
{
    gameFolder = Path.GetFullPath(Path.Combine(exeDir, "Game"));
}

Console.WriteLine($"[Launcher] Game folder: {gameFolder}");

// --- Initialize Steam ---
using var steam = new SteamBridge();
steam.Initialize();

// --- Start the HTTP server ---
var server = new GameServer(steam, gameFolder);
await server.StartAsync();

// --- Open the default browser ---
OpenBrowser(server.BaseUrl);
Console.WriteLine($"[Launcher] Browser opened to {server.BaseUrl}");
Console.WriteLine("[Launcher] Press Ctrl+C to quit.");

// --- Graceful shutdown on Ctrl+C ---
var shutdownCts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // prevent immediate process kill
    Console.WriteLine("[Launcher] Ctrl+C received, shutting down...");
    shutdownCts.Cancel();
};

// Also handle process exit (e.g. Steam stopping the game)
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    shutdownCts.Cancel();
};

// --- Main loop: pump Steam callbacks at ~15 Hz, periodic game save ---
const int callbackIntervalMs = 66; // ~15 Hz
const int heartbeatTimeoutSeconds = 120; // auto-close if browser gone for 2 min
var lastSave = DateTime.UtcNow;

try
{
    while (!shutdownCts.Token.IsCancellationRequested)
    {
        steam.RunCallbacks();

        // Periodic game save every 30 seconds
        if ((DateTime.UtcNow - lastSave).TotalSeconds > 30)
        {
            server.SaveGame();
            lastSave = DateTime.UtcNow;
        }

        // Optional: auto-shutdown if browser stops calling the API
        var elapsed = DateTime.UtcNow - server.LastHeartbeat;
        if (elapsed.TotalSeconds > heartbeatTimeoutSeconds)
        {
            Console.WriteLine($"[Launcher] No browser heartbeat for {heartbeatTimeoutSeconds}s — shutting down.");
            break;
        }

        await Task.Delay(callbackIntervalMs, shutdownCts.Token);
    }
}
catch (OperationCanceledException)
{
    // Expected on Ctrl+C
}

// --- Cleanup ---
await server.StopAsync();
steam.Dispose();
Console.WriteLine("[Launcher] Goodbye!");

// ================================================================
//  Helpers
// ================================================================

/// <summary>Opens the default browser on Windows, macOS, or Linux.</summary>
static void OpenBrowser(string url)
{
    try
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else
        {
            Process.Start("xdg-open", url);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Launcher] Could not open browser: {ex.Message}");
        Console.WriteLine($"[Launcher] Please manually navigate to: {url}");
    }
}
