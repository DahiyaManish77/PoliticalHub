(function () {
    function setCollapsed(nav, button, shouldCollapse) {
        nav.classList.toggle("is-collapsed", shouldCollapse);
        button.setAttribute(
            "aria-label",
            shouldCollapse ? "Open menu" : "Close menu"
        );

        var icon = button.querySelector("i");

        if (icon) {
            icon.className = shouldCollapse ? "bi bi-list" : "bi bi-x-lg";
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        var nav = document.querySelector(".site-mega-nav");
        var closeButton = document.querySelector(".site-menu-close");

        if (!nav || !closeButton) {
            return;
        }

        closeButton.addEventListener("click", function () {
            setCollapsed(nav, closeButton, !nav.classList.contains("is-collapsed"));
        });

        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape") {
                setCollapsed(nav, closeButton, true);
            }
        });
    });
})();
