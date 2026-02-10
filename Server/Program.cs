using System.Diagnostics;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        var urls = builder.Configuration["urls"]?.Split(';').FirstOrDefault()?.Trim() ?? "http://localhost:5079";
        Process.Start(new ProcessStartInfo { FileName = urls, UseShellExecute = true });
    }
    catch { /* ignore */ }
});

app.Run();
