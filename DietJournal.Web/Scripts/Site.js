$(document).bind("mobileinit", function () {
    //$.mobile.ajaxEnabled = false;

});

function showError(form, errorMessage) {
    var container = $(form).find("[data-valmsg-summary=true]");

    if (container && container.length > 0) {
        list = container.find("ul");

        if (list && list.length) {
            list.empty();
            container.addClass("validation-summary-errors").removeClass("validation-summary-valid");

            $("<li />").html(data.ErroMessage).appendTo(list);
        }
    }
    else {
        $(form).prepend("<div class='validation-summary-errors'>" + errorMessage + "</div>");
    }
}