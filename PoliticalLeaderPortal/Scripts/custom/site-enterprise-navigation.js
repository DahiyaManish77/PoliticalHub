(function () {
    var transitionMs = 320;
    var menuCloseTimer = null;
    var languageCloseTimer = null;
    var searchCloseTimer = null;

    function setCollapsed(nav, button, shouldCollapse) {
        if (menuCloseTimer) {
            window.clearTimeout(menuCloseTimer);
            menuCloseTimer = null;
        }

        if (shouldCollapse) {
            nav.classList.add("is-closing");
            nav.classList.remove("is-open");

            menuCloseTimer = window.setTimeout(function () {
                nav.classList.add("is-collapsed");
                nav.classList.remove("is-closing");
            }, transitionMs);
        } else {
            nav.classList.remove("is-collapsed", "is-closing");
            window.requestAnimationFrame(function () {
                nav.classList.add("is-open");
            });
        }

        button.setAttribute("aria-expanded", shouldCollapse ? "false" : "true");
        button.setAttribute("aria-label", shouldCollapse ? "Open menu" : "Close menu");

        var icon = button.querySelector("i");

        if (icon) {
            icon.className = shouldCollapse ? "bi bi-list" : "bi bi-x-lg";
        }
    }

    function setLanguageOpen(wrapper, button, shouldOpen) {
        if (languageCloseTimer) {
            window.clearTimeout(languageCloseTimer);
            languageCloseTimer = null;
        }

        if (shouldOpen) {
            wrapper.classList.remove("closing");
            wrapper.classList.add("open");
        } else if (wrapper.classList.contains("open")) {
            wrapper.classList.add("closing");
            wrapper.classList.remove("open");

            languageCloseTimer = window.setTimeout(function () {
                wrapper.classList.remove("closing");
            }, transitionMs);
        }

        button.setAttribute("aria-expanded", shouldOpen ? "true" : "false");
    }

    function setSearchOpen(wrapper, button, shouldOpen) {
        if (searchCloseTimer) {
            window.clearTimeout(searchCloseTimer);
            searchCloseTimer = null;
        }

        if (shouldOpen) {
            wrapper.classList.remove("closing");
            wrapper.classList.add("open");
            window.setTimeout(function () {
                var input = wrapper.querySelector("input[name='keyword']");

                if (input) {
                    input.focus();
                }
            }, 80);
        } else if (wrapper.classList.contains("open")) {
            wrapper.classList.add("closing");
            wrapper.classList.remove("open");

            searchCloseTimer = window.setTimeout(function () {
                wrapper.classList.remove("closing");
            }, transitionMs);
        }

        button.setAttribute("aria-expanded", shouldOpen ? "true" : "false");
    }

    function isMobileMenu() {
        return window.matchMedia("(max-width: 1199px)").matches;
    }

    function closeOpenMegaItems(exceptItem) {
        document.querySelectorAll(".site-primary-item.open").forEach(function (item) {
            if (item !== exceptItem) {
                item.classList.remove("open");

                var link = item.querySelector(".site-primary-link");

                if (link) {
                    link.setAttribute("aria-expanded", "false");
                }
            }
        });
    }

    function changeGoogleLanguage(lang) {
        var tries = 0;
        var timer = window.setInterval(function () {
            var combo = document.querySelector(".goog-te-combo");

            tries += 1;

            if (!combo && tries < 20) {
                return;
            }

            window.clearInterval(timer);

            if (!combo) {
                return;
            }

            combo.value = lang;

            if (document.createEvent) {
                var event = document.createEvent("HTMLEvents");
                event.initEvent("change", true, true);
                combo.dispatchEvent(event);
            } else {
                combo.fireEvent("onchange");
            }
        }, 250);
    }

    function getLanguageName(lang) {
        var languages = {
            en: "English",
            hi: "Hindi",
            pa: "Punjabi",
            gu: "Gujarati",
            mr: "Marathi",
            bn: "Bengali",
            ta: "Tamil",
            te: "Telugu"
        };

        return languages[lang] || "English";
    }

    document.addEventListener("DOMContentLoaded", function () {
        var nav = document.querySelector(".site-mega-nav");
        var menuButton = document.querySelector(".site-menu-close");
        var languageWrapper = document.getElementById("siteLanguageWrapper");
        var languageButton = document.getElementById("siteLanguageButton");
        var selectedLanguage = document.getElementById("siteSelectedLanguage");
        var searchWrapper = document.getElementById("siteSearchWrapper");
        var searchButton = document.getElementById("siteSearchButton");

        if (nav && menuButton) {
            menuButton.addEventListener("click", function () {
                setCollapsed(nav, menuButton, !nav.classList.contains("is-collapsed"));
            });
        }

        document.querySelectorAll(".site-primary-item.has-mega > .site-primary-link").forEach(function (link) {
            link.addEventListener("click", function (event) {
                if (!isMobileMenu()) {
                    return;
                }

                var item = link.closest(".site-primary-item");

                if (!item) {
                    return;
                }

                event.preventDefault();
                closeOpenMegaItems(item);
                item.classList.toggle("open");
                link.setAttribute("aria-expanded", item.classList.contains("open") ? "true" : "false");
            });

            link.addEventListener("keydown", function (event) {
                if (event.key !== "Enter" && event.key !== " ") {
                    return;
                }

                if (!isMobileMenu()) {
                    return;
                }

                event.preventDefault();
                link.click();
            });
        });

        if (languageWrapper && languageButton) {
            languageButton.addEventListener("click", function (event) {
                event.stopPropagation();
                setLanguageOpen(
                    languageWrapper,
                    languageButton,
                    !languageWrapper.classList.contains("open")
                );
            });

            languageWrapper.querySelectorAll("[data-lang]").forEach(function (item) {
                item.addEventListener("click", function () {
                    if (selectedLanguage) {
                        selectedLanguage.textContent = item.textContent.trim();
                    }

                    try {
                        localStorage.setItem("SelectedLanguage", item.getAttribute("data-lang"));
                        localStorage.setItem("SelectedLanguageName", item.textContent.trim());
                    } catch (e) {
                    }

                    setLanguageOpen(languageWrapper, languageButton, false);
                    changeGoogleLanguage(item.getAttribute("data-lang"));
                });
            });

            try {
                var savedLanguage = localStorage.getItem("SelectedLanguage");
                var savedLanguageName = localStorage.getItem("SelectedLanguageName");

                if (savedLanguage && selectedLanguage) {
                    selectedLanguage.textContent = savedLanguageName || getLanguageName(savedLanguage);

                    window.setTimeout(function () {
                        changeGoogleLanguage(savedLanguage);
                    }, 900);
                }
            } catch (e) {
            }
        }

        if (searchWrapper && searchButton) {
            searchButton.addEventListener("click", function (event) {
                event.stopPropagation();

                if (languageWrapper && languageButton) {
                    setLanguageOpen(languageWrapper, languageButton, false);
                }

                setSearchOpen(
                    searchWrapper,
                    searchButton,
                    !searchWrapper.classList.contains("open")
                );
            });

            var searchForm = searchWrapper.querySelector(".site-search-panel");

            if (searchForm) {
                searchForm.addEventListener("click", function (event) {
                    event.stopPropagation();
                });
            }
        }

        document.addEventListener("click", function (event) {
            if (languageWrapper && !languageWrapper.contains(event.target)) {
                setLanguageOpen(languageWrapper, languageButton, false);
            }

            if (searchWrapper && !searchWrapper.contains(event.target)) {
                setSearchOpen(searchWrapper, searchButton, false);
            }
        });

        document.addEventListener("keydown", function (event) {
            if (event.key !== "Escape") {
                return;
            }

            if (nav && menuButton) {
                setCollapsed(nav, menuButton, true);
            }

            closeOpenMegaItems();

            if (languageWrapper && languageButton) {
                setLanguageOpen(languageWrapper, languageButton, false);
            }

            if (searchWrapper && searchButton) {
                setSearchOpen(searchWrapper, searchButton, false);
            }
        });
    });
})();

