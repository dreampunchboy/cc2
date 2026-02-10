// Reports scroll position and viewport size to Blazor for 2D grid virtualization.
window.gridScrollInterop = {
  rafId: null,
  _handlers: {},

  init: function (elementId, dotNetRef) {
    const el = document.getElementById(elementId);
    if (!el) return;

    const self = window.gridScrollInterop;
    const report = function () {
      if (self.rafId) return;
      self.rafId = requestAnimationFrame(function () {
        self.rafId = null;
        dotNetRef.invokeMethodAsync('OnScrollOrResize', el.scrollLeft, el.scrollTop, el.clientWidth, el.clientHeight);
      });
    };

    el.addEventListener('scroll', report, { passive: true });
    window.addEventListener('resize', report);
    self._handlers[elementId] = { el: el, report: report };
    report();
  },

  dispose: function (elementId) {
    const self = window.gridScrollInterop;
    const h = self._handlers[elementId];
    if (h) {
      h.el.removeEventListener('scroll', h.report);
      window.removeEventListener('resize', h.report);
      delete self._handlers[elementId];
    }
    if (self.rafId) cancelAnimationFrame(self.rafId);
  }
};
