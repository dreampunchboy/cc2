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
  steam_appid.txt    (dev-only; do not include in Steam builds)
```

### 3. Steam Integration

- `steam_appid.txt` contains your real app ID `721450` for **local development when running the launcher directly** (not via Steam).
- When launching the game via the **Steam client**, Steam provides the App ID automatically and `steam_appid.txt` is ignored.
- For **shipping Steam builds**, remove `steam_appid.txt` from the final package so your live app ID only comes from Steam.

### 4. Steam publishing workflow (app 721450)

For live testing and release on your real Steam app:

1. **Build the launcher**
   - From the `Launcher/` folder:
   ```bash
   cd Launcher
   dotnet publish -c Release -r win-x64 --self-contained true -o ../publish
   ```
2. **Assemble the Steam content folder**
   - Ensure the output looks like:
   ```
   publish/
     CC2.Launcher.exe
     Game/
       index.html
       css/
       js/
   ```
   - Do **not** include `steam_appid.txt` in this folder when uploading to Steam.
3. **Configure Steamworks (app 721450)**
   - In the Steamworks partner site, set the app's **launch executable** to `CC2.Launcher.exe` at the root of the depot.
   - Create a **Windows depot** that uses the `publish/` folder as its content root.
4. **Use SteamPipe scripts (templates in `steam/`)**
   - Edit `steam/app_build_721450.vdf` and `steam/depot_build_721450_win.vdf` to match your depot ID and desired branch.
   - Run `steamcmd` with `app_build_721450.vdf` to upload a new build.
5. **Test live via Steam**
   - From your Steam Library, install/update app `721450` and launch it.
   - The game should run **online through Steam**, and the launcher/REST API will see real Steam user/achievement data instead of mock values.

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
