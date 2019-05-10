// Hide or show the menu in mobile display
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

// Alert message
$(document).ready(function () {
    message = $("#message").val()
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
})

// Highlight the reserved words
$("#sqlCode").highlightWithinTextarea({
    highlight: [
        {
            highlight: HighLightWords,
            className: "green"
        },
        {
            highlight: ["int", "varchar(100)", "datetime", "primary", "key"],
            className: "blue"
        }
    ]
})

// Look what words are matching with the reserved words
function HighLightWords() {
    words = undefined
    txt = $("#sqlCode").val()

    serverResponse = $.ajax({
        url: "/DBMS/MatchWords",
        type: "GET",
        data: { text : txt },
        async: false,
        dataType: "json"
    }).responseJSON

    if (serverResponse) {
        words = serverResponse["words"]
    }

    return words
}

// Detect the name of the table at load view
$(document).ready(function () {
    if ($("body").is(".Tables")) {
        name = $("#tableName").val()
        MakeBodyTable(name)
    }
})

// Detect the name of the table in the change of option
$("#tableName").on("change", function () {
    name = $(this).val()
    MakeBodyTable(name)
})

// Make the body of the table
function MakeBodyTable(name) {
    $.get("/DBMS/TableColumns", { name: name }, function (data) {
        names = data.names
        types = data.types
        $("#bodyTable").empty()
        for (i = 0; i < names.length; i++) {
            $row = $('<tr><td class="has-text-centered">' + names[i] + '</td><td class="has-text-centered">' + types[i] + '</td></tr>')
            $row.appendTo($("#bodyTable"))
        }
    })
}
