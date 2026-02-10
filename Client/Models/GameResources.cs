namespace BG.Client.Models;

/// <summary>Dynamic resources keyed by resourceKey (e.g. emailsProcessed, reportsDone).</summary>
public class GameResources
{
    public Dictionary<string, double> ByKey { get; } = new();

    public double Get(string key) => ByKey.TryGetValue(key, out var v) ? v : 0;
    public void Set(string key, double value) => ByKey[key] = value;
    public void Add(string key, double delta) => ByKey[key] = Get(key) + delta;
    public void Subtract(string key, double amount) => ByKey[key] = Math.Max(0, Get(key) - amount);
}

/// <summary>Per-second rates keyed by perSecondKey (e.g. emailsPerSecond, reportsPerSecond).</summary>
public class PerSecond
{
    public Dictionary<string, double> ByKey { get; } = new();

    public double Get(string key) => ByKey.TryGetValue(key, out var v) ? v : 0;
    public void Set(string key, double value) => ByKey[key] = value;
}
