// ============================================================
//  CC2 — Phaser 3 Game
//  A minimal prototype with Boot and Main scenes that
//  demonstrates the Steam bridge integration.
// ============================================================

// ---- Boot Scene ----
class BootScene extends Phaser.Scene {
    constructor() {
        super({ key: 'BootScene' });
    }

    preload() {
        // Show a simple loading bar
        const { width, height } = this.scale;
        const barW = width * 0.4;
        const barH = 24;
        const barX = (width - barW) / 2;
        const barY = height / 2;

        const bg = this.add.rectangle(barX + barW / 2, barY, barW, barH, 0x222244);
        const fill = this.add.rectangle(barX + 2, barY, 0, barH - 4, 0x5588ff);
        fill.setOrigin(0, 0.5);

        this.load.on('progress', (v) => {
            fill.width = (barW - 4) * v;
        });

        // Placeholder asset: generate a small colored square texture
        const gfx = this.make.graphics({ add: false });
        gfx.fillStyle(0x44aaff, 1);
        gfx.fillRoundedRect(0, 0, 64, 64, 10);
        gfx.generateTexture('player', 64, 64);
        gfx.destroy();
    }

    create() {
        this.scene.start('MainScene');
    }
}

// ---- Main Scene ----
class MainScene extends Phaser.Scene {
    constructor() {
        super({ key: 'MainScene' });
        this.playerName = 'Loading...';
        this.steamConnected = false;
    }

    async create() {
        const { width, height } = this.scale;

        // --- Background gradient (simulated with overlapping rectangles) ---
        this.add.rectangle(width / 2, height / 2, width, height, 0x1a1a2e);
        this.add.rectangle(width / 2, height * 0.7, width, height * 0.6, 0x16213e);

        // --- Fetch Steam user info ---
        try {
            const user = await SteamBridge.getUser();
            this.playerName = user.name;
            this.steamConnected = user.isOnline;
        } catch {
            this.playerName = 'Offline Player';
        }

        // --- Title ---
        this.add.text(width / 2, height * 0.15, 'CC2', {
            fontSize: '72px',
            fontFamily: 'Segoe UI, Arial, sans-serif',
            fontStyle: 'bold',
            color: '#e2e2e2',
            stroke: '#0f3460',
            strokeThickness: 6,
        }).setOrigin(0.5);

        // --- Player greeting ---
        const greeting = this.steamConnected
            ? `Welcome, ${this.playerName}!`
            : `Welcome, ${this.playerName}! (offline mode)`;

        this.add.text(width / 2, height * 0.28, greeting, {
            fontSize: '28px',
            fontFamily: 'Segoe UI, Arial, sans-serif',
            color: '#a8d8ea',
        }).setOrigin(0.5);

        // --- Movable player sprite ---
        this.player = this.add.sprite(width / 2, height / 2, 'player');
        this.player.setInteractive({ draggable: true });

        // Drag to move
        this.input.setDraggable(this.player);
        this.input.on('drag', (_pointer, obj, dragX, dragY) => {
            obj.x = dragX;
            obj.y = dragY;
        });

        // --- Instruction text ---
        this.add.text(width / 2, height * 0.88, 'Drag the square around  |  This is your Steam browser game prototype', {
            fontSize: '16px',
            fontFamily: 'Segoe UI, Arial, sans-serif',
            color: '#555577',
        }).setOrigin(0.5);

        // --- Connection status indicator ---
        const statusColor = this.steamConnected ? 0x44ff88 : 0xff8844;
        const statusLabel = this.steamConnected ? 'Steam: Connected' : 'Steam: Offline';
        this.add.circle(30, 30, 8, statusColor);
        this.add.text(46, 22, statusLabel, {
            fontSize: '14px',
            fontFamily: 'Segoe UI, Arial, sans-serif',
            color: '#888899',
        });

        // --- Start heartbeat ---
        SteamBridge.startHeartbeat(10000);

        // --- Handle window resize ---
        this.scale.on('resize', (gameSize) => {
            this.cameras.main.setSize(gameSize.width, gameSize.height);
        });
    }
}

// ---- Phaser Config ----
const config = {
    type: Phaser.AUTO,
    parent: 'game-container',
    backgroundColor: '#1a1a2e',
    scale: {
        mode: Phaser.Scale.RESIZE,
        autoCenter: Phaser.Scale.CENTER_BOTH,
        width: '100%',
        height: '100%',
    },
    scene: [BootScene, MainScene],
};

// Launch!
const game = new Phaser.Game(config);
