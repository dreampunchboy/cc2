using Microsoft.AspNetCore.Hosting;
using BG.Client.Components;
using BG.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<IGameDataService, GameDataService>();
builder.Services.AddOptions<GameDataOptions>()
    .Bind(builder.Configuration.GetSection(GameDataOptions.SectionName))
    .Configure<IWebHostEnvironment>((opt, env) =>
    {
        if (string.IsNullOrWhiteSpace(opt.DataPath))
        {
            opt.DataPath = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "data");
            return;
        }
        if (!Path.IsPathRooted(opt.DataPath))
            opt.DataPath = Path.Combine(env.ContentRootPath, opt.DataPath);
    });
builder.Services.AddScoped<GameStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
