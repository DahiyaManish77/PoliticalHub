(function ($) {
    "use strict";

    function postAction(url) {
        $.post(url, function (response) {
            if (response && response.message) {
                alert(response.message);
            }

            if (!response || response.success) {
                window.location.reload();
            }
        });
    }

    $(function () {
        $(document).on("click", ".js-poll-delete,.js-poll-action", function () {
            var url = $(this).data("url");

            if (!url) {
                return;
            }

            if (window.confirm("Continue with this action?")) {
                postAction(url);
            }
        });
    });
})(jQuery);
