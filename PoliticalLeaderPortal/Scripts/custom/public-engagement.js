(function () {
    var key = "som-public-hit-count";
    var sessionKey = "som-public-hit-counted";
    var current = parseInt(localStorage.getItem(key) || "0", 10);

    if (!sessionStorage.getItem(sessionKey)) {
        current += 1;
        localStorage.setItem(key, String(current));
        sessionStorage.setItem(sessionKey, "1");
    }

    var counter = document.getElementById("siteVisitCount");
    if (counter) {
        counter.textContent = current.toLocaleString();
    }
}());
