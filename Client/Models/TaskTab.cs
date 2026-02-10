namespace BG.Client.Models;

public class TaskTab
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ResourceKey { get; set; } = "";
    public string PerSecondKey { get; set; } = "";
    /// <summary>Tab is visible when CurrentRoleOrder >= UnlockAtRoleOrder.</summary>
    public int UnlockAtRoleOrder { get; set; }
    public List<Upgrade> Upgrades { get; set; } = new();
}
