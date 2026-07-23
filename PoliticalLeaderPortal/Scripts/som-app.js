(function () {
    var deferredPrompt = null;
    var buttons = [
        document.getElementById("somInstallButton"),
        document.getElementById("somInstallButtonBottom")
    ].filter(Boolean);
    var help = document.getElementById("somInstallHelp");

    if ("serviceWorker" in navigator) {
        navigator.serviceWorker.register("/som-app-sw.js").catch(function () { });
    }

    window.addEventListener("beforeinstallprompt", function (event) {
        event.preventDefault();
        deferredPrompt = event;
        buttons.forEach(function (button) {
            button.disabled = false;
        });
        if (help) {
            help.textContent = "Tap Install Som App to add it to your mobile home screen.";
        }
    });

    buttons.forEach(function (button) {
        button.addEventListener("click", function () {
            if (deferredPrompt) {
                deferredPrompt.prompt();
                deferredPrompt.userChoice.finally(function () {
                    deferredPrompt = null;
                });
                return;
            }

            if (help) {
                help.textContent = "Use your browser menu: Install app or Add to Home Screen.";
            }

            if (window.Swal) {
                Swal.fire({
                    title: "Install Som App",
                    text: "Open browser menu and choose Install app or Add to Home Screen.",
                    icon: "info",
                    confirmButtonText: "Got it"
                });
            }
        });
    });
})();
