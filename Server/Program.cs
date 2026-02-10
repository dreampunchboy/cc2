using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using BG.Client.Components;
using BG.Client.Services;

namespace BG.Server;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddSingleton<IGameDataService, GameDataService>();
        builder.Services.AddOptions<GameDataOptions>()
            .Bind(builder.Configuration.GetSection(GameDataOptions.SectionName))
            .Configure<IWebHostEnvironment>((opt, env) =>
            {
                if (!string.IsNullOrWhiteSpace(opt.DataPath))
                {
                    if (!Path.IsPathRooted(opt.DataPath))
                        opt.DataPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, opt.DataPath));
                    return;
                }
                var clientDataFolder = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "Client", "wwwroot", "data"));
                if (Directory.Exists(clientDataFolder))
                    opt.DataPath = clientDataFolder;
            });
        builder.Services.AddScoped<GameStateService>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
        }
        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

        app.MapStaticAssets();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.MapRazorPages();
        app.MapControllers();

        var appUrl = builder.Configuration["urls"]?.Split(';').FirstOrDefault()?.Trim() ?? "http://localhost:5079";
        var (browserUrl, customHost) = ResolveBrowserUrl(appUrl);
        var appDir = AppContext.BaseDirectory;
        var hostSetupPath = Path.Combine(appDir, "HostSetup.exe");

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                var urlToOpen = browserUrl;
                if (customHost != null && !CustomHostResolves(customHost) && File.Exists(hostSetupPath))
                {
                    if (TryRunHostSetup(hostSetupPath, customHost))
                        urlToOpen = appUrl;
                }
                Process.Start(new ProcessStartInfo { FileName = urlToOpen, UseShellExecute = true });
            }
            catch { /* ignore */ }
        });

        // System tray icon and context menu
        using var trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "CC2 Server"
        };

        string GetUrlForBrowser()
        {
            var url = ResolveBrowserUrl(appUrl).browserUrl;
            if (customHost != null && !CustomHostResolves(customHost) && File.Exists(hostSetupPath))
            {
                if (TryRunHostSetup(hostSetupPath, customHost))
                    url = appUrl;
            }
            return url;
        }

        var openItem = new ToolStripMenuItem("Open in browser");
        openItem.Click += (_, _) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = GetUrlForBrowser(), UseShellExecute = true });
            }
            catch { /* ignore */ }
        };

        var setupUrlLabel = customHost != null && Uri.TryCreate(appUrl, UriKind.Absolute, out var uri)
            ? $"Set up custom URL ({uri.Host}:{uri.Port})..."
            : "Set up custom URL...";
        var setupUrlItem = new ToolStripMenuItem(setupUrlLabel);
        setupUrlItem.Click += (_, _) =>
        {
            if (!File.Exists(hostSetupPath))
                return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = hostSetupPath,
                    Arguments = customHost ?? "CC2",
                    UseShellExecute = true
                });
            }
            catch { /* ignore */ }
        };

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) =>
        {
            trayIcon.Visible = false;
            app.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
            Application.Exit();
        };

        trayIcon.ContextMenuStrip = new ContextMenuStrip();
        trayIcon.ContextMenuStrip.Items.Add(openItem);
        trayIcon.ContextMenuStrip.Items.Add(setupUrlItem);
        trayIcon.ContextMenuStrip.Items.Add(exitItem);

        trayIcon.DoubleClick += (_, _) => openItem.PerformClick();

        // Run web host in background; tray message loop on main thread
        _ = Task.Run(() => app.Run());

        Application.Run(new ApplicationContext());
    }

    /// <summary>Parse configured URL and return the URL to open in the browser plus the custom hostname (if any).</summary>
    private static (string browserUrl, string? customHost) ResolveBrowserUrl(string appUrl)
    {
        if (!Uri.TryCreate(appUrl, UriKind.Absolute, out var uri) || uri.Host == null)
            return (appUrl, null);
        var isLocalhost = string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || uri.Host == "127.0.0.1";
        if (isLocalhost)
            return (appUrl, null);
        var fallback = $"http://localhost:{uri.Port}";
        return CustomHostResolves(uri.Host) ? (appUrl, uri.Host) : (fallback, uri.Host);
    }

    private static bool CustomHostResolves(string host)
    {
        try
        {
            var entry = Dns.GetHostEntry(host);
            return entry.AddressList.Any(a => a.Equals(IPAddress.Loopback) || a.Equals(IPAddress.IPv6Loopback));
        }
        catch
        {
            return false;
        }
    }

    private static bool TryRunHostSetup(string hostSetupPath, string hostname)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = hostSetupPath,
                ArgumentList = { hostname },
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit(TimeSpan.FromSeconds(15));
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
