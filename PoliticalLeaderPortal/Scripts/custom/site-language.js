// ======================================================
// CUSTOM LANGUAGE SWITCHER
// PoliticalLeaderPortal
// ======================================================

$(document).ready(function () {

    // ===============================
    // Open / Close Language Dropdown
    // ===============================

    $("#languageButton").on("click", function (e) {

        e.preventDefault();

        e.stopPropagation();

        $(".custom-language-dropdown").toggleClass("active");

    });

    // ===============================
    // Close on Outside Click
    // ===============================

    $(document).on("click", function () {

        $(".custom-language-dropdown").removeClass("active");

    });

    $(".custom-language-dropdown").on("click", function (e) {

        e.stopPropagation();

    });

    // ===============================
    // Restore Saved Language
    // ===============================

    var savedLanguage = localStorage.getItem("SelectedLanguage");

    if (savedLanguage) {

        $("#selectedLanguage").text(savedLanguage === "hi" ? "हिन्दी" : "English");

        setTimeout(function () {

            translateLanguage(savedLanguage);

        }, 1200);

    }

    // ===============================
    // Language Selection
    // ===============================

    $(".language-menu li").on("click", function () {

        var language = $(this).attr("data-lang");

        localStorage.setItem("SelectedLanguage", language);

        $("#selectedLanguage").text(language === "hi" ? "हिन्दी" : "English");

        $(".custom-language-dropdown").removeClass("active");

        translateLanguage(language);

    });

});


// ======================================================
// GOOGLE TRANSLATE
// ======================================================

function translateLanguage(language) {

    var interval = setInterval(function () {

        var combo = document.querySelector(".goog-te-combo");

        if (combo) {

            combo.value = language;

            combo.dispatchEvent(new Event("change"));

            clearInterval(interval);

        }

    }, 500);

}