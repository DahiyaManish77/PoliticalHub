/*==========================================================
    File : hero-slider-admin.js
    Part : 1
    Enterprise Foundation
==========================================================*/

(function ($) {

    "use strict";

    /*==========================================================
        HERO SLIDER ADMIN
    ==========================================================*/

    var HeroSliderAdmin = {

        /*==========================================================
            SELECTORS
        ==========================================================*/

        selectors: {

            form: "#heroSliderForm",

            title: "#Title",

            subTitle: "#SubTitle",

            description: "#Description",

            buttonText: "#ButtonText",

            buttonUrl: "#ButtonUrl",

            buttonText2: "#ButtonText2",

            buttonUrl2: "#ButtonUrl2",

            backgroundImage: "#BackgroundImageFile",

            leaderImage: "#LeaderImageFile",

            mobileImage: "#MobileImageFile",

            templateCards: ".hero-template-card",

            leaderCards: ".hero-position-card",

            alignmentCards: ".hero-alignment-card",

            animationCards: ".hero-animation-card",

            overlayCards: ".hero-overlay-card",

            colorPalette: ".hero-color-item",

            colorPicker: "#BackgroundColor",

            overlayOpacity: "#OverlayOpacity",

            displayOrder: "#DisplayOrder",

            progressBar: ".hero-progress-bar",

            progressLabel: "#heroProgressLabel",

            qualityScore: "#heroQualityScore",

            previewContainer: ".hero-preview",

            previewTitle: "#previewTitle",

            previewSubTitle: "#previewSubTitle",

            previewDescription: "#previewDescription",

            previewPrimaryButton: ".hero-btn-primary-preview",

            previewSecondaryButton: ".hero-btn-secondary-preview",

            previewBackground: ".hero-preview-background",

            previewLeader: ".hero-preview-leader",

            summaryTitle: "#summaryTitle",

            summaryTemplate: "#summaryTemplate",

            summaryAlignment: "#summaryAlignment",

            summaryAnimation: "#summaryAnimation",

            summaryOverlay: "#summaryOverlay"

        },

        /*==========================================================
            SETTINGS
        ==========================================================*/

        settings: {

            maxTitle: 120,

            maxSubTitle: 180,

            maxDescription: 600,

            maxButton: 30,

            supportedImages: [

                "jpg",

                "jpeg",

                "png",

                "webp"

            ],

            maxUploadSize: 5,

            animationSpeed: 400,

            toastDuration: 2500

        },

        /*==========================================================
            STATE
        ==========================================================*/

        state: {

            dirty: false,

            uploadInProgress: false,

            publishScore: 0,

            currentTemplate: "",

            currentAlignment: "",

            currentAnimation: "",

            currentOverlay: "",

            currentColor: "",

            initialized: false

        },

        /*==========================================================
            CACHE
        ==========================================================*/

        cache: function () {

            this.$window = $(window);

            this.$document = $(document);

            this.$form = $(this.selectors.form);

            this.$preview = $(this.selectors.previewContainer);

        },

        /*==========================================================
            INIT
        ==========================================================*/

        init: function () {

            this.cache();

            if (!this.$form.length) {

                return;

            }

            this.bindEvents();

            this.initializePreview();

            this.calculatePublishScore();

        },

        /*==========================================================
            EVENT REGISTRATION
        ==========================================================*/

        bindEvents: function () {
            var self = this;

            self.bindCharacterCounters();

            self.bindLivePreview();

            self.bindMediaManager();

            self.bindAppearanceDesigner();

            self.bindAnimationStudio();

            self.bindPublishPanel();

            self.bindValidation();

            self.bindKeyboardSupport();

            self.$document.on(

                "input change",

                "input, textarea, select",

                function () {

                    self.state.dirty = true;

                    self.calculatePublishScore();

                }

            );

            self.$window.on(

                "beforeunload",

                function () {

                    if (!self.state.dirty) {

                        return;

                    }

                    return "You have unsaved Hero Slider changes.";

                }

            );

        },

        /*==========================================================
            CHARACTER COUNTERS
        ==========================================================*/

        bindCharacterCounters: function () {

            var self = this;

            $(".hero-counter").each(function () {

                var $counter = $(this);

                var target = $("#" + $counter.data("target"));

                var max = parseInt($counter.data("max"), 10);

                if (!target.length) {

                    return;

                }

                var updateCounter = function () {

                    var length = $.trim(target.val()).length;

                    $counter.text(length + " / " + max);

                    $counter.removeClass(

                        "text-success text-warning text-danger"

                    );

                    if (length >= max * 0.90) {

                        $counter.addClass("text-danger");

                    }
                    else if (length >= max * 0.70) {

                        $counter.addClass("text-warning");

                    }
                    else {

                        $counter.addClass("text-success");

                    }

                };

                updateCounter();

                target.on(

                    "keyup input change",

                    updateCounter

                );

            });

        },

        /*==========================================================
            LIVE CONTENT PREVIEW
        ==========================================================*/

        bindLivePreview: function () {

            var self = this;

            $(self.selectors.title).on(

                "keyup input change",

                function () {

                    var value = $.trim($(this).val());

                    if (value === "") {

                        value = "Leader Name";

                    }

                    $(self.selectors.previewTitle).text(value);

                    $(self.selectors.summaryTitle).text(value);

                }

            );

            $(self.selectors.subTitle).on(

                "keyup input change",

                function () {

                    var value = $.trim($(this).val());

                    if (value === "") {

                        value = "Leader Subtitle";

                    }

                    $(self.selectors.previewSubTitle).text(value);

                }

            );

            $(self.selectors.description).on(

                "keyup input change",

                function () {

                    var value = $.trim($(this).val());

                    if (value === "") {

                        value = "Hero description will appear here.";

                    }

                    $(self.selectors.previewDescription).text(value);

                }

            );

            $(self.selectors.buttonText).on(

                "keyup input change",

                function () {

                    var value = $.trim($(this).val());

                    if (value === "") {

                        value = "Primary Button";

                    }

                    $(self.selectors.previewPrimaryButton).text(value);

                }

            );

            $(self.selectors.buttonText2).on(

                "keyup input change",

                function () {

                    var value = $.trim($(this).val());

                    if (value === "") {

                        value = "Secondary Button";

                    }

                    $(self.selectors.previewSecondaryButton).text(value);

                }

            );

        },

        /*==========================================================
            MEDIA MANAGER
        ==========================================================*/

        bindMediaManager: function () {
            var self = this;

            self.bindImageUploader(

                self.selectors.backgroundImage,

                ".hero-background-card"

            );

            self.bindImageUploader(

                self.selectors.leaderImage,

                ".hero-leader-card"

            );

            self.bindImageUploader(

                self.selectors.mobileImage,

                ".hero-mobile-card"

            );

            self.bindDragAndDrop();

            self.bindReplaceImage();

            self.bindRemoveImage();

            self.bindImagePreview();

        },

        bindImageUploader: function (inputSelector, cardSelector) {

            var self = this;

            self.$document.on(

                "change",

                inputSelector,

                function () {

                    if (!this.files || !this.files.length) {

                        return;

                    }

                    var file = this.files[0];

                    if (!self.validateImage(file)) {

                        this.value = "";

                        return;

                    }

                    var reader = new FileReader();

                    reader.onload = function (e) {

                        var $card = $(cardSelector);

                        $card.find(".hero-upload-zone").hide();

                        $card.find(".hero-image-preview")
                            .addClass("active")
                            .show();

                        $card.find(".hero-image-preview img")
                            .attr("src", e.target.result);

                        self.updateImageInformation(

                            $card,

                            file

                        );

                        self.simulateUploadProgress(

                            $card

                        );

                        self.updateLivePreview();

                        self.calculatePublishScore();

                    };

                    reader.readAsDataURL(file);

                }

            );

        },

        bindDragAndDrop: function () {
            var self = this;

            $(".hero-upload-zone").each(function () {

                var $zone = $(this);

                $zone.on(

                    "dragenter dragover",

                    function (e) {

                        e.preventDefault();

                        e.stopPropagation();

                        $zone.addClass("drag-active");

                    }

                );

                $zone.on(

                    "dragleave dragend drop",

                    function (e) {

                        e.preventDefault();

                        e.stopPropagation();

                        $zone.removeClass("drag-active");

                    }

                );

                $zone.on(

                    "drop",

                    function (e) {

                        var files = e.originalEvent.dataTransfer.files;

                        if (!files.length) {

                            return;

                        }

                        var input = $zone
                            .closest(".hero-media-card")
                            .find("input[type=file]");

                        if (!input.length) {

                            return;

                        }

                        input[0].files = files;

                        input.trigger("change");

                    }

                );

            });

        },

        bindReplaceImage: function () {

            var self = this;

            self.$document.on(

                "click",

                ".hero-btn-replace",

                function () {

                    $(this)
                        .closest(".hero-media-card")
                        .find("input[type=file]")
                        .trigger("click");

                }

            );

        },

        bindRemoveImage: function () {

            var self = this;

            self.$document.on(

                "click",

                ".hero-btn-remove",

                function () {

                    var $card = $(this)
                        .closest(".hero-media-card");

                    $card.find("input[type=file]").val("");

                    $card.find(".hero-upload-zone").show();

                    $card.find(".hero-image-preview")
                        .removeClass("active")
                        .hide();

                    $card.find(".hero-image-preview img")
                        .attr("src", "");

                    self.resetImageInformation(

                        $card

                    );

                    self.calculatePublishScore();

                    self.updateLivePreview();

                }

            );

        },

        bindImagePreview: function () {
            var self = this;

            self.$document.on(

                "click",

                ".hero-btn-preview",

                function () {

                    var $card = $(this)
                        .closest(".hero-media-card");

                    var image = $card
                        .find(".hero-image-preview img")
                        .attr("src");

                    if (!image) {

                        return;

                    }

                    $("#heroPreviewImage")
                        .attr("src", image);

                    $("#heroPreviewModal")
                        .modal("show");

                }

            );

        },

        updateImageInformation: function ($card, file) {

            var size = (file.size / 1024 / 1024).toFixed(2);

            $card.find(".hero-file-name")
                .text(file.name);

            $card.find(".hero-file-size")
                .text(size + " MB");

            $card.find(".hero-file-type")
                .text(file.type);

        },

        resetImageInformation: function ($card) {

            $card.find(".hero-file-name")
                .text("--");

            $card.find(".hero-file-size")
                .text("--");

            $card.find(".hero-file-type")
                .text("--");

            $card.find(".progress-bar")
                .css("width", "0%");

            $card.find(".hero-upload-percent")
                .text("0%");

        },

        simulateUploadProgress: function ($card) {

            var progress = 0;

            var bar = $card.find(".progress-bar");

            var label = $card.find(".hero-upload-percent");

            var timer = setInterval(function () {

                progress += 5;

                bar.css(

                    "width",

                    progress + "%"

                );

                label.text(

                    progress + "%"

                );

                if (progress >= 100) {

                    clearInterval(timer);

                }

            }, 20);

        },

        validateImage: function (file) {

            var self = this;

            var extension = file.name
                .split(".")
                .pop()
                .toLowerCase();

            if ($.inArray(

                extension,

                self.settings.supportedImages

            ) === -1) {

                self.showToast(

                    "Unsupported image format.",

                    "danger"

                );

                return false;

            }

            if ((file.size / 1024 / 1024) >

                self.settings.maxUploadSize) {

                self.showToast(

                    "Image exceeds maximum upload size.",

                    "danger"

                );

                return false;

            }

            return true;

        },

        /*==========================================================
            APPEARANCE DESIGNER
        ==========================================================*/

        bindAppearanceDesigner: function () {
            var self = this;

            self.$document.on(

                "click",

                self.selectors.templateCards,

                function () {

                    var $this = $(this);

                    self.selectors.templateCards &&
                        $(self.selectors.templateCards)
                            .removeClass("active");

                    $this.addClass("active");

                    self.state.currentTemplate =

                        $this.data("template");

                    $("#TemplateType").val(

                        self.state.currentTemplate

                    );

                    $(self.selectors.summaryTemplate)

                        .text(self.state.currentTemplate);

                    self.updateLivePreview();

                    self.calculatePublishScore();

                }

            );

            self.$document.on(

                "click",

                self.selectors.leaderCards,

                function () {

                    var $this = $(this);

                    $(self.selectors.leaderCards)

                        .removeClass("active");

                    $this.addClass("active");

                    self.state.currentAlignment =

                        $this.data("position");

                    $("#LeaderImagePosition").val(

                        self.state.currentAlignment

                    );

                    self.updateLeaderPosition();

                }

            );

            self.$document.on(

                "click",

                self.selectors.alignmentCards,

                function () {

                    var $this = $(this);

                    $(self.selectors.alignmentCards)

                        .removeClass("active");

                    $this.addClass("active");

                    var alignment =

                        $this.data("alignment");

                    $("#TextAlignment")

                        .val(alignment);

                    $(self.selectors.summaryAlignment)

                        .text(alignment);

                    self.updateTextAlignment();

                }

            );

            self.$document.on(

                "click",

                self.selectors.colorPalette,

                function () {

                    var color =

                        $(this).data("color");

                    $(self.selectors.colorPicker)

                        .val(color)

                        .trigger("change");

                }

            );

            $(self.selectors.colorPicker).on(

                "change input",

                function () {

                    self.state.currentColor =

                        $(this).val();

                    self.updateBackgroundColor();

                }

            );

        },

        updateBackgroundColor: function () {

            var self = this;

            $(self.selectors.previewBackground)

                .css(

                    "background-color",

                    self.state.currentColor

                );

        },

        updateLeaderPosition: function () {

            var self = this;

            $(self.selectors.previewLeader)

                .removeClass(

                    "left right center"

                )

                .addClass(

                    self.state.currentAlignment

                );

        },

        updateTextAlignment: function () {

            var alignment =

                $("#TextAlignment").val();

            $(".hero-preview-content")

                .removeClass(

                    "text-start text-center text-end"

                );

            if (alignment === "Center") {

                $(".hero-preview-content")

                    .addClass("text-center");

            }
            else if (alignment === "Right") {

                $(".hero-preview-content")

                    .addClass("text-end");

            }
            else {

                $(".hero-preview-content")

                    .addClass("text-start");

            }

        },

        /*==========================================================
            ANIMATION STUDIO
        ==========================================================*/

        bindAnimationStudio: function () {
            var self = this;

            self.$document.on(

                "click",

                self.selectors.animationCards,

                function () {

                    var $this = $(this);

                    var group = $this.data("group");

                    $(".hero-animation-card[data-group='" + group + "']")
                        .removeClass("active");

                    $this.addClass("active");

                    var animation = $this.data("animation");

                    switch (group) {

                        case "title":

                            $("#TitleAnimation").val(animation);

                            break;

                        case "subtitle":

                            $("#SubTitleAnimation").val(animation);

                            break;

                        case "description":

                            $("#DescriptionAnimation").val(animation);

                            break;

                        case "button":

                            $("#ButtonAnimation").val(animation);

                            break;

                    }

                    $(self.selectors.summaryAnimation)

                        .text(animation);

                    self.playPreviewAnimation();

                }

            );

            self.$document.on(

                "click",

                self.selectors.overlayCards,

                function () {

                    var $this = $(this);

                    $(self.selectors.overlayCards)

                        .removeClass("active");

                    $this.addClass("active");

                    var overlay =

                        $this.data("overlay");

                    $("#OverlayType").val(overlay);

                    self.state.currentOverlay = overlay;

                    $(self.selectors.summaryOverlay)

                        .text(overlay);

                    self.updateOverlayPreview();

                }

            );

            $(self.selectors.overlayOpacity).on(

                "input change",

                function () {

                    self.updateOverlayOpacity(

                        $(this).val()

                    );

                }

            );

        },

        playPreviewAnimation: function () {

            var self = this;

            var $preview =

                $(self.selectors.previewContainer);

            $preview.removeClass(

                "hero-preview-animate"

            );

            window.setTimeout(function () {

                $preview.addClass(

                    "hero-preview-animate"

                );

            }, 50);

        },

        updateOverlayPreview: function () {

            var self = this;

            var $overlay =

                self.$preview.find(".hero-preview-overlay");

            $overlay
                .removeClass(function (index, className) {

                    return (className.match(/(^|\s)overlay-\S+/g) || []).join(" ");

                })
                .addClass(

                    "overlay-" + self.state.currentOverlay

                );

        },

        updateOverlayOpacity: function (opacity) {

            $(".hero-preview-overlay")

                .css(

                    "opacity",

                    opacity

                );

        },

        /*==========================================================
            PUBLISH PANEL
        ==========================================================*/

        bindPublishPanel: function () {
            var self = this;

            self.calculatePublishScore();

            self.$document.on(

                "input change",

                "input, textarea, select",

                function () {

                    self.calculatePublishScore();

                }

            );

        },

        calculatePublishScore: function () {

            var self = this;

            var score = 0;

            if ($.trim($(self.selectors.title).val()) !== "") {

                score += 10;

            }

            if ($.trim($(self.selectors.subTitle).val()) !== "") {

                score += 10;

            }

            if ($.trim($(self.selectors.description).val()) !== "") {

                score += 15;

            }

            if ($("#TemplateType").val() !== "") {

                score += 10;

            }

            if ($("#LeaderImagePosition").val() !== "") {

                score += 5;

            }

            if ($("#TextAlignment").val() !== "") {

                score += 5;

            }

            if ($("#TitleAnimation").val() !== "") {

                score += 5;

            }

            if ($("#OverlayType").val() !== "") {

                score += 5;

            }

            if ($(self.selectors.backgroundImage).val() !== "") {

                score += 20;

            }

            if ($(self.selectors.buttonText).val() !== "") {

                score += 5;

            }

            if ($(self.selectors.buttonUrl).val() !== "") {

                score += 10;

            }

            self.state.publishScore = score;

            $(self.selectors.progressBar)

                .css("width", score + "%");

            $(self.selectors.progressLabel)

                .text(score + "% Complete");

            $(self.selectors.qualityScore)

                .text(score + "/100");

            self.updatePublishChecklist();

        },

        updatePublishChecklist: function () {

            var self = this;

            $(".hero-check-item").each(function () {

                var field = $(this).data("field");

                var valid = false;

                switch (field) {

                    case "title":

                        valid = $.trim($(self.selectors.title).val()) !== "";

                        break;

                    case "background":

                        valid = $(self.selectors.backgroundImage).val() !== "";

                        break;

                    case "template":

                        valid = $("#TemplateType").val() !== "";

                        break;

                    case "button":

                        valid = $.trim($(self.selectors.buttonText).val()) !== "";

                        break;

                }

                $(this)

                    .toggleClass("completed", valid)

                    .toggleClass("pending", !valid);

            });

        },

        /*==========================================================
            VALIDATION
        ==========================================================*/

        bindValidation: function () {
            var self = this;

            self.$form.on(

                "submit",

                function (e) {

                    if (!self.validateForm()) {

                        e.preventDefault();

                        self.showToast(

                            "Please complete all required fields.",

                            "danger"

                        );

                        self.scrollToFirstError();

                    }

                }

            );

            self.$document.on(

                "blur",

                "input[type=url], input[data-url='true']",

                function () {

                    self.validateUrl($(this));

                }

            );

        },

        validateForm: function () {

            var self = this;

            var valid = true;

            self.$form.find(".is-invalid")

                .removeClass("is-invalid");

            if ($.trim($(self.selectors.title).val()) === "") {

                $(self.selectors.title)

                    .addClass("is-invalid");

                valid = false;

            }

            if ($.trim($(self.selectors.description).val()) === "") {

                $(self.selectors.description)

                    .addClass("is-invalid");

                valid = false;

            }

            if ($("#TemplateType").val() === "") {

                valid = false;

            }

            if ($(self.selectors.backgroundImage).val() === "" &&

                $(".hero-background-card img").attr("src") === "") {

                valid = false;

            }

            self.validateUrl(

                $(self.selectors.buttonUrl)

            );

            self.validateUrl(

                $(self.selectors.buttonUrl2)

            );

            return valid;

        },

        validateUrl: function ($input) {

            if (!$input.length) {

                return true;

            }

            var value = $.trim($input.val());

            if (value === "") {

                $input.removeClass(

                    "is-valid is-invalid"

                );

                return true;

            }

            var expression =

                /^(https?:\/\/|\/)/i;

            if (expression.test(value)) {

                $input.removeClass("is-invalid")

                    .addClass("is-valid");

                return true;

            }

            $input.removeClass("is-valid")

                .addClass("is-invalid");

            return false;

        },

        scrollToFirstError: function () {

            var $error = $(".is-invalid:first");

            if (!$error.length) {

                return;

            }

            $("html, body").animate({

                scrollTop:

                    $error.offset().top - 120

            }, 500);

        },

        /*==========================================================
            KEYBOARD SUPPORT
        ==========================================================*/

        bindKeyboardSupport: function () {
            var self = this;

            self.$document.on(

                "keydown",

                function (e) {

                    if (e.key === "Escape") {

                        $(".modal").modal("hide");

                    }

                    if (e.ctrlKey && e.keyCode === 83) {

                        e.preventDefault();

                        self.$form.trigger("submit");

                    }

                }

            );

        },

        /*==========================================================
            LIVE PREVIEW
        ==========================================================*/

        initializePreview: function () {

            this.updateLivePreview();

        },

        updateLivePreview: function () {

            var self = this;

            $(self.selectors.previewTitle)

                .text(

                    $.trim($(self.selectors.title).val()) ||

                    "Leader Name"

                );

            $(self.selectors.previewSubTitle)

                .text(

                    $.trim($(self.selectors.subTitle).val()) ||

                    "Leader Subtitle"

                );

            $(self.selectors.previewDescription)

                .text(

                    $.trim($(self.selectors.description).val()) ||

                    "Hero description will appear here."

                );

            $(self.selectors.previewPrimaryButton)

                .text(

                    $.trim($(self.selectors.buttonText).val()) ||

                    "Primary Button"

                );

            $(self.selectors.previewSecondaryButton)

                .text(

                    $.trim($(self.selectors.buttonText2).val()) ||

                    "Secondary Button"

                );

            self.updateBackgroundColor();

            self.updateLeaderPosition();

            self.updateOverlayPreview();

        },

        /*==========================================================
            TOAST
        ==========================================================*/

        showToast: function (message, type) {

            var css = "success";

            if (type) {

                css = type;

            }

            if (window.Swal) {

                Swal.fire({

                    toast: true,

                    position: "top-end",

                    icon: css,

                    title: message,

                    timer: this.settings.toastDuration,

                    showConfirmButton: false

                });

            }

            else {

                alert(message);

            }

        },

        /*==========================================================
            UTILITIES
        ==========================================================*/

        debounce: function (callback, delay) {

            var timer;

            return function () {

                var context = this;

                var args = arguments;

                clearTimeout(timer);

                timer = setTimeout(function () {

                    callback.apply(

                        context,

                        args

                    );

                }, delay);

            };

        },

        throttle: function (callback, delay) {

            var waiting = false;

            return function () {

                if (waiting) {

                    return;

                }

                callback.apply(

                    this,

                    arguments

                );

                waiting = true;

                setTimeout(function () {

                    waiting = false;

                }, delay);

            };

        },

        destroy: function () {
            var self = this;

            self.$document.off();

            self.$window.off();

            if (self.$form.length) {

                self.$form.off();

            }

        },

        ready: function () {

            this.init();

        }

    };

    $(function () {

        HeroSliderAdmin.ready();

    });

})(jQuery);