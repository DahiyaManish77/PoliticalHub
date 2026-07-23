(function ($) {
    "use strict";

    var HelpAgent = {
        selectors: {
            root: "[data-ai-help-agent]",
            toggle: "[data-ai-help-toggle]",
            close: "[data-ai-help-close]",
            panel: "[data-ai-help-panel]",
            body: "[data-ai-help-body]",
            form: "[data-ai-help-form]",
            input: "[data-ai-help-input]",
            suggestion: "[data-ai-help-question]"
        },

        init: function () {
            var self = this;

            self.$root = $(self.selectors.root);

            if (!self.$root.length) {
                return;
            }

            self.context = self.$root.data("context") || "public";
            self.bindEvents();
        },

        bindEvents: function () {
            var self = this;

            self.$root.on("click", self.selectors.toggle, function () {
                self.toggle();
            });

            self.$root.on("click", self.selectors.close, function () {
                self.close();
            });

            self.$root.on("click", self.selectors.suggestion, function () {
                self.ask($(this).data("ai-help-question"));
            });

            self.$root.on("submit", self.selectors.form, function (event) {
                event.preventDefault();

                var $input = self.$root.find(self.selectors.input);
                var question = $.trim($input.val());

                if (!question) {
                    return;
                }

                $input.val("");
                self.ask(question);
            });

            $(document).on("keydown.aiHelpAgent", function (event) {
                if (event.key === "Escape") {
                    self.close();
                }
            });
        },

        toggle: function () {
            if (this.$root.hasClass("is-open")) {
                this.close();
                return;
            }

            this.open();
        },

        open: function () {
            this.$root.addClass("is-open");
            this.$root.find(this.selectors.toggle).attr("aria-expanded", "true");
            this.$root.find(this.selectors.input).trigger("focus");
        },

        close: function () {
            this.$root.removeClass("is-open");
            this.$root.find(this.selectors.toggle).attr("aria-expanded", "false");
        },

        ask: function (question) {
            var self = this;

            self.open();
            self.addMessage(question, "user");

            window.setTimeout(function () {
                self.addMessage(self.getAnswer(question), "bot");
            }, 220);
        },

        addMessage: function (message, type) {
            var $body = this.$root.find(this.selectors.body);
            var safeMessage = $("<div>").text(message).html();
            var messageClass = type === "user" ? "ai-help-message-user" : "ai-help-message-bot";
            var icon = type === "user" ? "bi-person-fill" : "bi-robot";
            var avatar = type === "user" ? "" : "<span class=\"ai-help-avatar\"><i class=\"bi " + icon + "\"></i></span>";

            $body.append(
                "<div class=\"ai-help-message " + messageClass + "\">" +
                avatar +
                "<div>" + safeMessage + "</div>" +
                "</div>"
            );

            $body.scrollTop($body.prop("scrollHeight"));
        },

        getAnswer: function (question) {
            var text = question.toLowerCase();

            if (this.context === "admin") {
                return this.getAdminAnswer(text);
            }

            return this.getPublicAnswer(text);
        },

        getAdminAnswer: function (text) {
            if (this.hasAny(text, ["voter", "aadhaar", "aadhar", "village", "assembly", "booth"])) {
                return "Go to Admin > Voter Module. Use Create to add a voter with photo, Aadhaar details, state, district, block, assembly, parliament constituency, village and ward. Duplicate records are checked before saving.";
            }

            if (this.hasAny(text, ["hero", "slider", "image", "video", "banner"])) {
                return "Go to Admin > Hero Slider. Create or edit a slide, upload an image or video, set display order and active status. The homepage will show the updated slide dynamically.";
            }

            if (this.hasAny(text, ["permission", "role", "access", "menu", "module"])) {
                return "Open Admin > Role Menu Permission. Select a role, enable the allowed menus/modules, then assign CanView, CanCreate, CanEdit and CanDelete as needed.";
            }

            if (this.hasAny(text, ["contact", "suggestion", "volunteer", "citizen", "message"])) {
                return "Open Admin > Citizen Connect to review Contact Us, Become Volunteer and Send Suggestion entries submitted from the website.";
            }

            if (this.hasAny(text, ["event", "task", "jan sampark", "expense", "war room", "campaign"])) {
                return "Use Admin > Election War Room for events, booths, tasks, volunteers, expenses and Jan Sampark records. Access depends on the permissions given to your role.";
            }

            if (this.hasAny(text, ["backup", "download", "database", "drive", "gdrive"])) {
                return "Use Admin > Voter Backup Settings to manage automatic voter backups and download backup files. New voter creation also generates backup data.";
            }

            return "I can help with Voter Module, Hero Slider, Election War Room, Role Permissions, Citizen Connect, backups, menus and website content. Try asking: how do I add voters, update hero video, or give module permission?";
        },

        getPublicAnswer: function (text) {
            if (this.hasAny(text, ["contact", "office", "phone", "email"])) {
                return "Use the Contact Us section on the website to send your message to the office. The admin team can review it from the Citizen Connect panel.";
            }

            if (this.hasAny(text, ["volunteer", "join", "support"])) {
                return "Open Become Volunteer from the website links and submit your details. The admin team will see your request in Citizen Connect.";
            }

            if (this.hasAny(text, ["suggestion", "feedback", "complaint"])) {
                return "Use Send Suggestion to share feedback, local issues or development suggestions. Please include clear contact and location details.";
            }

            if (this.hasAny(text, ["search", "news", "update"])) {
                return "Click the Search option in the top navigation, enter a keyword, choose dates if needed, and submit to find news, events and public updates.";
            }

            if (this.hasAny(text, ["event", "gallery", "photo", "video", "meeting"])) {
                return "Use the Events, Gallery and Videos sections on the website to view public meetings, photos, speeches and latest program updates.";
            }

            return "I can guide you to Contact Us, Become Volunteer, Send Suggestion, Search, Events, Gallery, Videos and Downloads. Ask me what you want to do on the website.";
        },

        hasAny: function (text, words) {
            for (var index = 0; index < words.length; index += 1) {
                if (text.indexOf(words[index]) !== -1) {
                    return true;
                }
            }

            return false;
        }
    };

    $(function () {
        HelpAgent.init();
    });
})(jQuery);
