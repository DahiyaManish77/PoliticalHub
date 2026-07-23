/*!
==========================================================
Enterprise Latest News Module
Political Leader Portal
Version : 2.1
Author : Enterprise UI Team
==========================================================
*/

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        initializeNewsModule();
    });

    // Expose a global re-init for partial views
    window.refreshNewsModule = function () {
        initializeNewsModule();
    };

    function initializeNewsModule() {
        initializeRevealAnimation();
        initializeImageEffects();
        initializeAccessibility();
    }

    /*==========================================================
        REVEAL ANIMATION
    ==========================================================*/
    function initializeRevealAnimation() {
        if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            $(".reveal-element").addClass("is-visible");
            return;
        }

        var observer = new IntersectionObserver(
            function (entries) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) return;
                    requestAnimationFrame(function () {
                        entry.target.classList.add("is-visible");
                    });
                    observer.unobserve(entry.target);
                });
            },
            {
                threshold: 0.18,
                rootMargin: "0px 0px -60px 0px"
            }
        );

        $(".reveal-element").each(function () {
            observer.observe(this);
        });
    }

    /*==========================================================
        IMAGE LOADING
    ==========================================================*/
    function initializeImageEffects() {
        $(".editorial-newsroom-section img").each(function () {
            var image = this;
            if (image.complete) {
                image.classList.add("loaded");
                return;
            }
            image.addEventListener("load", function () {
                image.classList.add("loaded");
            });
        });
    }

    /*==========================================================
        ACCESSIBILITY
    ==========================================================*/
    function initializeAccessibility() {
        $(".editorial-story-title a, .editorial-lead-title a")
            .attr("title", "Read Full News");

        $(".editorial-read-link, .editorial-story-link")
            .attr("aria-label", "Read full article");
    }

})();
