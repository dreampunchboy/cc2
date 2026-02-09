using Steamworks;

namespace CC2.Launcher;

/// <summary>
/// Wraps Steamworks.NET calls with graceful fallback when Steam is not running.
/// When Steam is unavailable, all methods return safe mock data so the game
/// can still be developed and tested without the Steam client.
/// </summary>
public sealed class SteamBridge : IDisposable
{
    private bool _disposed;

    /// <summary>True if SteamAPI.Init() succeeded and Steam is available.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Attempts to initialize the Steamworks API.
    /// Returns true if Steam is available, false otherwise (game continues with mock data).
    /// </summary>
    public bool Initialize()
    {
        try
        {
            if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                // Steam wants to restart through the client — in production this
                // would exit, but for dev we just note it and continue.
                Console.WriteLine("[Steam] RestartAppIfNecessary returned true — continuing in dev mode.");
            }

            IsInitialized = SteamAPI.Init();

            if (IsInitialized)
            {
                Console.WriteLine($"[Steam] Initialized successfully. User: {GetPlayerName()} ({GetSteamId()})");
            }
            else
            {
                Console.WriteLine("[Steam] SteamAPI.Init() failed — running in offline/mock mode.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Steam] Exception during init: {ex.Message} — running in offline/mock mode.");
            IsInitialized = false;
        }

        return IsInitialized;
    }

    /// <summary>Returns the current player's Steam display name, or a mock name.</summary>
    public string GetPlayerName()
    {
        if (!IsInitialized) return "Dev Player";

        try
        {
            return SteamFriends.GetPersonaName();
        }
        catch
        {
            return "Dev Player";
        }
    }

    /// <summary>Returns the current player's Steam ID as a string, or a mock ID.</summary>
    public string GetSteamId()
    {
        if (!IsInitialized) return "76561190000000000";

        try
        {
            return SteamUser.GetSteamID().m_SteamID.ToString();
        }
        catch
        {
            return "76561190000000000";
        }
    }

    /// <summary>
    /// Attempts to unlock a Steam achievement by its API name.
    /// Returns true if the achievement was set (or already unlocked), false on failure.
    /// </summary>
    public bool UnlockAchievement(string achievementId)
    {
        if (!IsInitialized)
        {
            Console.WriteLine($"[Steam] Mock: Achievement '{achievementId}' unlocked (offline mode).");
            return true;
        }

        try
        {
            SteamUserStats.SetAchievement(achievementId);
            bool stored = SteamUserStats.StoreStats();
            Console.WriteLine($"[Steam] Achievement '{achievementId}' set. StoreStats={stored}");
            return stored;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Steam] Failed to unlock achievement '{achievementId}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Pumps Steam callbacks. Must be called regularly (~15 Hz) to keep
    /// Steamworks responsive (achievements, overlay, etc.).
    /// </summary>
    public void RunCallbacks()
    {
        if (!IsInitialized) return;

        try
        {
            SteamAPI.RunCallbacks();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Steam] RunCallbacks error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (IsInitialized)
        {
            Console.WriteLine("[Steam] Shutting down SteamAPI.");
            SteamAPI.Shutdown();
            IsInitialized = false;
        }
    }
}
