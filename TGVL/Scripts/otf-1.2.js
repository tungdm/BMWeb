$(function () {

    var submitAutocompleteForm = function (event, ui) {
        var $input = $(this);
        $input.val(ui.item.label);

        var $form = $input.parents("form:first");
        $form.submit();
    };

    var createAutoComplete = function () {
        var $input = $(this);

        var options = {
            source: $input.attr("data-otf-autocomplete"),
            select: submitAutocompleteForm
        };

        $input.autocomplete(options);
    };


    $("input[data-otf-autocomplete]").each(createAutoComplete);

    $('#select').click(function () {
        
        $("form[id='formAjax']").submit();
        
    });
});

function selectedSuccess() {
    var table = $("#productList").children().clone(true, true);

    //console.log("Start");
    //var table = $("#cloneDest").children().clone(true, true);
    
    $("#clone").html(table);
    $("#listProduct").empty();
    $("#searchString").val('');
    $('#select').hide();
    $('#searchProduct').modal('toggle');
}

