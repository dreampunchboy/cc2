using BG.Client.Data;
using BG.Client.Models;
using System.Timers;

namespace BG.Client.Services;

public class GameStateService : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private PerSecond _perSecondCache = new();

    public GameResources Resources { get; } = new() { EmailsProcessed = 20, ReportsDone = 0 };
    public Dictionary<string, int> OwnedByUpgradeId { get; } = new();
    public string ActiveTabId { get; set; } = "emails";

    public event Action? OnChange;

    public GameStateService()
    {
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += Tick;
        _timer.Start();
    }

    public PerSecond ComputePerSecond()
    {
        double emailsPerSecond = 0, reportsPerSecond = 0;
        foreach (var tab in TabsData.TaskTabs)
        {
            foreach (var up in tab.Upgrades)
            {
                var owned = OwnedByUpgradeId.GetValueOrDefault(up.Id, 0);
                if (tab.PerSecondKey == "emailsPerSecond")
                    emailsPerSecond += owned * up.EffectPerUnit;
                else
                    reportsPerSecond += owned * up.EffectPerUnit;
            }
        }
        _perSecondCache = new PerSecond { EmailsPerSecond = emailsPerSecond, ReportsPerSecond = reportsPerSecond };
        return _perSecondCache;
    }

    public PerSecond PerSecond => _perSecondCache;

    private void Tick(object? sender, ElapsedEventArgs e)
    {
        var ps = ComputePerSecond();
        Resources.EmailsProcessed += ps.EmailsPerSecond;
        Resources.ReportsDone += ps.ReportsPerSecond;
        NotifyChange();
    }

    public void Buy(string upgradeId, double cost)
    {
        var tab = TabsData.TaskTabs.FirstOrDefault(t => t.Upgrades.Any(u => u.Id == upgradeId));
        if (tab == null) return;

        var current = tab.ResourceKey == "emailsProcessed" ? Resources.EmailsProcessed : Resources.ReportsDone;
        if (current < cost) return;

        if (tab.ResourceKey == "emailsProcessed")
            Resources.EmailsProcessed -= cost;
        else
            Resources.ReportsDone -= cost;

        OwnedByUpgradeId[upgradeId] = OwnedByUpgradeId.GetValueOrDefault(upgradeId, 0) + 1;
        NotifyChange();
    }

    public static int NextCost(double baseCost, double costMultiplier, int owned) =>
        (int)Math.Floor(baseCost * Math.Pow(costMultiplier, owned));

    private void NotifyChange() => OnChange?.Invoke();

    public void Dispose() => _timer.Dispose();
}
