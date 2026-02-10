// ============================================================
//  Office Clicker — DOM render, poll state, wire actions (jQuery)
//  Grid: hundreds of rows/columns; only first rows/cols hold game data.
// ============================================================

(function ($) {
    var GRID_ROWS = 200;
    var GRID_COLS = 100;
    var DATA_COLS = 4;

    var $formulaInput = $('#formula-input');
    var $cellRef = $('.cell-ref');
    var $sheetGrid = $('#sheet-grid');
    var $gridHead = $('#grid-head tr');
    var $gridBody = $('#grid-body');
    var $sheetTabs = $('#sheet-tabs');
    var $btnWork = $('#btn-work');

    var state = null;
    var currentSheetId = 1;
    var pollInterval = null;
    var gridBuilt = false;

    function columnLetter(n) {
        var s = '';
        n++;
        while (n > 0) {
            var r = (n - 1) % 26;
            s = String.fromCharCode(65 + r) + s;
            n = Math.floor((n - 1) / 26);
        }
        return s;
    }

    function buildGrid() {
        if (gridBuilt) return;
        var $headRow = $gridHead;
        for (var c = 0; c < GRID_COLS; c++) {
            $headRow.append($('<th scope="col">').text(columnLetter(c)));
        }
        var fragment = document.createDocumentFragment();
        for (var r = 0; r < GRID_ROWS; r++) {
            var $tr = $('<tr>');
            if (r === 0) $tr.addClass('sheet-info-row');
            $tr.append($('<td class="row-num">').text(r === 0 ? '—' : String(r)));
            for (var c = 0; c < GRID_COLS; c++) {
                $tr.append($('<td>'));
            }
            fragment.appendChild($tr[0]);
        }
        $gridBody.append(fragment);
        gridBuilt = true;
    }

    function setCell(rowIndex, colIndex, content) {
        var $row = $gridBody.find('tr').eq(rowIndex);
        if (!$row.length) return;
        var $cell = $row.find('td').eq(colIndex);
        if (typeof content === 'string' && content.indexOf('<') !== -1) {
            $cell.html(content);
        } else {
            $cell.text(content);
        }
    }

    function clearDataRows(maxRow) {
        for (var r = 0; r <= maxRow; r++) {
            for (var c = 1; c < DATA_COLS; c++) {
                setCell(r, c, '');
            }
        }
    }

    function render() {
        if (!state) return;

        var currency = state.currency ?? 0;
        var perSecond = state.perSecond ?? 0;
        var sheets = state.sheets ?? [];
        var upgrades = state.upgrades ?? [];
        var sheet = sheets.find(function (s) { return s.id === currentSheetId; });
        var sheetUpgrades = upgrades.filter(function (u) { return u.sheetId === currentSheetId; });

        $formulaInput.val('= ' + GameAPI.formatNumber(currency) + ' Productivity | ' + GameAPI.formatNumber(perSecond) + '/s');
        $cellRef.text('Total');

        var dataRows = 1 + sheetUpgrades.length;
        clearDataRows(dataRows);

        if (sheet) {
            var canBuySheet = sheet.nextCost > 0 && currency >= sheet.nextCost;
            var buyHtml = sheet.nextCost > 0
                ? '<button type="button" class="btn-buy btn-buy-sheet" data-sheet-id="' + sheet.id + '"' + (canBuySheet ? '' : ' disabled') + '>Buy (' + GameAPI.formatNumber(sheet.nextCost) + ')</button>'
                : '—';
            setCell(0, 1, 'Sheet: ' + sheet.name + ' — Owned: ' + sheet.owned + ' | Rate: ' + GameAPI.formatNumber((sheet.baseRate || 0) * sheet.owned) + '/s');
            setCell(0, 2, '');
            setCell(0, 3, buyHtml);
        }

        sheetUpgrades.forEach(function (u, idx) {
            var r = idx + 1;
            var canBuy = !u.purchased && currency >= u.cost;
            var cellClass = 'cell-buy' + (u.purchased ? ' cell-purchased' : '');
            var cellContent = u.purchased
                ? 'Purchased'
                : '<button type="button" class="btn-buy" data-upgrade-id="' + u.id + '"' + (canBuy ? '' : ' disabled') + '>Buy</button>';
            setCell(r, 1, u.name);
            setCell(r, 2, GameAPI.formatNumber(u.cost));
            setCell(r, 3, cellContent);
        });

        $sheetTabs.empty();
        sheets.forEach(function (s) {
            var $tab = $('<button type="button" class="sheet-tab">' + s.name + '</button>');
            $tab.data('sheet-id', s.id);
            if (s.id === currentSheetId) {
                $tab.addClass('active');
            }
            $tab.on('click', function () {
                currentSheetId = $(this).data('sheet-id');
                $sheetTabs.find('.sheet-tab').removeClass('active');
                $(this).addClass('active');
                render();
            });
            $sheetTabs.append($tab);
        });
    }

    function poll() {
        GameAPI.getState()
            .then(function (s) {
                state = s;
                render();
            })
            .catch(function () {
                if (!state) {
                    $formulaInput.val('Connecting...');
                }
            });
    }

    function init() {
        buildGrid();

        $btnWork.on('click', function () {
            GameAPI.click().catch(function (e) { console.warn(e); });
        });

        $gridBody.on('click', '.btn-buy', function () {
            var $btn = $(this);
            if ($btn.prop('disabled')) return;

            var upgradeId = $btn.data('upgrade-id');
            var sheetId = $btn.data('sheet-id');

            var promise = upgradeId
                ? GameAPI.buyUpgrade(parseInt(upgradeId, 10))
                : GameAPI.buySheet(parseInt(sheetId, 10), 1);
            promise.then(poll).catch(function (err) { console.warn(err); });
        });

        poll();
        pollInterval = setInterval(poll, 150);
    }

    $(function () {
        init();
    });
})(jQuery);
