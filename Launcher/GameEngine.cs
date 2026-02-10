// ============================================================
//  GameEngine — Office Clicker (server-side, cheat-proof)
//  Sheets = production types; Rows = upgrades per sheet.
// ============================================================

using System.Text.Json;

namespace CC2.Launcher;

public sealed class GameEngine
{
    private readonly object _lock = new();
    private OfficeGameState _state;
    private DateTime _lastUpdateUtc;
    private readonly string _savePath;

    private static readonly SheetDef[] Sheets =
    {
        new(1, "Manual Clicks", 0, 0),
        new(2, "Reports", 10, 0.5),
        new(3, "Emails", 100, 4),
        new(4, "Meetings", 1100, 20),
        new(5, "Presentations", 12000, 100),
    };

    private static readonly UpgradeDef[] Upgrades =
    {
        new(1, 1, "Keyboard shortcut", "+0.1 click power", 50, UpgradeEffectType.ClickAdd, 0.1),
        new(2, 1, "Double-click training", "x2 click", 500, UpgradeEffectType.ClickMultiply, 2),
        new(3, 2, "Better template", "+10% Reports", 100, UpgradeEffectType.SheetRateMultiply, 1.1),
        new(4, 2, "Auto-save", "+25% Reports", 500, UpgradeEffectType.SheetRateMultiply, 1.25),
        new(5, 3, "Outlook rules", "+10% Emails", 200, UpgradeEffectType.SheetRateMultiply, 1.1),
        new(6, 3, "Signature block", "+20% Emails", 1000, UpgradeEffectType.SheetRateMultiply, 1.2),
        new(7, 4, "Agenda template", "+15% Meetings", 1500, UpgradeEffectType.SheetRateMultiply, 1.15),
        new(8, 4, "Recurring invite", "+25% Meetings", 5000, UpgradeEffectType.SheetRateMultiply, 1.25),
        new(9, 5, "Slide master", "+10% Presentations", 15000, UpgradeEffectType.SheetRateMultiply, 1.1),
        new(10, 5, "Presenter view", "+30% Presentations", 50000, UpgradeEffectType.SheetRateMultiply, 1.3),
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GameEngine()
    {
        _savePath = Path.Combine(AppContext.BaseDirectory, "office_clicker_save.json");
        _state = new OfficeGameState();
        _lastUpdateUtc = DateTime.UtcNow;
    }

    public OfficeGameStateDto GetState()
    {
        lock (_lock)
        {
            AdvanceTime();
            return ToDto();
        }
    }

    public bool Click()
    {
        lock (_lock)
        {
            AdvanceTime();
            var power = GetClickPower();
            _state.Currency += power;
            return true;
        }
    }

    public (bool success, string? error) BuySheet(int sheetId, int amount)
    {
        lock (_lock)
        {
            AdvanceTime();
            var def = Sheets.FirstOrDefault(s => s.Id == sheetId);
            if (def == null) return (false, "Invalid sheet");
            if (def.BaseCost <= 0) return (false, "Cannot buy");

            if (amount < 1 || amount > 100) return (false, "Invalid amount");
            var owned = _state.SheetOwned.GetValueOrDefault(sheetId, 0);
            var price = GetSheetPrice(sheetId, amount);
            if (_state.Currency < price) return (false, "Not enough Productivity");

            _state.Currency -= price;
            _state.SheetOwned[sheetId] = owned + amount;
            return (true, null);
        }
    }

    public (bool success, string? error) BuyUpgrade(int upgradeId)
    {
        lock (_lock)
        {
            AdvanceTime();
            var def = Upgrades.FirstOrDefault(u => u.Id == upgradeId);
            if (def == null) return (false, "Invalid upgrade");
            if (_state.PurchasedUpgrades.Contains(upgradeId)) return (false, "Already purchased");

            if (_state.Currency < def.Cost) return (false, "Not enough Productivity");

            _state.Currency -= def.Cost;
            _state.PurchasedUpgrades.Add(upgradeId);
            return (true, null);
        }
    }

    public void Save()
    {
        lock (_lock)
        {
            try
            {
                var json = JsonSerializer.Serialize(_state, JsonOptions);
                File.WriteAllText(_savePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameEngine] Save failed: {ex.Message}");
            }
        }
    }

    public void Load()
    {
        lock (_lock)
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    var json = File.ReadAllText(_savePath);
                    _state = JsonSerializer.Deserialize<OfficeGameState>(json) ?? new OfficeGameState();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameEngine] Load failed: {ex.Message}");
            }

