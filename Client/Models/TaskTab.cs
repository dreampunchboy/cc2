namespace BG.Client.Models;

public class TaskTab
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ResourceKey { get; set; } = ""; // "emailsProcessed" | "reportsDone"
    public string PerSecondKey { get; set; } = ""; // "emailsPerSecond" | "reportsPerSecond"
    public List<Upgrade> Upgrades { get; set; } = new();
}
