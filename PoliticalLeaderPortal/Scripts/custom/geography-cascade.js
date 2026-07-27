(function ($) {
    "use strict";
    function initialise(form) {
        var $form = $(form), endpoint = $form.data("geography-url");
        if (!endpoint) return;
        function select(id) { return $form.find("#" + id); }
        function clear(id, text) {
            var $select = select(id);
            if ($select.length) $select.empty().append($("<option/>", { value: "", text: text || "Select" }));
        }
        function clearMany(ids) { $.each(ids, function (_, id) { clear(id); }); }
        function load(id, type, parentId, parentType, text) {
            var $select = select(id);
            if (!$select.length) return;
            clear(id, text);
            if (!parentId) return;
            $select.prop("disabled", true);
            $.getJSON(endpoint, { type: type, entityType: type, parentId: parentId, parentType: parentType })
                .done(function (items) {
                    $.each(items || [], function (_, item) {
                        $select.append($("<option/>", { value: item.Value, text: item.Text }));
                    });
                })
                .fail(function () {
                    $select.append($("<option/>", { value: "", text: "Unable to load options", disabled: true }));
                })
                .always(function () { $select.prop("disabled", false); });
        }
        select("StateId").on("change.geography", function () {
            clearMany(["DistrictId", "ParliamentaryConstituencyId", "AssemblyConstituencyId", "TehsilId", "BlockId", "GramPanchayatId", "VillageId", "WardId", "BoothId"]);
            load("DistrictId", "District", this.value, "State", "Select District");
            load("ParliamentaryConstituencyId", "ParliamentaryConstituency", this.value, "State", "Select Parliamentary Constituency");
            load("AssemblyConstituencyId", "AssemblyConstituency", this.value, "State", "Select Assembly Constituency");
        });
        select("ParliamentaryConstituencyId").on("change.geography", function () {
            clearMany(["AssemblyConstituencyId", "WardId", "BoothId"]);
            load("AssemblyConstituencyId", "AssemblyConstituency", this.value, "ParliamentaryConstituency", "Select Assembly Constituency");
        });
        select("DistrictId").on("change.geography", function () {
            clearMany(["TehsilId", "BlockId", "GramPanchayatId", "VillageId"]);
            load("TehsilId", "Tehsil", this.value, "District", "Select Tehsil");
            load("BlockId", "Block", this.value, "District", "Select Block");
        });
        select("TehsilId").on("change.geography", function () {
            if (!select("BlockId").val()) load("VillageId", "Village", this.value, "Tehsil", "Select Village");
        });
        select("BlockId").on("change.geography", function () {
            clearMany(["GramPanchayatId", "VillageId"]);
            load("GramPanchayatId", "GramPanchayat", this.value, "Block", "Select Gram Panchayat");
            load("VillageId", "Village", this.value, "Block", "Select Village");
        });
        select("GramPanchayatId").on("change.geography", function () {
            load("VillageId", "Village", this.value, "GramPanchayat", "Select Village");
        });
        select("AssemblyConstituencyId").on("change.geography", function () {
            clearMany(["WardId", "BoothId"]);
            load("WardId", "Ward", this.value, "AssemblyConstituency", "Select Ward");
            load("BoothId", "Booth", this.value, "AssemblyConstituency", "Select Booth");
        });
    }
    $(function () { $("[data-geography-form]").each(function () { initialise(this); }); });
})(jQuery);
