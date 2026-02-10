// Closes the app: confirms with user, requests server shutdown, then closes the browser window.
window.closeApp = function () {
    if (!confirm('Are you sure you want to exit?')) return;
    fetch('/api/shutdown', { method: 'POST' }).finally(function () {
        window.close();
    });
};