            _state.SheetOwned ??= new Dictionary<int, int>();
            _state.PurchasedUpgrades ??= new HashSet<int>();
        }
    }

    private void AdvanceTime()
    {
        var now = DateTime.UtcNow;
        var elapsed = (now - _lastUpdateUtc).TotalSeconds;
        _lastUpdateUtc = now;
        if (elapsed <= 0) return;

        var perSecond = GetPerSecond();
        _state.Currency += perSecond * elapsed;
    }

    private double GetClickPower()
    {
        double power = 1;
        foreach (var u in Upgrades.Where(x => x.SheetId == 1 && _state.PurchasedUpgrades.Contains(x.Id)))
        {
            if (u.EffectType == UpgradeEffectType.ClickAdd) power += u.EffectValue;
            if (u.EffectType == UpgradeEffectType.ClickMultiply) power *= u.EffectValue;
        }
        return power;
    }

    private double GetPerSecond()
    {
        double total = 0;
        foreach (var sheet in Sheets)
        {
            if (sheet.BaseRate <= 0) continue;
            var owned = _state.SheetOwned.GetValueOrDefault(sheet.Id, 0);
            if (owned <= 0) continue;
            var mult = 1.0;
            foreach (var u in Upgrades.Where(x => x.SheetId == sheet.Id && _state.PurchasedUpgrades.Contains(x.Id)))
            {
                if (u.EffectType == UpgradeEffectType.SheetRateMultiply) mult *= u.EffectValue;
            }
            total += sheet.BaseRate * owned * mult;
        }
        return total;
    }

    private double GetSheetPrice(int sheetId, int amount)
    {
        var def = Sheets.FirstOrDefault(s => s.Id == sheetId);
        if (def == null || def.BaseCost <= 0) return double.MaxValue;
        var owned = _state.SheetOwned.GetValueOrDefault(sheetId, 0);
        double total = 0;
        for (var i = 0; i < amount; i++)
            total += def.BaseCost * Math.Pow(1.15, owned + i);
        return total;
    }

    private OfficeGameStateDto ToDto()
    {
        var sheetDtos = Sheets.Select(s => new SheetStateDto
        {
            Id = s.Id,
            Name = s.Name,
            Owned = _state.SheetOwned.GetValueOrDefault(s.Id, 0),
            NextCost = s.BaseCost > 0 ? GetSheetPrice(s.Id, 1) : 0,
            BaseRate = s.BaseRate,
        }).ToList();

        var upgradeDtos = Upgrades.Select(u => new UpgradeStateDto
        {
            Id = u.Id,
            SheetId = u.SheetId,
            Name = u.Name,
            Description = u.Description,
            Cost = u.Cost,
            Purchased = _state.PurchasedUpgrades.Contains(u.Id),
        }).ToList();

        return new OfficeGameStateDto
        {
            Currency = _state.Currency,
            PerSecond = GetPerSecond(),
            ClickPower = GetClickPower(),
            Sheets = sheetDtos,
            Upgrades = upgradeDtos,
        };
    }

    private record SheetDef(int Id, string Name, double BaseCost, double BaseRate);

    private record UpgradeDef(int Id, int SheetId, string Name, string Description, double Cost, UpgradeEffectType EffectType, double EffectValue);
}

public enum UpgradeEffectType
{
    ClickAdd,
    ClickMultiply,
    SheetRateMultiply,
}

public record OfficeGameStateDto
{
    public double Currency { get; init; }
    public double PerSecond { get; init; }
    public double ClickPower { get; init; }
    public List<SheetStateDto> Sheets { get; init; } = [];
    public List<UpgradeStateDto> Upgrades { get; init; } = [];
}

public record SheetStateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public int Owned { get; init; }
    public double NextCost { get; init; }
    public double BaseRate { get; init; }
}

public record UpgradeStateDto
{
    public int Id { get; init; }
    public int SheetId { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public double Cost { get; init; }
    public bool Purchased { get; init; }
}

public class OfficeGameState
{
    public double Currency { get; set; }
    public Dictionary<int, int> SheetOwned { get; set; } = new();
    public HashSet<int> PurchasedUpgrades { get; set; } = new();
}
