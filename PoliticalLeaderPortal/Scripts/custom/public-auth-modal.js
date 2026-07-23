(function () {
    function openModal(selector) {
        var element = document.querySelector(selector);

        if (!element || !window.bootstrap || !window.bootstrap.Modal) {
            return false;
        }

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
})();
