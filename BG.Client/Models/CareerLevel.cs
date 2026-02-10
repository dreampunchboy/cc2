namespace BG.Client.Models;

/// <summary>
/// A career tier (Intern, Clerk, ... CEO). Order 0 = Intern.
/// Cost to promote TO this level from the previous (Intern has no cost).
/// </summary>
public class CareerLevel
{
    public int Order { get; set; }
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    /// <summary>Resource key to spend (e.g. emailsProcessed). Null/empty for Intern (no cost).</summary>
    public string? PromoteCostResourceKey { get; set; }

    /// <summary>Amount required to promote to this level. 0 for Intern.</summary>
    public double PromoteCostAmount { get; set; }
}
