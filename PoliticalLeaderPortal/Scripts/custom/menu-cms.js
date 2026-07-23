/***************************************************************************************************
 * Project        : PoliticalLeaderPortal
 * Module         : Menu Management
 * File           : menu-cms.js
 * Author         : AV Tech Bee Solutions
 *
 * Description
 * -----------
 * Handles all client-side functionality for the Menu CMS.
 *
 * This file is shared by:
 *
 *      1. Menu List
 *      2. Create Menu
 *      3. Edit Menu
 *
 * Technologies
 * ------------
 * - jQuery
 * - Bootstrap 5
 * - SweetAlert2
 *
 * NOTE
 * ----
 * Do NOT write JavaScript inside Razor Views.
 * All Menu CMS JavaScript belongs here.
 *
 ***************************************************************************************************/



/***************************************************************************************************
 * DOCUMENT READY
 *
 * Purpose
 * -------
 * Initializes every Menu CMS feature.
 *
 * Future Modification
 * -------------------
 * Whenever a new feature is added,
 * register it inside initialize().
 *
 ***************************************************************************************************/

$(document).ready(function () {

    initialize();

});



/***************************************************************************************************
 * INITIALIZE
 *
 * Purpose
 * -------
 * Calls every module initialization.
 *
 ***************************************************************************************************/

function initialize() {

    initializeIconPreview();

    initializeSeoPreview();

    initializeLivePreview();

    initializeCharacterCounters();

    initializeSearch();

    initializeDelete();

    initializeToggleStatus();

    initializeRefresh();

}



/***************************************************************************************************
 * COMMON HELPERS
 *
 * Shared helper methods used by multiple modules.
 *
 ***************************************************************************************************/



/***************************************************************************************************
 * isNullOrEmpty()
 *
 * Returns true if string is null or empty.
 *
 ***************************************************************************************************/

function isNullOrEmpty(value) {

    return $.trim(value) === "";

}



/***************************************************************************************************
 * updateStatusIcon()
 *
 * Changes Quality Checklist icon.
 *
 * Used By:
 *
 *      initializeLivePreview()
 *
 ***************************************************************************************************/

function updateStatusIcon(elementId, isValid) {

    var icon =
        isValid
            ? '<i class="fas fa-check text-success me-2"></i>'
            : '<i class="fas fa-times text-danger me-2"></i>';

    var text = $(elementId).text();

    $(elementId).html(icon + text);

}
/***************************************************************************************************
 * MODULE
 * ------
 * ICON PREVIEW
 *
 * Purpose
 * -------
 * Shows live FontAwesome icon preview while typing.
 *
 * Used By
 * -------
 * Create Menu
 * Edit Menu
 *
 * Future Modification
 * -------------------
 * If a FontAwesome Picker or Icon Library is added later,
 * only modify this function.
 *
 ***************************************************************************************************/

function initializeIconPreview() {

    // Stop if control doesn't exist
    if ($("#IconClass").length === 0)
        return;

    // Initial Preview
    updateIconPreview();

    // Live Preview
    $("#IconClass").on("keyup change", function () {

        updateIconPreview();

    });

}



/***************************************************************************************************
 * updateIconPreview()
 *
 * Purpose
 * -------
 * Updates Icon Preview.
 *
 ***************************************************************************************************/

function updateIconPreview() {

    var iconClass = $("#IconClass").val();

    if (isNullOrEmpty(iconClass)) {

        iconClass = "fas fa-bars";

    }

    // Small Icon Preview
    $("#menuIconPreview")
        .attr("class", iconClass);

    // Large Live Preview
    $("#previewIcon")
        .attr("class", iconClass);

}



/***************************************************************************************************
 * MODULE
 * ------
 * SEO PREVIEW
 *
 * Purpose
 * -------
 * Updates Google Search Preview.
 *
 * Used By
 * -------
 * Create Menu
 * Edit Menu
 *
 ***************************************************************************************************/

function initializeSeoPreview() {

    if ($("#PageTitle").length === 0)
        return;

    updateSeoPreview();

    $("#PageTitle,#MetaDescription")
        .on("keyup change", function () {

            updateSeoPreview();

        });

}



/***************************************************************************************************
 * updateSeoPreview()
 *
 * Purpose
 * -------
 * Updates Google Search Preview.
 *
 ***************************************************************************************************/

