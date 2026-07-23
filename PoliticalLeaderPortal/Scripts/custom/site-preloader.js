(function () {
    var preloader = document.getElementById("siteVideoPreloader");
    if (!preloader) return;

    document.body.classList.add("preloader-active");

    function hidePreloader() {
        if (preloader.classList.contains("is-hidden")) return;
        preloader.classList.add("is-hidden");
        document.body.classList.remove("preloader-active");
        window.setTimeout(function () {
            if (preloader && preloader.parentNode) {
                preloader.parentNode.removeChild(preloader);
            }
        }, 700);
    }

    var video = preloader.querySelector("video");
    if (video) {
        video.addEventListener("ended", hidePreloader);
        video.play().catch(function () {
            window.setTimeout(hidePreloader, 1400);
        });
    }

    window.addEventListener("load", function () {
        window.setTimeout(hidePreloader, 2800);
    });

    window.setTimeout(hidePreloader, 4200);
}());
