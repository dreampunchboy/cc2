// ============================================================
//  GameAPI — Office Clicker (server-authoritative)
// ============================================================

const GameAPI = (() => {
    const BASE = window.location.origin;

    async function getState() {
        const res = await fetch(`${BASE}/api/game/state`);
        if (!res.ok) throw new Error('Failed to fetch state');
        return res.json();
    }

    async function click() {
        const res = await fetch(`${BASE}/api/game/click`, { method: 'POST' });
        return res.ok;
    }

    async function buySheet(sheetId, amount = 1) {
        const res = await fetch(`${BASE}/api/game/buy-sheet`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ sheetId, amount }),
        });
        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.error || 'Buy failed');
        }
        return true;
    }

    async function buyUpgrade(upgradeId) {
        const res = await fetch(`${BASE}/api/game/buy-upgrade`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ upgradeId }),
        });
        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.error || 'Buy failed');
        }
        return true;
    }

    function formatNumber(n) {
        if (typeof n !== 'number' || !isFinite(n) || n < 0) return '0';
        if (n < 1000) return Math.floor(n).toLocaleString();
        if (n < 1e6) return (n / 1e3).toFixed(1).replace(/\.0$/, '') + 'K';
        if (n < 1e9) return (n / 1e6).toFixed(1).replace(/\.0$/, '') + 'M';
        if (n < 1e12) return (n / 1e9).toFixed(1).replace(/\.0$/, '') + 'B';
        return n.toExponential(2);
    }

    return {
        getState,
        click,
        buySheet,
        buyUpgrade,
        formatNumber,
    };
})();
