# Spreadsheet Clicker (BG – Browser Game)

Office clicker game built with Blazor WebAssembly (hosted). Excel-style grid UI; tabs = office tasks (Emails, Reports); rows = purchasable upgrades. Runs in the browser. Steam runs the host exe, which serves the site and opens the default browser.

## Run (development)

Open the solution in **Visual Studio** and run the **Server** project (F5). The host starts and opens the browser.

Or from the command line:

```bash
dotnet run --project Server
```

(Use `--launch-profile BG.Server` for port 5079.)

## Build

```bash
dotnet build BG.sln
```

## Publish to Steam Content Builder

```bash
publish.bat
```

Builds the Server project (Release) and publishes output to `C:\Software\steamworks_sdk\sdk\tools\ContentBuilder\content`. That folder contains `BG.Server.exe` (or the host executable name) and the Blazor Client files. Running the exe starts the HTTP server and opens the default browser to http://localhost:5000.

## Stack

- **Client:** Blazor WebAssembly (.NET 8)
- **Host:** ASP.NET Core (.NET 8) – serves the Blazor app and opens the browser
- **UI:** CSS Grid (Excel-style), plain CSS
- **Persistence:** (planned) localStorage / file save
