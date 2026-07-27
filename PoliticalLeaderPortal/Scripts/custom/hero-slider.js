(function ($) {

    "use strict";

    /*==========================================================
        HERO SLIDER
    ==========================================================*/

    var HeroSlider = {

        swiper: null,

        videoEndedHandler: null,

        isPointerInside: false,

        selectors: {

            slider: ".heroSwiper",

            slide: ".swiper-slide",

            title: ".hero-title",

            subtitle: ".hero-subtitle",

            description: ".hero-description",

            buttons: ".hero-buttons",

            leader: ".hero-leader-image",

            overlay: ".hero-overlay"

        },

        settings: {

            autoplay: 5000,

            speed: 900,

            loop: true,

            effect: "fade",

            grabCursor: true,

            watchOverflow: true,

            preloadImages: true,

            lazy: false,

            observer: true,

            observeParents: true,

            keyboard: {

                enabled: true,

                onlyInViewport: true

            },

            pagination: {

                el: ".hero-swiper-pagination",

                clickable: true

            },

            navigation: {

                nextEl: ".hero-swiper-next",

                prevEl: ".hero-swiper-prev"

            }

        },

        cache: function () {

            this.$window = $(window);

            this.$document = $(document);

            this.$slider = $(this.selectors.slider);

            this.$slides = this.$slider.find(this.selectors.slide);

        },

        init: function () {

            this.cache();

            if (!this.$slider.length) {

                return;

            }

            this.initializeSwiper();

            this.bindEvents();

            this.initializeAnimations();

        },

        prepareHeroBackgrounds: function () {

            this.$slider.find(".hero-background").each(function () {

                var background = this;

                var image = background.querySelector("img.hero-background-img");

                if (!image || !image.getAttribute("src")) {

                    return;

                }

                background.style.backgroundImage = "url('" + image.getAttribute("src") + "')";

            });

        },
        initializeSwiper: function () {

            var self = this;

            if (typeof Swiper === "undefined") {

                return;

            }

            if (self.$slides.length <= 1) {

                self.settings.loop = false;

            }

            self.swiper = new Swiper(self.selectors.slider, {

                loop: self.settings.loop,

                speed: self.settings.speed,

                effect: self.settings.effect,

                fadeEffect: {

                    crossFade: true

                },

                grabCursor: self.settings.grabCursor,

                preloadImages: self.settings.preloadImages,

                watchOverflow: self.settings.watchOverflow,

                observer: self.settings.observer,

                observeParents: self.settings.observeParents,

                lazy: self.settings.lazy,

                keyboard: self.settings.keyboard,

                pagination: self.settings.pagination,

                navigation: self.settings.navigation,

                autoplay: {

                    delay: self.settings.autoplay,

                    disableOnInteraction: false,

                    pauseOnMouseEnter: false

                },

                on: {

                    init: function () {

                        self.playCurrentSlideAnimation();

                        self.syncSlidePlayback();

                        self.startAutoplay();

                    },

                    slideChangeTransitionStart: function () {

                        self.resetAnimations();

                        self.pauseInactiveVideos();

                    },

                    slideChangeTransitionEnd: function () {

                        self.playCurrentSlideAnimation();

                        self.syncSlidePlayback();

                    }

                }

            });

            window.setTimeout(function () {

                if (self.swiper) {

                    self.swiper.update();

                    self.startAutoplay();

                }

            }, 250);

        },

        bindEvents: function () {

            var self = this;

            self.$slider.on("mouseenter", function () {

                self.isPointerInside = true;

            });

            self.$slider.on("mouseleave", function () {

                self.isPointerInside = false;

                self.syncSlidePlayback();

            });

            self.$slider.on("click", ".hero-swiper-prev, .hero-swiper-next, .hero-swiper-pagination", function () {

                window.setTimeout(function () {

                    self.syncSlidePlayback();

                }, self.settings.speed + 50);

            });

            self.$document.on("visibilitychange", function () {

                if (!self.swiper || !self.swiper.autoplay) {

                    return;

                }

                if (document.hidden) {

                    self.pauseInactiveVideos(true);

                    self.swiper.autoplay.stop();

                }
                else {

                    self.syncSlidePlayback();

                }

            });

        },

        syncSlidePlayback: function () {

            var self = this;

            if (!self.swiper) {

                return;

            }

            var activeSlide = self.swiper.slides[self.swiper.activeIndex];

            var $activeSlide = $(activeSlide);

            var isVideoSlide = $activeSlide.data("hero-video") === true ||
                $activeSlide.data("hero-video") === "true";

            var videoKind = $activeSlide.data("hero-video-kind");

            self.pauseInactiveVideos();

                if (!isVideoSlide) {

                self.startAutoplay();

                return;

            }

            if (self.swiper.autoplay) {

                self.swiper.autoplay.stop();

            }

            if (videoKind !== "direct") {

                return;

            }

            self.playActiveVideo($activeSlide);

        },

        startAutoplay: function () {

            if (!this.swiper ||
                !this.swiper.autoplay ||
                document.hidden) {

                return;

            }

            if (this.$slides && this.$slides.length > 1) {

                this.swiper.autoplay.start();

            }

        },

        playActiveVideo: function ($activeSlide) {

            var self = this;

            var video = $activeSlide.find("video.hero-background-video").get(0);

            if (!video) {

                return;

            }

            if (self.videoEndedHandler) {

                $(video).off("ended.heroSlider", self.videoEndedHandler);

            }

            self.videoEndedHandler = function () {

                if (!self.swiper) {

                    return;

                }

                self.swiper.slideNext();

            };

            video.loop = false;
            video.muted = video.muted || video.hasAttribute("muted");

            $(video)
                .off("ended.heroSlider")
                .on("ended.heroSlider", self.videoEndedHandler);

            try {

                video.currentTime = 0;

            }
            catch (ex) {

                // Some streamed media cannot seek before metadata is ready.

            }

            var playPromise = video.play();

            if (playPromise && typeof playPromise.catch === "function") {

                playPromise.catch(function () {

                    if (self.swiper && self.swiper.autoplay && !document.hidden && !self.isPointerInside) {

                        self.swiper.autoplay.start();

                    }

                });

            }

        },

        pauseInactiveVideos: function (includeActive) {

            var self = this;

            if (!self.$slider || !self.$slider.length) {

                return;

            }

            self.$slider.find("video.hero-background-video").each(function () {

                var video = this;

                if (!includeActive &&
                    self.swiper &&
                    $(video).closest(".swiper-slide").get(0) === self.swiper.slides[self.swiper.activeIndex]) {

                    return;

                }

                video.pause();

            });

        },

        initializeAnimations: function () {
            var self = this;

            self.$slides.each(function () {

                var $slide = $(this);

                $slide.find(self.selectors.title)
                    .add(self.selectors.subtitle)
                    .add(self.selectors.description)
                    .add(self.selectors.buttons)
                    .add(self.selectors.leader)
                    .css({

                        opacity: 0

                    });

            });

            self.playCurrentSlideAnimation();

        },

        resetAnimations: function () {

            var self = this;

            self.$slides.each(function () {

                var $slide = $(this);

                $slide.find(self.selectors.title)
                    .add(self.selectors.subtitle)
                    .add(self.selectors.description)
                    .add(self.selectors.buttons)
                    .add(self.selectors.leader)
                    .removeClass(

                        "animate__animated " +
                        "animate__fadeIn " +
                        "animate__fadeInUp " +
                        "animate__fadeInDown " +
                        "animate__fadeInLeft " +
                        "animate__fadeInRight " +
                        "animate__zoomIn " +
                        "animate__bounceIn " +
                        "animate__pulse"

                    )
                    .css({

                        opacity: 0

                    });

            });

        },

        playCurrentSlideAnimation: function () {

            var self = this;

            if (!self.swiper) {

                return;

            }

            var $activeSlide = $(self.swiper.slides[self.swiper.activeIndex]);

            self.animateElement(

                $activeSlide.find(self.selectors.subtitle),

                "animate__fadeInDown",

                150

            );

            self.animateElement(

                $activeSlide.find(self.selectors.title),

                "animate__fadeInUp",

                350

            );

            self.animateElement(

                $activeSlide.find(self.selectors.description),

                "animate__fadeInUp",

                600

            );

            self.animateElement(

                $activeSlide.find(self.selectors.buttons),

                "animate__fadeInUp",

                900

            );

            self.animateElement(

                $activeSlide.find(self.selectors.leader),

                "animate__zoomIn",

                500

            );

        },

        animateElement: function ($element, animation, delay) {

            if (!$element.length) {

                return;

            }

            window.setTimeout(function () {

                $element.css({

                    opacity: 1

                });

                $element.addClass(

                    "animate__animated " + animation

                );

            }, delay);

        }, handleResize: function () {

            var self = this;

            self.$window.off("resize.heroSlider");

            self.$window.on(

                "resize.heroSlider",

                self.debounce(function () {

                    if (!self.swiper) {

                        return;

                    }

                    self.swiper.update();

                }, 250)

            );

        },

        initializeIntersectionObserver: function () {

            var self = this;

            if (!("IntersectionObserver" in window)) {

                return;

            }

            var observer = new IntersectionObserver(function (entries) {

                $.each(entries, function (_, entry) {

                    if (!self.swiper) {

                        return;
                    }

                    if (entry.isIntersecting) {

                        self.syncSlidePlayback();

                    }
                    else {

                        self.pauseInactiveVideos(true);

                        if (self.swiper.autoplay) {

                            self.swiper.autoplay.stop();

                        }

                    }

                });

            }, {

                threshold: 0.35

            });

            self.$slider.each(function () {

                observer.observe(this);

            });

        },

        debounce: function (callback, delay) {

            var timer;

            return function () {

                var context = this;

                var args = arguments;

                clearTimeout(timer);

                timer = setTimeout(function () {

                    callback.apply(context, args);

                }, delay);

            };

        },

        refresh: function () {

            if (this.swiper) {

                this.swiper.update();

            }

        },

        destroy: function () {

            if (this.swiper) {

                this.swiper.destroy(true, true);

                this.swiper = null;

            }

        },

        initialize: function () {

            this.init();

            this.handleResize();

            this.initializeIntersectionObserver();

        },
        cacheAnimations: function () {

            this.animationClasses = {

                fade: "animate__fadeIn",

                fadeUp: "animate__fadeInUp",

                fadeDown: "animate__fadeInDown",

                slideLeft: "animate__fadeInLeft",

                slideRight: "animate__fadeInRight",

                zoom: "animate__zoomIn",

                bounce: "animate__bounceIn",

                pulse: "animate__pulse"

            };

        },

        supportsReducedMotion: function () {

            return window.matchMedia &&
                window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        },

        pause: function () {

            if (this.swiper && this.swiper.autoplay) {

                this.swiper.autoplay.stop();

            }

        },

        resume: function () {

            if (this.swiper && this.swiper.autoplay) {

                this.swiper.autoplay.start();

            }

        },

        reinitialize: function () {

            this.destroy();

            this.init();

        },

        isMobile: function () {

            return window.innerWidth < 768;

        },

        isTablet: function () {

            return window.innerWidth >= 768 &&
                window.innerWidth < 992;

        },

        isDesktop: function () {

            return window.innerWidth >= 992;

        },

        throttle: function (callback, limit) {

            var waiting = false;

            return function () {

                if (!waiting) {

                    callback.apply(this, arguments);

                    waiting = true;

                    setTimeout(function () {

                        waiting = false;

                    }, limit);

                }

            };

        },
        ready: function () {

            this.cacheAnimations();

            this.initialize();

        }

    };

    $(function () {

        HeroSlider.ready();

    });

})(jQuery);





