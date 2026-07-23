document.addEventListener("DOMContentLoaded", function () {

    var counters =
        document.querySelectorAll(".counter");

    if (!counters.length) {
        return;
    }

    var started = false;

    function runCounters() {

        if (started)
            return;

        started = true;

        counters.forEach(function (counter) {

            var target =
                parseInt(
                    counter.getAttribute("data-target")
                );

            var count = 0;

            var increment =
                Math.ceil(target / 120);

            var timer =
                setInterval(function () {

                    count += increment;

                    if (count >= target) {

                        count = target;

                        clearInterval(timer);
                    }

                    counter.innerText =
                        count.toLocaleString();

                }, 15);

        });

    }

    var section =
        document.querySelector(
            ".home-statistics-section"
        );

    if (!section) {
        runCounters();
        return;
    }

    var observer =
        new IntersectionObserver(
            function (entries) {

                entries.forEach(function (entry) {

                    if (entry.isIntersecting) {

                        runCounters();
                    }

                });

            },
            {
                threshold: 0.3
            });

    observer.observe(section);

});
