// ============================================================
//  SteamBridge — Client-side REST API wrapper
//  Communicates with the C# launcher's local HTTP server.
//  Base URL is auto-detected from the current page origin.
// ============================================================

const SteamBridge = (() => {
    const BASE = window.location.origin;

    /**
     * GET /api/steam/user
     * Returns: { name: string, steamId: string, isOnline: boolean }
     */
    async function getUser() {
        try {
            const res = await fetch(`${BASE}/api/steam/user`);
            return await res.json();
        } catch (err) {
            console.warn('[SteamBridge] getUser failed:', err);
            return { name: 'Offline Player', steamId: '0', isOnline: false };
        }
    }

    /**
     * POST /api/steam/achievement
     * @param {string} id — The achievement API name (e.g. "ACH_FIRST_WIN")
     * Returns: { success: boolean, achievementId: string }
     */
    async function unlockAchievement(id) {
        try {
            const res = await fetch(`${BASE}/api/steam/achievement`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id }),
            });
            return await res.json();
        } catch (err) {
            console.warn('[SteamBridge] unlockAchievement failed:', err);
            return { success: false, achievementId: id };
        }
    }

    /**
     * GET /api/steam/status
     * Also acts as a heartbeat so the launcher knows the browser is alive.
     * Returns: { ok: boolean, steamConnected: boolean, timestamp: string }
     */
    async function getStatus() {
        try {
            const res = await fetch(`${BASE}/api/steam/status`);
            return await res.json();
        } catch (err) {
            console.warn('[SteamBridge] getStatus failed:', err);
            return { ok: false, steamConnected: false, timestamp: null };
        }
    }

    /**
     * Starts a periodic heartbeat that calls /api/steam/status every N ms.
     * Keeps the launcher aware that the browser tab is still open.
     * @param {number} intervalMs — interval between heartbeats (default 10 000)
     * @returns {number} — the interval ID (can be cleared with clearInterval)
     */
    function startHeartbeat(intervalMs = 10000) {
        // Immediate first beat
        getStatus();
        return setInterval(() => getStatus(), intervalMs);
    }

    return {
        getUser,
        unlockAchievement,
        getStatus,
        startHeartbeat,
    };
})();
