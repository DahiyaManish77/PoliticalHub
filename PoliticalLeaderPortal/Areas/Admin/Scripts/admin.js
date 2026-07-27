(function ($) {
    "use strict";

    var AdminUi = {
        init: function () {
            this.installRequestProtection();
            this.bindShell();
            this.bindSidebarMenus();
            this.bindLanguageSelector();
            this.bindSidebarSearch();
            this.bindGlobalMenuSearch();
            this.showFlashMessages();
            this.initTables();
            this.bindAjaxDeleteButtons();
            this.bindFormFeedback();
            this.bindDeleteConfirmations();
            this.dismissLegacyAlerts();
        },

        installRequestProtection: function () {
            var tokenInput = document.querySelector("#adminRequestVerificationToken input[name='__RequestVerificationToken']");

            if (!tokenInput) {
                return;
            }

            var token = tokenInput.value;

            $.ajaxPrefilter(function (options, originalOptions) {
                var method = (options.type || options.method || "GET").toUpperCase();

                if (method === "GET" || method === "HEAD" || method === "OPTIONS") {
                    return;
                }

                if (options.data instanceof FormData) {
                    if (!options.data.has("__RequestVerificationToken")) {
                        options.data.append("__RequestVerificationToken", token);
                    }
                    return;
                }

                if (typeof options.data === "string") {
                    if (options.data.indexOf("__RequestVerificationToken=") < 0) {
                        options.data += (options.data ? "&" : "") +
                            "__RequestVerificationToken=" + encodeURIComponent(token);
                    }
                    return;
                }

                options.data = options.data || {};
                if (!options.data.__RequestVerificationToken) {
                    options.data.__RequestVerificationToken = token;
                }
            });

            if (!window.fetch || window.fetch.__adminRequestProtected) {
                return;
            }

            var nativeFetch = window.fetch;
            var protectedFetch = function (resource, init) {
                init = init || {};
                var method = (init.method || "GET").toUpperCase();

                if (method !== "GET" && method !== "HEAD" && method !== "OPTIONS") {
                    if (init.body instanceof FormData) {
                        if (!init.body.has("__RequestVerificationToken")) {
                            init.body.append("__RequestVerificationToken", token);
                        }
                    } else if (init.body instanceof URLSearchParams) {
                        if (!init.body.has("__RequestVerificationToken")) {
                            init.body.append("__RequestVerificationToken", token);
                        }
                    } else if (typeof init.body === "string") {
                        if (init.body.indexOf("__RequestVerificationToken=") < 0) {
                            init.body += (init.body ? "&" : "") +
                                "__RequestVerificationToken=" + encodeURIComponent(token);
                        }
                    } else if (!init.body) {
                        var requestBody = new URLSearchParams();
                        requestBody.append("__RequestVerificationToken", token);
                        init.body = requestBody;
                    }
                }

                return nativeFetch.call(window, resource, init);
            };

            protectedFetch.__adminRequestProtected = true;
            window.fetch = protectedFetch;
        },

        bindShell: function () {
            var toggle = document.getElementById("adminSidebarToggle");
            var collapse = document.getElementById("adminSidebarCollapse");
            var backdrop = document.getElementById("adminSidebarBackdrop");

            if (toggle) {
                toggle.addEventListener("click", function () {
                    if (window.matchMedia("(max-width: 991.98px)").matches) {
                        document.body.classList.toggle("admin-sidebar-open");
                    } else {
                        document.body.classList.toggle("admin-sidebar-collapsed");
                    }
                });
            }

            if (collapse) {
                collapse.addEventListener("click", function () {
                    document.body.classList.toggle("admin-sidebar-collapsed");
                });
            }

            if (backdrop) {
                backdrop.addEventListener("click", function () {
                    document.body.classList.remove("admin-sidebar-open");
                });
            }

            document.addEventListener("keydown", function (event) {
                if (event.key === "Escape") {
                    document.body.classList.remove("admin-sidebar-open");
                }
            });
        },

        bindSidebarMenus: function () {
            document.addEventListener("click", function (event) {
                var trigger = event.target.closest(".admin-menu-item.has-children > .admin-menu-link, .admin-menu-item.has-children > .admin-menu-heading");

                if (!trigger) {
                    return;
                }

                var menuItem = trigger.parentElement;

                if (!menuItem || !menuItem.classList.contains("has-children")) {
                    return;
                }

                var href = trigger.getAttribute("href");
                var isToggleOnly =
                    trigger.classList.contains("admin-menu-heading") ||
                    !href ||
                    href === "#";

                if (isToggleOnly) {
                    event.preventDefault();
                    menuItem.classList.toggle("open");
                    return;
                }

                if (event.target.closest(".menu-arrow")) {
                    event.preventDefault();
                    menuItem.classList.toggle("open");
                }
            });
        },

        bindLanguageSelector: function () {
            var wrapper = document.getElementById("siteLanguageWrapper");
            var button = document.getElementById("siteLanguageButton");
            var selectedLanguage = document.getElementById("siteSelectedLanguage");
            var closeTimer = null;

            if (!wrapper || !button) {
                return;
            }

            function setOpen(shouldOpen) {
                if (closeTimer) {
                    window.clearTimeout(closeTimer);
                    closeTimer = null;
                }

                if (shouldOpen) {
                    wrapper.classList.remove("closing");
                    wrapper.classList.add("open");
                } else if (wrapper.classList.contains("open")) {
                    wrapper.classList.add("closing");
                    wrapper.classList.remove("open");

                    closeTimer = window.setTimeout(function () {
                        wrapper.classList.remove("closing");
                    }, 220);
                }

                button.setAttribute("aria-expanded", shouldOpen ? "true" : "false");
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

                    var event = document.createEvent("HTMLEvents");
                    event.initEvent("change", true, true);
                    combo.dispatchEvent(event);
                }, 250);
            }

            function languageName(lang) {
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

            button.addEventListener("click", function (event) {
                event.stopPropagation();
                setOpen(!wrapper.classList.contains("open"));
            });

            wrapper.querySelectorAll("[data-lang]").forEach(function (item) {
                item.addEventListener("click", function () {
                    var lang = item.getAttribute("data-lang");
                    var text = item.textContent.trim();

                    if (selectedLanguage) {
                        selectedLanguage.textContent = text;
                    }

                    try {
                        localStorage.setItem("SelectedLanguage", lang);
                        localStorage.setItem("SelectedLanguageName", text);
                    } catch (e) {
                    }

                    setOpen(false);
                    changeGoogleLanguage(lang);
                });
            });

            document.addEventListener("click", function (event) {
                if (!wrapper.contains(event.target)) {
                    setOpen(false);
                }
            });

            document.addEventListener("keydown", function (event) {
                if (event.key === "Escape") {
                    setOpen(false);
                }
            });

            try {
                var savedLanguage = localStorage.getItem("SelectedLanguage");
                var savedLanguageName = localStorage.getItem("SelectedLanguageName");

                if (savedLanguage && selectedLanguage) {
                    selectedLanguage.textContent = savedLanguageName || languageName(savedLanguage);

                    window.setTimeout(function () {
                        changeGoogleLanguage(savedLanguage);
                    }, 900);
                }
            } catch (e) {
            }
        },

        bindSidebarSearch: function () {
            var input = document.getElementById("adminSidebarSearch");

            if (!input) {
                return;
            }

            input.addEventListener("input", function () {
                var keyword = input.value.trim().toLowerCase();

                document.querySelectorAll(".admin-menu-item").forEach(function (item) {
                    var text = item.textContent.toLowerCase();
                    var matches = !keyword || text.indexOf(keyword) >= 0;

                    item.style.display = matches ? "" : "none";

                    if (matches && keyword) {
                        item.classList.add("open");
                    }
                });
            });
        },

        bindGlobalMenuSearch: function () {
            var globalInput = document.getElementById("adminGlobalMenuSearch");
            var sidebarInput = document.getElementById("adminSidebarSearch");

            if (!globalInput || !sidebarInput) {
                return;
            }

            globalInput.addEventListener("input", function () {
                sidebarInput.value = globalInput.value;
                sidebarInput.dispatchEvent(new Event("input", { bubbles: true }));
            });

            globalInput.addEventListener("focus", function () {
                if (window.innerWidth < 992) {
                    document.body.classList.add("admin-sidebar-open");
                }
            });
        },

        showFlashMessages: function () {
            if (!window.Swal || !window.adminFlash) {
                return;
            }

            var flashTypes = [
                { key: "success", title: "Success", icon: "success" },
                { key: "error", title: "Action failed", icon: "error" },
                { key: "warning", title: "Please check", icon: "warning" }
            ];

            flashTypes.some(function (flash) {
                var message = window.adminFlash[flash.key];

                if (!message) {
                    return false;
                }

                Swal.fire({
                    title: flash.title,
                    text: message,
                    icon: flash.icon,
                    confirmButtonColor: "#2457d6"
                });

                return true;
            });
        },

        initTables: function () {
            if (!$.fn.DataTable) {
                return;
            }

            $("main.admin-content table.table").each(function () {
                var $table = $(this);
                var hasHeader = $table.find("thead th").length > 0;
                var hasRows = $table.find("tbody tr").length > 0;
                var isDetailsTable = !$table.closest(".table-responsive").length;
                var isAlreadyInitialized = $.fn.DataTable.isDataTable(this);
                var hasOptOut = $table.is("[data-admin-table='false']");
                var columnCount = $table.find("thead th").length;
                var hasColspanRows = $table.find("tbody td[colspan]").length > 0;
                var hasMismatchedRows = false;

                $table.find("tbody tr").each(function () {
                    var cellCount = $(this).children("td, th").length;

                    if (cellCount > 0 && cellCount !== columnCount) {
                        hasMismatchedRows = true;
                        return false;
                    }

                    return true;
                });

                if (!hasHeader ||
                    !hasRows ||
                    isDetailsTable ||
                    isAlreadyInitialized ||
                    hasOptOut ||
                    hasColspanRows ||
                    hasMismatchedRows) {
                    return;
                }

                $table.addClass("admin-data-table nowrap");

                $table.DataTable({
                    responsive: true,
                    autoWidth: false,
                    pageLength: 10,
                    lengthMenu: [5, 10, 25, 50],
                    order: [],
                    language: {
                        search: "",
                        searchPlaceholder: "Search records",
                        lengthMenu: "_MENU_ rows",
                        info: "Showing _START_ to _END_ of _TOTAL_",
                        emptyTable: "No records found"
                    },
                    columnDefs: [
                        {
                            targets: columnCount - 1,
                            orderable: false,
                            searchable: false
                        }
                    ]
                });
            });
        },

        bindAjaxDeleteButtons: function () {
            if (!window.Swal || !window.jQuery) {
                return;
            }

            document.addEventListener("click", function (event) {
                var button = event.target.closest(".btnDelete, .delete-btn");

                if (!button || button.dataset.skipAdminDeleteAlert === "true") {
                    return;
                }

                var id = button.getAttribute("data-id");

                if (!id) {
                    return;
                }

                event.preventDefault();
                event.stopPropagation();
                event.stopImmediatePropagation();

                Swal.fire({
                    title: "Delete this record?",
                    text: "This action cannot be undone.",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#dc3545",
                    cancelButtonColor: "#64748b",
                    confirmButtonText: "Delete"
                }).then(function (result) {
                    if (!result.isConfirmed) {
                        return;
                    }

                    var deleteUrl = button.getAttribute("data-delete-url") || AdminUi.resolveDeleteUrl();

                    $.post(deleteUrl, { id: id })
                        .done(function (response) {
                            if (response && response.success === false) {
                                Swal.fire({
                                    title: "Delete failed",
                                    text: response.message || "The record could not be deleted.",
                                    icon: "error",
                                    confirmButtonColor: "#2457d6"
                                });
                                return;
                            }

                            Swal.fire({
                                title: "Deleted",
                                text: "Record deleted successfully.",
                                icon: "success",
                                confirmButtonColor: "#2457d6",
                                timer: 1100,
                                showConfirmButton: false
                            }).then(function () {
                                window.location.reload();
                            });
                        })
                        .fail(function () {
                            Swal.fire({
                                title: "Delete failed",
                                text: "Please try again.",
                                icon: "error",
                                confirmButtonColor: "#2457d6"
                            });
                        });
                });
            }, true);
        },

        resolveDeleteUrl: function () {
            var path = window.location.pathname.replace(/\/$/, "");

            if (/\/Index$/i.test(path)) {
                path = path.replace(/\/Index$/i, "");
            }

            return path + "/Delete";
        },

        bindFormFeedback: function () {
            if (!window.Swal) {
                return;
            }

            $("form").on("submit", function () {
                var $form = $(this);

                if ($form.data("skip-admin-submit-alert") || $form.data("adminSubmitting")) {
                    return true;
                }

                var $submitter = $form.find("button[type='submit'], input[type='submit']").filter(":focus").first();
                var buttonText = ($submitter.text() || $submitter.val() || "").toLowerCase();
                var isDelete = buttonText.indexOf("delete") >= 0 || $form.attr("action") && $form.attr("action").toLowerCase().indexOf("delete") >= 0;

                if (isDelete) {
                    return true;
                }

                $form.data("adminSubmitting", true);

                var title = buttonText.indexOf("update") >= 0 || buttonText.indexOf("edit") >= 0
                    ? "Updating..."
                    : "Saving...";

                Swal.fire({
                    title: title,
                    text: "Please wait while your changes are processed.",
                    allowOutsideClick: false,
                    allowEscapeKey: false,
                    didOpen: function () {
                        Swal.showLoading();
                    }
                });

                $form.find("button[type='submit'], input[type='submit']").prop("disabled", true);

                return true;
            });
        },

        bindDeleteConfirmations: function () {
            if (!window.Swal) {
                return;
            }

            $(document).on("click", "a[href*='Delete']:not([data-swal-bound='true']), button[data-admin-delete='true']", function (event) {
                var element = this;
                var $element = $(element);

                if ($element.hasClass("btnDelete") || $element.hasClass("delete-btn") || $element.data("skipAdminDeleteAlert")) {
                    return;
                }

                event.preventDefault();

                Swal.fire({
                    title: "Delete this record?",
                    text: "This action cannot be undone.",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#dc3545",
                    cancelButtonColor: "#64748b",
                    confirmButtonText: "Delete"
                }).then(function (result) {
                    if (!result.isConfirmed) {
                        return;
                    }

                    if (element.tagName.toLowerCase() === "a") {
                        window.location.href = element.href;
                    } else {
                        $element.closest("form").data("skip-admin-submit-alert", true).trigger("submit");
                    }
                });
            });
        },

        dismissLegacyAlerts: function () {
            if (!window.adminFlash) {
                return;
            }

            if (window.adminFlash.success || window.adminFlash.error || window.adminFlash.warning) {
                $(".alert-success, .alert-danger, .alert-warning").hide();
            }
        }
    };

    $(function () {
        AdminUi.init();
    });
})(jQuery);
