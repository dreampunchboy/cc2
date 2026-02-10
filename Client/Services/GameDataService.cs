using System.Text.Json;
using BG.Client.Models;
using Microsoft.Extensions.Options;

namespace BG.Client.Services;

public class GameDataService : IGameDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private const string CareerLevelsFile = "career-levels.json";
    private const string TaskTabsFile = "task-tabs.json";
    private const string GameConfigFile = "game-config.json";
    private const string SingleDataFile = "game-data.json";

    private readonly GameDataRoot _data;

    public GameDataService(IOptions<GameDataOptions>? options = null)
    {
        _data = Load(options?.Value?.DataPath);
    }

    public IReadOnlyList<CareerLevel> CareerLevels => _data.CareerLevels;
    public IReadOnlyList<TaskTab> TaskTabs => _data.TaskTabs;
    public GameConfig? Config => _data.GameConfig;

    public CareerLevel? GetLevel(int order) =>
        _data.CareerLevels.FirstOrDefault(c => c.Order == order);

    public CareerLevel? GetNextLevel(int currentOrder)
    {
        var next = currentOrder + 1;
        return next < _data.CareerLevels.Count ? GetLevel(next) : null;
    }

    private static GameDataRoot Load(string? dataPath)
    {
        var pathsToTry = new List<string>();
        if (!string.IsNullOrWhiteSpace(dataPath))
            pathsToTry.Add(dataPath.Trim());
        var baseDir = AppContext.BaseDirectory;
        pathsToTry.Add(Path.Combine(baseDir, "wwwroot", "data"));
        pathsToTry.Add(Path.Combine(baseDir, "data"));

        foreach (var path in pathsToTry)
        {
            if (string.IsNullOrEmpty(path)) continue;

            if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(path))
                {
                    var root = LoadSingleFile(path);
                    if (root != null) return root;
                }
                continue;
            }

            if (Directory.Exists(path))
            {
                var root = LoadFromFolder(path);
                if (root != null) return root;
            }
        }

        var tried = string.Join(", ", pathsToTry.Where(p => !string.IsNullOrEmpty(p)));
        throw new InvalidOperationException(
            $"Game data not found or invalid. DataPath should be a folder containing {CareerLevelsFile}, {TaskTabsFile}, {GameConfigFile}, or a single {SingleDataFile}. Tried: {tried}.");
    }

    private static GameDataRoot? LoadSingleFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var root = JsonSerializer.Deserialize<GameDataRoot>(json, JsonOptions);
        if (root?.CareerLevels?.Count > 0 && root.TaskTabs?.Count > 0)
            return root;
        return null;
    }

    private static GameDataRoot? LoadFromFolder(string folderPath)
    {
        var careerPath = Path.Combine(folderPath, CareerLevelsFile);
        var taskTabsPath = Path.Combine(folderPath, TaskTabsFile);
        var configPath = Path.Combine(folderPath, GameConfigFile);

        if (!File.Exists(careerPath) || !File.Exists(taskTabsPath))
            return null;

        var careerJson = File.ReadAllText(careerPath);
        var taskTabsJson = File.ReadAllText(taskTabsPath);
        var careerLevels = JsonSerializer.Deserialize<List<CareerLevel>>(careerJson, JsonOptions);
        var taskTabs = JsonSerializer.Deserialize<List<TaskTab>>(taskTabsJson, JsonOptions);

        if (careerLevels == null || careerLevels.Count == 0 || taskTabs == null || taskTabs.Count == 0)
            return null;

        GameConfig? config = null;
        if (File.Exists(configPath))
        {
            var configJson = File.ReadAllText(configPath);
            config = JsonSerializer.Deserialize<GameConfig>(configJson, JsonOptions);
        }

        return new GameDataRoot
        {
            Version = 1,
            CareerLevels = careerLevels,
            TaskTabs = taskTabs,
            GameConfig = config,
        };
    }
}
