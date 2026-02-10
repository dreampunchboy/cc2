// Particle background and confetti for Blazor client

(function () {
    'use strict';

    var particlesConfig = {
        particles: {
            number: { value: 60 },
            color: { value: '#888888' },
            shape: { type: 'circle' },
            opacity: { value: 0.35 },
            size: { value: 2.5 },
            move: { enable: true, speed: 1, random: true, straight: false }
        },
        interactivity: {
            detect_on: 'canvas',
            events: { onhover: { enable: true, mode: 'grab' }, onclick: { enable: false } },
            modes: { grab: { distance: 120, line_linked: { opacity: 0.3 } } }
        },
        retina_detect: true
    };

    window.particlesInit = function () {
        var el = document.getElementById('particles-js');
        if (el && typeof particlesJS === 'function') {
            particlesJS('particles-js', particlesConfig);
        }
    };

    window.triggerConfetti = function (options) {
        if (typeof confetti !== 'function') return;
        var opts = options || {};
        confetti({
            origin: opts.origin || { x: 0.5, y: 0.5 },
            particleCount: opts.particleCount || 80,
            spread: opts.spread || 70
        });
    };
})();
