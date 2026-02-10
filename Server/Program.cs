using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Hosting;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        var appUrl = builder.Configuration["urls"]?.Split(';').FirstOrDefault()?.Trim() ?? "http://localhost:5079";

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = appUrl, UseShellExecute = true });
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

        var openItem = new ToolStripMenuItem("Open in browser");
        openItem.Click += (_, _) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = appUrl, UseShellExecute = true });
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
        trayIcon.ContextMenuStrip.Items.Add(exitItem);

        trayIcon.DoubleClick += (_, _) => openItem.PerformClick();

        // Run web host in background; tray message loop on main thread
        _ = Task.Run(() => app.Run());

        Application.Run(new ApplicationContext());
    }
}
