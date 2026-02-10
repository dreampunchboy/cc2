using BG.Client.Models;

namespace BG.Client.Data;

public static class TabsData
{
    private static readonly List<Upgrade> EmailsUpgrades = new()
    {
        new Upgrade { Id = "junior-assistant", Name = "Junior Assistant", Description = "Processes emails faster", BaseCost = 15, CostMultiplier = 1.15, Owned = 0, EffectPerUnit = 0.1 },
        new Upgrade { Id = "inbox-rules", Name = "Inbox Rules", Description = "Auto-sort and flag", BaseCost = 50, CostMultiplier = 1.2, Owned = 0, EffectPerUnit = 0.3 },
        new Upgrade { Id = "outlook-macros", Name = "Outlook Macros", Description = "Batch actions", BaseCost = 200, CostMultiplier = 1.2, Owned = 0, EffectPerUnit = 1.0 },
        new Upgrade { Id = "ai-auto-reply", Name = "AI Auto-Reply", Description = "AI drafts replies", BaseCost = 800, CostMultiplier = 1.25, Owned = 0, EffectPerUnit = 5.0 },
        new Upgrade { Id = "offshore-team", Name = "Offshore Team", Description = "24/7 email handling", BaseCost = 4000, CostMultiplier = 1.3, Owned = 0, EffectPerUnit = 25 },
    };

    private static readonly List<Upgrade> ReportsUpgrades = new()
    {
        new Upgrade { Id = "report-stub", Name = "Report Stub", Description = "Placeholder for Reports tab", BaseCost = 100, CostMultiplier = 1.1, Owned = 0, EffectPerUnit = 0.5 },
    };

    public static readonly List<TaskTab> TaskTabs = new()
    {
        new TaskTab
        {
            Id = "emails",
            Name = "📧 Emails",
            ResourceKey = "emailsProcessed",
            PerSecondKey = "emailsPerSecond",
            Upgrades = EmailsUpgrades,
        },
        new TaskTab
        {
            Id = "reports",
            Name = "📊 Reports",
            ResourceKey = "reportsDone",
            PerSecondKey = "reportsPerSecond",
            Upgrades = ReportsUpgrades,
        },
    };
}