function updateSeoPreview() {

    var title = $("#PageTitle").val();

    var description = $("#MetaDescription").val();


    if (isNullOrEmpty(title)) {

        title = "Your Page Title";

    }

    if (isNullOrEmpty(description)) {

        description = "Your Meta Description will appear here...";

    }

    $("#seoTitlePreview")
        .text(title);

    $("#seoDescriptionPreview")
        .text(description);

}



/***************************************************************************************************
 * MODULE
 * ------
 * CHARACTER COUNTERS
 *
 * Purpose
 * -------
 * Shows live character count.
 *
 * Future Modification
 * -------------------
 * If maximum character limits change,
 * modify only this function.
 *
 ***************************************************************************************************/

function initializeCharacterCounters() {

    if ($("#PageTitle").length === 0)
        return;

    updateCharacterCounters();

    $("#PageTitle,#MetaDescription")
        .on("keyup change", function () {

            updateCharacterCounters();

        });

}



/***************************************************************************************************
 * updateCharacterCounters()
 *
 * Purpose
 * -------
 * Updates all character counters.
 *
 ***************************************************************************************************/

function updateCharacterCounters() {

    var titleLength =
        $("#PageTitle").val().length;

    var metaLength =
        $("#MetaDescription").val().length;

    $("#pageTitleCount")
        .text(titleLength);

    $("#metaDescriptionCount")
        .text(metaLength);

}
/***************************************************************************************************
 * MODULE
 * ------
 * LIVE MENU PREVIEW
 *
 * Purpose
 * -------
 * Displays a live preview of the Menu while the user types.
 *
 * Updates
 * -------
 * ✔ Menu Name
 * ✔ Route Preview
 * ✔ Icon Preview
 * ✔ Quality Checklist
 *
 * Used By
 * -------
 * Create Menu
 * Edit Menu
 *
 * Future Modification
 * -------------------
 * If additional preview fields are added
 * (Badge, Parent Menu, Menu Type etc.)
 * modify only updateLivePreview().
 *
 ***************************************************************************************************/

function initializeLivePreview() {

    // Exit if preview controls do not exist
    if ($("#previewMenuName").length === 0)
        return;

    // Initial Preview
    updateLivePreview();

    // Listen every important field
    $("#MenuName," +
        "#ControllerName," +
        "#ActionName," +
        "#AreaName," +
        "#IconClass")
        .on("keyup change", function () {

            updateLivePreview();

        });

}



/***************************************************************************************************
 * FUNCTION
 * --------
 * updateLivePreview()
 *
 * Purpose
 * -------
 * Refreshes all live preview controls.
 *
 ***************************************************************************************************/

function updateLivePreview() {

    //----------------------------------------
    // Read Values
    //----------------------------------------

    var menuName = $("#MenuName").val();

    var controller = $("#ControllerName").val();

    var action = $("#ActionName").val();

    var area = $("#AreaName").val();

    var icon = $("#IconClass").val();


    //----------------------------------------
    // Default Values
    //----------------------------------------

    if (isNullOrEmpty(menuName))
        menuName = "Menu Name";

    if (isNullOrEmpty(controller))
        controller = "Controller";

    if (isNullOrEmpty(action))
        action = "Action";

    if (isNullOrEmpty(icon))
        icon = "fas fa-bars";


    //----------------------------------------
    // Menu Preview
    //----------------------------------------

    $("#previewMenuName")
        .text(menuName);


    //----------------------------------------
    // Route Preview
    //----------------------------------------

    var route = "";

    if (!isNullOrEmpty(area)) {

        route += area + "/";

    }

    route += controller + "/" + action;

    $("#previewRoute")
        .text(route);


    //----------------------------------------
    // Icon Preview
    //----------------------------------------

    $("#previewIcon")
        .attr("class", icon);


    //----------------------------------------
    // Quality Checklist
    //----------------------------------------

    validateQualityChecklist();

}



/***************************************************************************************************
 * MODULE
 * ------
 * QUALITY CHECKLIST
 *
 * Purpose
 * -------
 * Shows required field completion status.
 *
 * Future Modification
 * -------------------
 * If more validation items are added,
 * update this method only.
 *
 ***************************************************************************************************/

function validateQualityChecklist() {

    updateChecklistItem(
        "#checkMenuName",
        $("#MenuName").val()
    );

    updateChecklistItem(
        "#checkController",
        $("#ControllerName").val()
    );

    updateChecklistItem(
        "#checkAction",
        $("#ActionName").val()
    );

    updateChecklistItem(
        "#checkIcon",
        $("#IconClass").val()
    );

}



/***************************************************************************************************
 * FUNCTION
 * --------
 * updateChecklistItem()
 *
 * Purpose
 * -------
 * Updates a single Quality Checklist row.
 *
 ***************************************************************************************************/

