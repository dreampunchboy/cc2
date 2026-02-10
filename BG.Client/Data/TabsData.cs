using BG.Client.Models;

namespace BG.Client.Data;

public static class TabsData
{
    private static readonly List<Upgrade> EmailsUpgrades = new()
    {
        new Upgrade { Id = "junior-assistant", Name = "Junior Assistant", Description = "Processes emails faster", BaseCost = 15, CostMultiplier = 1.15, Owned = 0, EffectPerUnit = 0.1, UnlockAtRoleOrder = 0 },
        new Upgrade { Id = "inbox-rules", Name = "Inbox Rules", Description = "Auto-sort and flag", BaseCost = 50, CostMultiplier = 1.2, Owned = 0, EffectPerUnit = 0.3, UnlockAtRoleOrder = 0 },
        new Upgrade { Id = "outlook-macros", Name = "Outlook Macros", Description = "Batch actions", BaseCost = 200, CostMultiplier = 1.2, Owned = 0, EffectPerUnit = 1.0, UnlockAtRoleOrder = 0 },
        new Upgrade { Id = "ai-auto-reply", Name = "AI Auto-Reply", Description = "AI drafts replies", BaseCost = 800, CostMultiplier = 1.25, Owned = 0, EffectPerUnit = 5.0, UnlockAtRoleOrder = 1 },
        new Upgrade { Id = "offshore-team", Name = "Offshore Team", Description = "24/7 email handling", BaseCost = 4000, CostMultiplier = 1.3, Owned = 0, EffectPerUnit = 25, UnlockAtRoleOrder = 1 },
    };

    private static readonly List<Upgrade> ReportsUpgrades = new()
    {
        new Upgrade { Id = "template-library", Name = "Template Library", Description = "Reuse headers and sections", BaseCost = 100, CostMultiplier = 1.12, Owned = 0, EffectPerUnit = 0.3, UnlockAtRoleOrder = 1 },
        new Upgrade { Id = "pivot-tables", Name = "Pivot Tables", Description = "Turn data into tables faster", BaseCost = 250, CostMultiplier = 1.15, Owned = 0, EffectPerUnit = 0.8, UnlockAtRoleOrder = 1 },
        new Upgrade { Id = "bi-dashboard", Name = "BI Dashboard", Description = "One-click refresh, charts", BaseCost = 800, CostMultiplier = 1.2, Owned = 0, EffectPerUnit = 2.5, UnlockAtRoleOrder = 2 },
        new Upgrade { Id = "data-warehouse-link", Name = "Data Warehouse Link", Description = "Automated data pulls", BaseCost = 2500, CostMultiplier = 1.22, Owned = 0, EffectPerUnit = 8, UnlockAtRoleOrder = 2 },
        new Upgrade { Id = "analytics-team", Name = "Analytics Team", Description = "24/7 reporting", BaseCost = 8000, CostMultiplier = 1.25, Owned = 0, EffectPerUnit = 30, UnlockAtRoleOrder = 2 },
    };

    public static readonly List<TaskTab> TaskTabs = new()
    {
        new TaskTab
        {
            Id = "emails",
            Name = "📧 Emails",
            ResourceKey = "emailsProcessed",
            PerSecondKey = "emailsPerSecond",
            UnlockAtRoleOrder = 0,
            Upgrades = EmailsUpgrades,
        },
        new TaskTab
        {
            Id = "reports",
            Name = "📊 Reports",
            ResourceKey = "reportsDone",
            PerSecondKey = "reportsPerSecond",
            UnlockAtRoleOrder = 1,
            Upgrades = ReportsUpgrades,
        },
    };
}
