namespace BG.Client.Models;

/// <summary>
/// Optional game configuration from game-data.json (e.g. starting resources).
/// </summary>
public class GameConfig
{
    public Dictionary<string, double> StartingResources { get; set; } = new();
}