function updateChecklistItem(elementId, value) {

    var element = $(elementId);

    //----------------------------------------
    // Store original label only once
    //----------------------------------------

    if (element.attr("data-label") === undefined) {

        element.attr(
            "data-label",
            $.trim(element.text())
        );

    }

    var label = element.attr("data-label");

    //----------------------------------------
    // Valid
    //----------------------------------------

    if (!isNullOrEmpty(value)) {

        element.html(

            '<i class="fas fa-check-circle text-success me-2"></i>' +

            label

        );

    }

    //----------------------------------------
    // Invalid
    //----------------------------------------

    else {

        element.html(

            '<i class="fas fa-times-circle text-danger me-2"></i>' +

            label

        );

    }

}

/***************************************************************************************************
* MODULE
* ------
* LIVE SEARCH
*
* Purpose
* -------
* Filters Menu Grid while typing.
*
* Used By
* -------
* Menu Index
*
* Future Modification
* -------------------
* If server-side search is introduced,
* modify only this module.
*
***************************************************************************************************/

function initializeSearch() {

    if ($("#txtSearch").length === 0)
        return;

    $("#txtSearch").on("keyup", function () {

        var keyword = $.trim($(this).val()).toLowerCase();

        filterMenuGrid(keyword);

    });

}



/***************************************************************************************************
 * FUNCTION
 * --------
 * filterMenuGrid()
 *
 * Purpose
 * -------
 * Filters menu rows without reloading page.
 *
 ***************************************************************************************************/

function filterMenuGrid(keyword) {

    $("#menuListContainer tbody tr").each(function () {

        var row = $(this);

        var text = row.text().toLowerCase();

        if (text.indexOf(keyword) > -1) {

            row.show();

        }
        else {

            row.hide();

        }

    });

}



/***************************************************************************************************
 * MODULE
 * ------
 * REFRESH BUTTON
 *
 * Purpose
 * -------
 * Reloads Menu Grid.
 *
 ***************************************************************************************************/

function initializeRefresh() {

    if ($("#btnRefresh").length === 0)
        return;

    $("#btnRefresh").click(function () {

        reloadMenuGrid();

    });

}



/***************************************************************************************************
 * FUNCTION
 * --------
 * reloadMenuGrid()
 *
 * Purpose
 * -------
 * Reload Menu Grid using AJAX.
 *
 * Future Modification
 * -------------------
 * If filters are added,
 * pass them inside AJAX request.
 *
 ***************************************************************************************************/

function reloadMenuGrid() {

    showLoading();

    $.ajax({

        url: window.MenuCMS.menuListUrl,

        type: "GET",

        cache: false,

        success: function (response) {

            $("#menuListContainer").html(response);

        },

        error: function () {

            showErrorMessage(
                "Unable to reload menu list."
            );

        },

        complete: function () {

            hideLoading();

        }

    });

}



/***************************************************************************************************
 * MODULE
 * ------
 * LOADING PANEL
 *
 * Purpose
 * -------
 * Shows loading overlay during AJAX requests.
 *
 ***************************************************************************************************/

function showLoading() {

    if ($("#menuLoadingOverlay").length === 0) {

        $("body").append(

            '<div id="menuLoadingOverlay">' +

            '<div class="menu-spinner"></div>' +

            '</div>'

        );

    }

}



/***************************************************************************************************
 * FUNCTION
 * --------
 * hideLoading()
 *
 * Purpose
 * -------
 * Removes loading overlay.
 *
 ***************************************************************************************************/

function hideLoading() {

    $("#menuLoadingOverlay").remove();

}



/***************************************************************************************************
 * MODULE
 * ------
 * COMMON ERROR MESSAGE
 *
 * Purpose
 * -------
 * Displays common error popup.
 *
 * Future Modification
 * -------------------
 * Replace with Toast Notification if required.
 *
 ***************************************************************************************************/

function showErrorMessage(message) {

    if (typeof Swal !== "undefined") {

        Swal.fire({

            icon: "error",

            title: "Error",

            text: message,

            confirmButtonColor: "#0d6efd"

        });

    }
    else {

        alert(message);

    }

}



/***************************************************************************************************
 * MODULE
 * ------
 * COMMON SUCCESS MESSAGE
 *
 * Purpose
 * -------
 * Displays success popup.
 *
 ***************************************************************************************************/

