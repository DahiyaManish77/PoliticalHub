(function ($) {
    "use strict";

    var storagePrefix = "politicalPortal.publicActions.";

    function getKey($root) {
        return storagePrefix + ($root.data("item-type") || "item") + "." + ($root.data("item-id") || "0");
    }

    function readCount(key) {
        var value = parseInt(window.localStorage.getItem(key + ".likes") || "0", 10);
        return isNaN(value) ? 0 : value;
    }

    function writeCount($root, count) {
        $root.find("[data-like-count]").text(count);
    }

    function hydrateActions() {
        $("[data-public-actions]").each(function () {
            var $root = $(this);
            var key = getKey($root);
            var liked = window.localStorage.getItem(key + ".liked") === "true";
            var count = readCount(key);

            writeCount($root, count);
            $root.find("[data-action='like']").toggleClass("is-liked", liked);
        });
    }

    function absoluteUrl(url) {
        if (!url) {
            return window.location.href;
        }

        var anchor = document.createElement("a");
        anchor.href = url;
        return anchor.href;
    }

    function shareItem($root) {
        var title = $root.data("title") || document.title;
        var url = absoluteUrl($root.data("url"));

        if (navigator.share) {
            navigator.share({
                title: title,
                url: url
            });
            return;
        }

        if (navigator.clipboard) {
            navigator.clipboard.writeText(url);
            showMediaNotice("Link copied to clipboard.");
            return;
        }

        window.prompt("Copy this link", url);
    }

    function showMediaNotice(message) {
        var $notice = $(".media-action-notice");

        if (!$notice.length) {
            $notice = $("<div class=\"media-action-notice\" />").appendTo("body");
        }

        $notice.text(message).addClass("is-visible");

        window.clearTimeout($notice.data("timer"));

        $notice.data("timer", window.setTimeout(function () {
            $notice.removeClass("is-visible");
        }, 1800));
    }

    function ensureCommentModal() {
        var $modal = $("[data-media-comment-modal]");

        if ($modal.length) {
            return $modal;
        }

        $modal = $(
            "<div class=\"media-comment-modal\" data-media-comment-modal aria-hidden=\"true\">" +
            "  <div class=\"media-comment-backdrop\" data-media-comment-close></div>" +
            "  <div class=\"media-comment-dialog\" role=\"dialog\" aria-modal=\"true\" aria-label=\"Add comment\">" +
            "    <button type=\"button\" class=\"media-comment-close\" data-media-comment-close aria-label=\"Close comment\"><i class=\"bi bi-x-lg\"></i></button>" +
            "    <span class=\"media-comment-kicker\">Comment</span>" +
            "    <h3 data-media-comment-title>Share your thought</h3>" +
            "    <textarea data-media-comment-text maxlength=\"500\" placeholder=\"Write a short comment...\"></textarea>" +
            "    <div class=\"media-comment-actions\">" +
            "      <button type=\"button\" class=\"btn-cancel\" data-media-comment-close>Cancel</button>" +
            "      <button type=\"button\" class=\"btn-save\" data-media-comment-save>Save Comment</button>" +
            "    </div>" +
            "  </div>" +
            "</div>"
        ).appendTo("body");

        return $modal;
    }

    function commentItem($root) {
        var key = getKey($root);
        var existing = window.localStorage.getItem(key + ".comment") || "";
        var title = $root.data("title") || "Share your thought";
        var $modal = ensureCommentModal();

        $modal
            .data("comment-key", key)
            .addClass("is-open")
            .attr("aria-hidden", "false");

        $modal.find("[data-media-comment-title]").text(title);
        $modal.find("[data-media-comment-text]").val(existing).trigger("focus");
    }

    function likeItem($root) {
        var key = getKey($root);
        var liked = window.localStorage.getItem(key + ".liked") === "true";
        var count = readCount(key);

        liked = !liked;
        count = Math.max(0, count + (liked ? 1 : -1));

        window.localStorage.setItem(key + ".liked", liked ? "true" : "false");
        window.localStorage.setItem(key + ".likes", count.toString());

        writeCount($root, count);
        $root.find("[data-action='like']").toggleClass("is-liked", liked);
    }

    function bindPublicActions() {
        $(document).on("click", "[data-public-actions] [data-action]", function (event) {
            event.preventDefault();
            event.stopPropagation();

            var $button = $(this);
            var $root = $button.closest("[data-public-actions]");
            var action = $button.data("action");

            if (action === "like") {
                likeItem($root);
            }
            else if (action === "share") {
                shareItem($root);
            }
            else if (action === "comment") {
                commentItem($root);
            }
        });

        $(document).on("click", "[data-media-comment-close]", function () {
            $("[data-media-comment-modal]")
                .removeClass("is-open")
                .attr("aria-hidden", "true");
        });

        $(document).on("click", "[data-media-comment-save]", function () {
            var $modal = $(this).closest("[data-media-comment-modal]");
            var key = $modal.data("comment-key");
            var comment = $modal.find("[data-media-comment-text]").val();

            if (key) {
                window.localStorage.setItem(key + ".comment", comment || "");
            }

            $modal.removeClass("is-open").attr("aria-hidden", "true");
            showMediaNotice("Comment saved on this device.");
        });
    }

    function openGallery($card) {
        var src = $card.data("image-src");
        var title = $card.data("image-title") || "Photo Gallery";
        var id = $card.data("image-id") || src;
        var $lightbox = $("[data-gallery-lightbox]");

        if (!$lightbox.length || !src) {
            return;
        }

        $lightbox.find("[data-gallery-lightbox-img]")
            .attr("src", src)
            .attr("alt", title);

        $lightbox.find("[data-gallery-lightbox-title]").text(title);
        $lightbox.find("[data-gallery-download]").attr("href", src);

        var $actions = $lightbox.find("[data-public-actions]");
        $actions
            .attr("data-item-id", id)
            .attr("data-title", title)
            .attr("data-url", src)
            .data("item-id", id)
            .data("title", title)
            .data("url", src);

        $lightbox.addClass("is-open").attr("aria-hidden", "false");
        $("body").addClass("gallery-lightbox-open");
        hydrateActions();
    }

    function closeGallery() {
        $("[data-gallery-lightbox]")
            .removeClass("is-open")
            .attr("aria-hidden", "true");

        $("body").removeClass("gallery-lightbox-open");
    }

    function bindGalleryLightbox() {
        var $lightbox = $("[data-gallery-lightbox]");

        if ($lightbox.length && !$lightbox.parent().is("body")) {
            $lightbox.appendTo("body");
        }

        $(document).on("click", ".js-gallery-lightbox", function () {
            openGallery($(this));
        });

        $(document).on("keydown", ".js-gallery-lightbox", function (event) {
            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();
                openGallery($(this));
            }
        });

        $(document).on("click", "[data-gallery-close]", function () {
            closeGallery();
        });

        $(document).on("keydown", function (event) {
            if (event.key === "Escape") {
                closeGallery();
                $("[data-media-comment-modal]")
                    .removeClass("is-open")
                    .attr("aria-hidden", "true");
            }
        });
    }

    $(function () {
        hydrateActions();
        bindPublicActions();
        bindGalleryLightbox();
    });
})(jQuery);
