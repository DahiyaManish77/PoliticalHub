(function () {
    "use strict";

    function keepModalsInteractive() {
        ["#loginModal", "#registerModal"].forEach(function (selector) {
            var modal = document.querySelector(selector);
            if (modal && modal.parentNode !== document.body) document.body.appendChild(modal);
        });
    }

    function openModal(selector) {
        keepModalsInteractive();
        var element = document.querySelector(selector);
        if (!element || !window.bootstrap || !window.bootstrap.Modal) return false;
        window.bootstrap.Modal.getOrCreateInstance(element).show();
        return true;
    }

    document.addEventListener("click", function (event) {
        var loginTrigger = event.target.closest('[data-bs-target="#loginModal"], [href$="/Account/Login"], [href*="/Account/Login?"]');
        var registerTrigger = event.target.closest('[data-bs-target="#registerModal"], [href$="/Account/Register"], [href*="/Account/Register?"]');
        if (loginTrigger) {
            event.preventDefault();
            openModal("#loginModal");
            return;
        }
        if (registerTrigger) {
            event.preventDefault();
            openModal("#registerModal");
        }
    });

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", keepModalsInteractive);
    else keepModalsInteractive();
})();
