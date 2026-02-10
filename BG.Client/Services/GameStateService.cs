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

    /// <summary>Current career tier (0 = Intern, 7 = CEO). Gates which tabs and upgrades are visible.</summary>
    public int CurrentRoleOrder { get; set; }

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

    /// <summary>Tabs visible at current career level.</summary>
    public IReadOnlyList<TaskTab> GetVisibleTabs() =>
        TabsData.TaskTabs.Where(t => t.UnlockAtRoleOrder <= CurrentRoleOrder).ToList();

    /// <summary>Upgrades visible at current career level for a tab.</summary>
    public IReadOnlyList<Upgrade> GetVisibleUpgrades(TaskTab tab) =>
        tab.Upgrades.Where(u => u.UnlockAtRoleOrder <= CurrentRoleOrder).ToList();

    /// <summary>Whether the player can afford to promote to the next role.</summary>
    public bool CanPromote(out CareerLevel? nextLevel, out double cost, out string? costResourceKey)
    {
        nextLevel = CareerData.GetNextLevel(CurrentRoleOrder);
        costResourceKey = null;
        cost = 0;
        if (nextLevel == null) return false;
        costResourceKey = nextLevel.PromoteCostResourceKey;
        cost = nextLevel.PromoteCostAmount;
        if (string.IsNullOrEmpty(costResourceKey) || cost <= 0) return false;
        var current = costResourceKey == "emailsProcessed" ? Resources.EmailsProcessed : Resources.ReportsDone;
        return current >= cost;
    }

    /// <summary>Spend resources and advance to the next career level. Returns true if promoted.</summary>
    public bool Promote()
    {
        if (!CanPromote(out var nextLevel, out var cost, out var costResourceKey) || nextLevel == null || string.IsNullOrEmpty(costResourceKey))
            return false;
        if (costResourceKey == "emailsProcessed")
            Resources.EmailsProcessed -= cost;
        else
            Resources.ReportsDone -= cost;
        CurrentRoleOrder = nextLevel.Order;
        NotifyChange();
        return true;
    }

    private void NotifyChange() => OnChange?.Invoke();

    public void Dispose() => _timer.Dispose();
}
