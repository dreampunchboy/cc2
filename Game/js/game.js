// ============================================================
//  Office Clicker — DOM render, poll state, wire actions
// ============================================================

(function () {
    const formulaInput = document.getElementById('formula-input');
    const cellRef = document.querySelector('.cell-ref');
    const gridBody = document.getElementById('grid-body');
    const sheetTabsEl = document.getElementById('sheet-tabs');
    const btnWork = document.getElementById('btn-work');

    let state = null;
    let currentSheetId = 1;
    let pollInterval = null;

    function poll() {
        GameAPI.getState()
            .then((s) => {
                state = s;
                render();
            })
            .catch(() => {
                if (!state) {
                    if (formulaInput) formulaInput.value = 'Connecting...';
                }
            });
    }

    function render() {
        if (!state) return;

        const currency = state.currency ?? 0;
        const perSecond = state.perSecond ?? 0;
        const clickPower = state.clickPower ?? 1;

        if (formulaInput) {
            formulaInput.value = `= ${GameAPI.formatNumber(currency)} Productivity | ${GameAPI.formatNumber(perSecond)}/s`;
        }
        if (cellRef) cellRef.textContent = 'Total';

        const sheets = state.sheets ?? [];
        const upgrades = state.upgrades ?? [];
        const sheet = sheets.find((s) => s.id === currentSheetId);
        const sheetUpgrades = upgrades.filter((u) => u.sheetId === currentSheetId);

        if (gridBody) {
            gridBody.innerHTML = '';

            if (sheet) {
                const infoRow = document.createElement('tr');
                infoRow.className = 'sheet-info-row';
                const canBuySheet = sheet.nextCost > 0 && currency >= sheet.nextCost;
                infoRow.innerHTML = `
          <td class="row-num">—</td>
          <td colspan="2">Sheet: ${sheet.name} — Owned: ${sheet.owned} | Rate: ${GameAPI.formatNumber((sheet.baseRate || 0) * sheet.owned)}/s</td>
          <td class="cell-buy">${sheet.nextCost > 0 ? `<button type="button" class="btn-buy btn-buy-sheet" data-sheet-id="${sheet.id}" ${canBuySheet ? '' : 'disabled'}>Buy (${GameAPI.formatNumber(sheet.nextCost)})</button>` : '—'}</td>
        `;
                gridBody.appendChild(infoRow);
            }

            sheetUpgrades.forEach((u, idx) => {
                const tr = document.createElement('tr');
                const canBuy = !u.purchased && currency >= u.cost;
                tr.innerHTML = `
          <td class="row-num">${idx + 1}</td>
          <td>${u.name}</td>
          <td>${GameAPI.formatNumber(u.cost)}</td>
          <td class="cell-buy ${u.purchased ? 'cell-purchased' : ''}">
            ${u.purchased ? 'Purchased' : `<button type="button" class="btn-buy" data-upgrade-id="${u.id}" ${canBuy ? '' : 'disabled'}>Buy</button>`}
          </td>
        `;
                gridBody.appendChild(tr);
            });
        }

        if (sheetTabsEl) {
            sheetTabsEl.innerHTML = '';
            (state.sheets ?? []).forEach((s) => {
                const tab = document.createElement('button');
                tab.type = 'button';
                tab.className = 'sheet-tab' + (s.id === currentSheetId ? ' active' : '');
                tab.textContent = s.name;
                tab.dataset.sheetId = s.id;
                tab.addEventListener('click', () => {
                    currentSheetId = s.id;
                    render();
                    document.querySelectorAll('.sheet-tab').forEach((t) => t.classList.remove('active'));
                    tab.classList.add('active');
                });
                sheetTabsEl.appendChild(tab);
            });
        }
    }

    function init() {
        btnWork?.addEventListener('click', async () => {
            try {
                await GameAPI.click();
            } catch (e) {
                console.warn(e);
            }
        });

        gridBody?.addEventListener('click', async (e) => {
            const btn = e.target.closest('.btn-buy');
            if (!btn || btn.disabled) return;

            const upgradeId = btn.dataset.upgradeId;
            const sheetId = btn.dataset.sheetId;

            try {
                if (upgradeId) {
                    await GameAPI.buyUpgrade(parseInt(upgradeId, 10));
                } else if (sheetId) {
                    await GameAPI.buySheet(parseInt(sheetId, 10), 1);
                }
                poll();
            } catch (err) {
                console.warn(err);
            }
        });

        poll();
        pollInterval = setInterval(poll, 150);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
