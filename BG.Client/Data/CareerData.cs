using BG.Client.Models;

namespace BG.Client.Data;

public static class CareerData
{
    /// <summary>
    /// Career ladder: Intern (0) through CEO (7). Each entry defines the cost to promote TO that level from the previous.
    /// </summary>
    public static readonly List<CareerLevel> CareerLevels = new()
    {
        new CareerLevel { Order = 0, Id = "intern", Name = "Intern", PromoteCostResourceKey = null, PromoteCostAmount = 0 },
        new CareerLevel { Order = 1, Id = "clerk", Name = "Clerk", PromoteCostResourceKey = "emailsProcessed", PromoteCostAmount = 500 },
        new CareerLevel { Order = 2, Id = "coordinator", Name = "Coordinator", PromoteCostResourceKey = "reportsDone", PromoteCostAmount = 100 },
        new CareerLevel { Order = 3, Id = "analyst", Name = "Analyst", PromoteCostResourceKey = "reportsDone", PromoteCostAmount = 500 },
        new CareerLevel { Order = 4, Id = "manager", Name = "Manager", PromoteCostResourceKey = "reportsDone", PromoteCostAmount = 2000 },
        new CareerLevel { Order = 5, Id = "director", Name = "Director", PromoteCostResourceKey = "reportsDone", PromoteCostAmount = 5000 },
        new CareerLevel { Order = 6, Id = "vp", Name = "VP", PromoteCostResourceKey = "reportsDone", PromoteCostAmount = 10000 },
        new CareerLevel { Order = 7, Id = "ceo", Name = "CEO", PromoteCostResourceKey = "reportsDone", PromoteCostAmount = 20000 },
    };

    public static CareerLevel? GetLevel(int order) =>
        CareerLevels.FirstOrDefault(c => c.Order == order);

    public static CareerLevel? GetNextLevel(int currentOrder)
    {
        var next = currentOrder + 1;
        return next < CareerLevels.Count ? GetLevel(next) : null;
    }
}
