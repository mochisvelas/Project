﻿// Hide or show the menu in mobile display
$(document).ready(function () {
    $(".navbar-burger").click(function () {
        $(".navbar-burger").toggleClass("is-active")
        $(".navbar-menu").toggleClass("is-active")
    })
})

// Change the name of the file
$(document).ready(function () {
    $(".file-input").on("change", function () {
        $("#fileName").text($(this)[0].files[0].name)
    })
})

// Display the form to submit reserved words
$(".newWord").click(function () {
    $("#reservedWordForm").show()
})

// Hide the parent element
$(".delete").click(function () {
    $(this).parent().hide()
})

// Hide the form to submit reserved words
$("#cancel").click(function () {
    $("#reservedWordForm").hide()
})

$(document).ready(function () {
    message = $("#message").val()
    if ($("body").hasClass("ReservedWords")) {
        if (message == "success") {
            $("#notification").show()
            $("#notification").attr("class", "notification is-success")
            $("#text").text("La acción se ejecuto de manera exitosa.")
        } else {
            if (message != "null") {
                $("#notification").show()
                $("#notification").attr("class", "notification is-danger")
                $("#text").text(message)
            }
        }
    }
})