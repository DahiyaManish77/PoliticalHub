(function ($) {
    "use strict";
    $(function () {
        var $root = $("[data-people-say]");
        if (!$root.length) return;
        // Keep the Bootstrap modal outside section-level stacking contexts so its
        // backdrop never blocks the form controls.
        var $uploadModal = $root.find("#peopleSayUploadModal");
        if ($uploadModal.length) $uploadModal.appendTo(document.body);
        var $videoInput = $uploadModal.find('input[name="VideoFile"]');
        var $videoStatus = $uploadModal.find(".people-say-video-validation");
        $videoInput.on("change", function () {
            var input = this, file = input.files && input.files[0];
            input.dataset.landscapeValid = "";
            $videoStatus.removeClass("is-valid is-error").text("");
            if (!file) return;
            var preview = document.createElement("video");
            var objectUrl = URL.createObjectURL(file);
            preview.preload = "metadata";
            preview.onloadedmetadata = function () {
                URL.revokeObjectURL(objectUrl);
                if (preview.videoWidth <= preview.videoHeight) {
                    input.value = "";
                    $videoStatus.addClass("is-error").text("Portrait video rejected. Please record again while holding your phone horizontally.");
                    return;
                }
                input.dataset.landscapeValid = "true";
                $videoStatus.addClass("is-valid").text("Landscape video confirmed and ready to upload.");
            };
            preview.onerror = function () {
                URL.revokeObjectURL(objectUrl);
                input.value = "";
                $videoStatus.addClass("is-error").text("This video could not be checked. Please choose a valid MP4, WEBM or MOV file.");
            };
            preview.src = objectUrl;
        });
        $uploadModal.on("submit", "form", function (event) {
            if ($videoInput[0] && $videoInput[0].dataset.landscapeValid !== "true") {
                event.preventDefault();
                $videoStatus.addClass("is-error").text("Please choose and confirm a horizontal landscape video before submitting.");
            }
        });
        $root.on("click", ".people-say-toast button", function () { $(this).closest(".people-say-toast").fadeOut(180); });
        window.setTimeout(function () { $root.find(".people-say-toast.is-success").fadeOut(300); }, 8000);
        var $track = $root.find(".people-say-track"), $lightbox = $root.find(".people-say-lightbox");
        var $player = $lightbox.find(".people-say-player"), $youtubePlayer = $lightbox.find(".people-say-youtube-player"), currentId = 0, responseUrl = "";
        function token($scope) { return $scope.find('input[name="__RequestVerificationToken"]').first().val() || $root.find('input[name="__RequestVerificationToken"]').first().val(); }
        function engage(id, type, done) {
            $.post($root.data("engage-url"), { id: id, type: type, __RequestVerificationToken: token($root) }).always(function (r) { if (done) done(r); });
        }
        function open(card) {
            currentId = Number(card.data("video-id")); responseUrl = card.data("response-url") || "";
            var isYouTube = String(card.data("youtube")).toLowerCase() === "true";
            if (isYouTube) {
                $player[0].pause(); $player.removeAttr("src").prop("hidden", true);
                $youtubePlayer.attr("src", card.data("video-url")).prop("hidden", false);
            } else {
                $youtubePlayer.removeAttr("src").prop("hidden", true);
                $player.prop("hidden", false).attr("src", card.data("video-url"))[0].play();
            }
            $lightbox.addClass("is-open").attr("aria-hidden", "false");
            $lightbox.find(".people-say-response-panel").prop("hidden", !responseUrl);
            $lightbox.find(".people-say-comment-panel").prop("hidden", true);
            if (!isYouTube) engage(currentId, "View");
        }
        function close() { $player[0].pause(); $player.removeAttr("src"); $youtubePlayer.removeAttr("src"); $lightbox.removeClass("is-open").attr("aria-hidden", "true"); }
        $root.on("click", ".js-people-video", function () { open($(this).closest(".people-say-card")); });
        $root.on("click", ".people-say-close", close);
        $lightbox.on("click", function (e) { if (e.target === this) close(); });
        $(document).on("keydown", function (e) { if (e.key === "Escape") close(); });
        $root.on("click", ".people-say-prev", function () { $track[0].scrollBy({ left: -360, behavior: "smooth" }); });
        $root.on("click", ".people-say-next", function () { $track[0].scrollBy({ left: 360, behavior: "smooth" }); });
        $root.on("click", ".js-people-like", function () {
            var $button = $(this), id = $button.closest(".people-say-card").data("video-id");
            engage(id, "Like", function (r) { if (r && r.success) { var $count = $button.find("span"); $count.text(Number($count.text()) + 1); $button.addClass("is-liked"); } });
        });
        $root.on("click", ".js-people-share", function () {
            var $button = $(this), card = $button.closest(".people-say-card"), id = card.data("video-id"), url = location.origin + location.pathname + "#people-say";
            if (id > 0) engage(id, "Share");
            if (navigator.share) navigator.share({ title: card.find("h3").text(), url: url });
            else { navigator.clipboard.writeText(url); alert("Link copied."); }
        });
        function loadComments(id) {
            var $list = $lightbox.find(".people-say-comment-list").html("<div class='text-muted'>Loading comments…</div>");
            $.getJSON($root.data("comments-url"), { id: id }).done(function (items) {
                $list.empty();
                if (!items.length) $list.html("<div class='text-muted'>No approved comments yet.</div>");
                $.each(items, function (_, x) { $("<div class='people-say-comment-item'/>").append($("<strong/>").text(x.PersonName + ": ")).append(document.createTextNode(x.CommentText)).appendTo($list); });
            });
        }
        $root.on("click", ".js-people-comments", function () {
            var card = $(this).closest(".people-say-card"); if (!$lightbox.hasClass("is-open")) open(card);
            currentId = Number(card.data("video-id")); $lightbox.find(".people-say-comment-panel").prop("hidden", false);
            $lightbox.find('[name="PeopleSayVideoId"]').val(currentId); loadComments(currentId);
        });
        $lightbox.on("submit", ".people-say-comment-form", function (e) {
            e.preventDefault(); var $form = $(this), data = $form.serialize();
            $.post($root.data("comment-url"), data).done(function (r) { $form.find(".people-say-comment-status").text(r.message); if (r.success) $form.find("textarea").val(""); });
        });
        $root.on("click", ".js-play-response", function () { if (responseUrl) { $player.attr("src", responseUrl)[0].play(); } });
        if ($track.length && $track.children().length > 3) {
            window.setInterval(function () { if (!$track.is(":hover")) { var max = $track[0].scrollWidth - $track[0].clientWidth; $track[0].scrollTo({ left: $track[0].scrollLeft >= max - 10 ? 0 : $track[0].scrollLeft + 360, behavior: "smooth" }); } }, 4500);
        }
    });
})(jQuery);
