# CC2 — Steam Browser Game Prototype

A Steam game that launches a local web server and opens the player's browser to play a Phaser 3 2D game. The C# launcher bridges the browser game to the Steamworks API for achievements, user info, and more.

## Architecture

```
Steam Client  ──launches──>  C# Launcher (.exe)
                                 │
                         starts Kestrel HTTP server
                         opens default browser
                                 │
                    Browser  <──serves──  localhost:{port}
                       │                      │
                       └── REST API calls ────┘
                                 │
                          Steamworks.NET ──> Steam API
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or later)
- A modern web browser (Chrome, Edge, Firefox)
- Steam client (optional for development — the game runs in offline/mock mode without it)

## Quick Start

### 1. Run in Development

```bash
cd Launcher
dotnet run
```

This will:
- Initialize Steamworks (or fall back to mock mode if Steam isn't running)
- Start a local HTTP server on a random free port
- Open your default browser to the game

### 2. Build for Distribution

```bash
cd Launcher
dotnet publish -c Release -r win-x64 --self-contained true -o ../publish
```

Then copy the `Game/` folder next to the published output:

```
publish/
  CC2.Launcher.exe
  Game/
    index.html
    css/
    js/
  steam_appid.txt    (remove for production — use Steamworks app config instead)
```

### 3. Steam Integration

- `steam_appid.txt` contains `480` (Valve's Spacewar test app) for local development
- For production, replace `480` with your real Steam App ID
- Remove `steam_appid.txt` entirely when shipping — Steam provides the app ID at runtime

## Project Structure

```
CC2/
  Launcher/                    C# .NET 8 Console App
    CC2.Launcher.csproj        Project file
    Program.cs                 Entry point (server + browser + Steam loop)
    SteamBridge.cs             Steamworks.NET wrapper
    GameServer.cs              Kestrel HTTP server + REST API
  Game/                        Browser game (static files)
    index.html                 Main HTML page
    css/style.css              Fullscreen canvas styling
    js/game.js                 Phaser 3 game scenes
    js/steam-bridge.js         Client-side REST API wrapper
  steam_appid.txt              Dev-only Steam App ID
```

## API Endpoints

The launcher exposes these REST endpoints on `localhost`:

| Method | Path | Description |
|--------|------|-------------|
| `GET`  | `/api/steam/status` | Health check + Steam connection status |
| `GET`  | `/api/steam/user` | Current player name and Steam ID |
| `POST` | `/api/steam/achievement` | Unlock an achievement (`{ "id": "ACH_NAME" }`) |

## Notes

- **No Steam Overlay**: The Steam overlay only works with DirectX/OpenGL/Vulkan renderers. Since the game runs in an external browser, overlay won't appear. Achievements and playtime tracking still work via the API bridge.
- **Firewall**: The server binds to `localhost` only — no firewall prompts, no external network access.
- **Heartbeat**: The browser sends periodic pings to `/api/steam/status`. If the launcher doesn't receive a heartbeat for 2 minutes, it auto-shuts down.