function showSuccessMessage(message) {

    if (typeof Swal !== "undefined") {

        Swal.fire({

            icon: "success",

            title: "Success",

            text: message,

            confirmButtonColor: "#198754"

        });

    }

}
/***************************************************************************************************
 * MODULE
 * ------
 * DELETE MENU
 *
 * Purpose
 * -------
 * Handles Menu Delete using AJAX.
 *
 * Used By
 * -------
 * Index.cshtml
 *
 * Future Modification
 * -------------------
 * If Soft Delete is introduced,
 * modify only deleteMenu().
 *
 ***************************************************************************************************/

function initializeDelete() {

    $(document).on("click", ".btnDelete", function () {

        var menuId = $(this).data("id");

        deleteMenu(menuId);

    });

}



/***************************************************************************************************
 * FUNCTION
 * --------
 * deleteMenu()
 *
 * Purpose
 * -------
 * Deletes selected Menu.
 *
 ***************************************************************************************************/

function deleteMenu(menuId) {

    Swal.fire({

        title: "Delete Menu?",

        text: "This action cannot be undone.",

        icon: "warning",

        showCancelButton: true,

        confirmButtonColor: "#dc3545",

        cancelButtonColor: "#6c757d",

        confirmButtonText: "Yes, Delete",

        cancelButtonText: "Cancel"

    }).then(function (result) {

        if (!result.isConfirmed)
            return;

        showLoading();

        $.ajax({

            url: window.MenuCMS.deleteUrl,

            type: "POST",

            data: {

                id: menuId

            },

            success: function (response) {

                hideLoading();

                if (response.success) {

                    showSuccessMessage(response.message);

                    reloadMenuGrid();

                }
                else {

                    showErrorMessage(response.message);

                }

            },

            error: function () {

                hideLoading();

                showErrorMessage("Unable to delete menu.");

            }

        });

    });

}



/***************************************************************************************************
 * MODULE
 * ------
 * TOGGLE MENU STATUS
 *
 * Purpose
 * -------
 * Activates / Deactivates Menu.
 *
 ***************************************************************************************************/

function initializeToggleStatus() {

    $(document).on("click", ".btnToggle", function () {

        var menuId = $(this).data("id");

        toggleMenuStatus(menuId);

    });

}



/***************************************************************************************************
 * FUNCTION
 * --------
 * toggleMenuStatus()
 *
 * Purpose
 * -------
 * Toggles Active / Inactive Status.
 *
 ***************************************************************************************************/

function toggleMenuStatus(menuId) {

    showLoading();

    $.ajax({

        url: window.MenuCMS.toggleUrl,

        type: "POST",

        data: {

            id: menuId

        },

        success: function (response) {

            hideLoading();

            if (response.success) {

                reloadMenuGrid();

                showSuccessMessage(response.message);

            }
            else {

                showErrorMessage(response.message);

            }

        },

        error: function () {

            hideLoading();

            showErrorMessage("Unable to update menu status.");

        }

    });

}



/***************************************************************************************************
 * MODULE
 * ------
 * COMMON AJAX POST
 *
 * Purpose
 * -------
 * Reusable AJAX POST helper.
 *
 * Future Modification
 * -------------------
 * If AntiForgery Token is required globally,
 * update this function only.
 *
 ***************************************************************************************************/

function ajaxPost(url, data, successCallback) {

    $.ajax({

        url: url,

        type: "POST",

        data: data,

        success: successCallback,

        error: function () {

            hideLoading();

            showErrorMessage("Unexpected server error.");

        }

    });

}



/***************************************************************************************************
 * MODULE
 * ------
 * RELOAD GRID AFTER CRUD
 *
 * Purpose
 * -------
 * Keeps Menu List updated after
 * Create / Edit / Delete / Toggle.
 *
 ***************************************************************************************************/

function refreshGridAfterAction() {

    reloadMenuGrid();

}



/***************************************************************************************************
 * MODULE
 * ------
 * FUTURE FEATURES
 *
 * Reserved For
 * ------------
 *
 * ✔ Drag & Drop Ordering
 *
 * ✔ Tree View
 *
 * ✔ Mega Menu Builder
 *
 * ✔ Permission Based Menu
 *
 * ✔ Role Based Menu
 *
 * ✔ Import / Export
 *
 * ✔ Bulk Delete
 *
 * ✔ Bulk Active / Inactive
 *
 * ✔ Menu Analytics
 *
 * ✔ Click Tracking
 *
 ***************************************************************************************************/



/***************************************************************************************************
 *
 * END OF FILE
 *
 * menu-cms.js
 *
 ***************************************************************************************************/