namespace BG.Client.Models;

public class Upgrade
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public double BaseCost { get; set; }
    public double CostMultiplier { get; set; }
    public int Owned { get; set; }

    /// <summary>Amount of Productivity generated per effectTime. Replaces effectPerUnit.</summary>
    public double EffectAmount { get; set; }
    /// <summary>Time in seconds for effectAmount. Displayed as "X / Y sec".</summary>
    public double EffectTime { get; set; } = 1;

    /// <summary>Legacy: per-second rate. Used if EffectAmount/EffectTime not set (effectTime defaults 1).</summary>
    public double EffectPerUnit { get; set; }

    /// <summary>Productivity per second = EffectAmount/EffectTime (or EffectPerUnit if legacy).</summary>
    public double RatePerSecond => EffectAmount > 0 && EffectTime > 0 ? EffectAmount / EffectTime : EffectPerUnit;

    /// <summary>Upgrade row is visible when CurrentRoleOrder >= UnlockAtRoleOrder.</summary>
    public int UnlockAtRoleOrder { get; set; }
}
