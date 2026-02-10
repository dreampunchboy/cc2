namespace BG.Client.Models;

/// <summary>
/// Root DTO for game-data.json. Career levels, task tabs (with upgrades), and optional config.
/// </summary>
public class GameDataRoot
{
    public int Version { get; set; }
    public List<CareerLevel> CareerLevels { get; set; } = new();
    public List<TaskTab> TaskTabs { get; set; } = new();
    public GameConfig? GameConfig { get; set; }
}
