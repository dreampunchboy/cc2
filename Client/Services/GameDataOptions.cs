namespace BG.Client.Services;

/// <summary>
/// Path to the data folder (containing career-levels.json, task-tabs.json, game-config.json)
/// or to a single game-data.json file. Configured via appsettings "GameData:DataPath" (e.g. "wwwroot/data").
/// </summary>
public class GameDataOptions
{
    public const string SectionName = "GameData";
    public string? DataPath { get; set; }
}
