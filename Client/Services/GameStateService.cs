using BG.Client.Models;
using System.Timers;

namespace BG.Client.Services;

public class GameStateService : IDisposable
{
    public const string ProductivityKey = "productivity";
    public const string ProductivityPerSecondKey = "productivityPerSecond";

    private readonly IGameDataService _gameData;
    private readonly System.Timers.Timer _timer;
    private PerSecond _perSecondCache = new();

    public GameResources Resources { get; }
    public Dictionary<string, int> OwnedByUpgradeId { get; } = new();
    /// <summary>Per-upgrade generation progress (0-1) for the tick timer display. Only nonzero when owned > 0.</summary>
    public IReadOnlyDictionary<string, double> UpgradeGenerationProgress => _upgradeGenerationProgress;
    private readonly Dictionary<string, double> _upgradeGenerationProgress = new();

    public string ActiveTabId { get; set; } = "inbox";

    /// <summary>Current career tier (0 = Intern, 7 = CEO). Gates which tabs and upgrades are visible.</summary>
    public int CurrentRoleOrder { get; set; }

    public event Action? OnChange;

    public GameStateService(IGameDataService gameData)
    {
        _gameData = gameData;
        Resources = new GameResources();
        var config = _gameData.Config?.StartingResources;
        var start = config?.GetValueOrDefault(ProductivityKey) ?? 20;
        Resources.Set(ProductivityKey, start);
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += Tick;
        _timer.Start();
    }

    public PerSecond ComputePerSecond()
    {
        var ps = new PerSecond();
        double totalRate = 0;
        foreach (var tab in _gameData.TaskTabs)
        {
            foreach (var up in tab.Upgrades)
            {
                var owned = OwnedByUpgradeId.GetValueOrDefault(up.Id, 0);
                totalRate += owned * up.RatePerSecond;
            }
        }
        ps.Set(ProductivityPerSecondKey, totalRate);
        _perSecondCache = ps;
        return ps;
    }

    public PerSecond PerSecond => _perSecondCache;

    private void Tick(object? sender, ElapsedEventArgs e)
    {
        var ps = ComputePerSecond();
        Resources.Add(ProductivityKey, ps.Get(ProductivityPerSecondKey));

        // Advance generation-cycle progress for each owned upgrade (tick timer for progress bar)
        var effectTimeFallback = 1.0;
        foreach (var tab in _gameData.TaskTabs)
        {
            foreach (var up in tab.Upgrades)
            {
                var owned = OwnedByUpgradeId.GetValueOrDefault(up.Id, 0);
                if (owned == 0) continue;
                var cycleTime = up.EffectAmount > 0 && up.EffectTime > 0 ? up.EffectTime : effectTimeFallback;
                var inc = 1.0 / cycleTime;
                var current = _upgradeGenerationProgress.GetValueOrDefault(up.Id, 0);
                current += inc;
                if (current >= 1.0) current -= 1.0;
                _upgradeGenerationProgress[up.Id] = current;
            }
        }

        NotifyChange();
    }

    public void Buy(string upgradeId, double cost)
    {
        var tab = _gameData.TaskTabs.FirstOrDefault(t => t.Upgrades.Any(u => u.Id == upgradeId));
        if (tab == null) return;

        var current = Resources.Get(ProductivityKey);
        if (current < cost) return;

        Resources.Subtract(ProductivityKey, cost);
        OwnedByUpgradeId[upgradeId] = OwnedByUpgradeId.GetValueOrDefault(upgradeId, 0) + 1;
        NotifyChange();
    }

    public static int NextCost(double baseCost, double costMultiplier, int owned) =>
        (int)Math.Floor(baseCost * Math.Pow(costMultiplier, owned));

    /// <summary>Process one manual click – adds 1 Productivity.</summary>
    public void ProcessClick(string resourceKey)
    {
        Resources.Add(ProductivityKey, 1);
        NotifyChange();
    }

    /// <summary>Tabs visible at current career level.</summary>
    public IReadOnlyList<TaskTab> GetVisibleTabs() =>
        _gameData.TaskTabs.Where(t => t.UnlockAtRoleOrder <= CurrentRoleOrder).ToList();

    /// <summary>Upgrades visible at current career level for a tab.</summary>
    public IReadOnlyList<Upgrade> GetVisibleUpgrades(TaskTab tab) =>
        tab.Upgrades.Where(u => u.UnlockAtRoleOrder <= CurrentRoleOrder).ToList();

    /// <summary>Whether the player can afford to promote to the next role.</summary>
    public bool CanPromote(out CareerLevel? nextLevel, out double cost, out string? costResourceKey)
    {
        nextLevel = _gameData.GetNextLevel(CurrentRoleOrder);
        costResourceKey = null;
        cost = 0;
        if (nextLevel == null) return false;
        costResourceKey = nextLevel.PromoteCostResourceKey;
        cost = nextLevel.PromoteCostAmount;
        if (string.IsNullOrEmpty(costResourceKey) || cost <= 0) return false;
        return Resources.Get(costResourceKey) >= cost;
    }

    /// <summary>Spend resources and advance to the next career level. Returns true if promoted.</summary>
    public bool Promote()
    {
        if (!CanPromote(out var nextLevel, out var cost, out var costResourceKey) || nextLevel == null || string.IsNullOrEmpty(costResourceKey))
            return false;
        Resources.Subtract(costResourceKey, cost);
        CurrentRoleOrder = nextLevel.Order;
        NotifyChange();
        return true;
    }

    private void NotifyChange() => OnChange?.Invoke();

    public void Dispose() => _timer.Dispose();
}
