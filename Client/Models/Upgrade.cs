namespace BG.Client.Models;

public class Upgrade
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public double BaseCost { get; set; }
    public double CostMultiplier { get; set; }
    public int Owned { get; set; }
    public double EffectPerUnit { get; set; }
    /// <summary>Upgrade row is visible when CurrentRoleOrder >= UnlockAtRoleOrder.</summary>
    public int UnlockAtRoleOrder { get; set; }
}
