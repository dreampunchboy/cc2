using BG.Client.Models;

namespace BG.Client.Services;

public interface IGameDataService
{
    IReadOnlyList<CareerLevel> CareerLevels { get; }
    IReadOnlyList<TaskTab> TaskTabs { get; }
    GameConfig? Config { get; }

    CareerLevel? GetLevel(int order);
    CareerLevel? GetNextLevel(int currentOrder);
}
