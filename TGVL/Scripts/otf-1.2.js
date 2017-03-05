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

//Request/Create
function selectedSuccess() {
    var table = $("#productList").children().clone(true, true);

    
    $("#clone").html(table);
    $("#listProduct").empty();
    $("#searchString").val('');
    $('#select').hide();
    $('#searchProduct').modal('toggle');
}

//Request/Details
//Lấy thông tin của request đổ lên modal
function reply(requestId) {
    var options = {
        url: '/Reply/Create',
        type: 'GET',
        data: 'requestId=' + requestId
    };

    $.ajax(options).done(function (data) {
        
        var $target = $('#replyInfo');

        if (data.Message != null) {
            var messageError = "<p>" + data.Message + "</p>";
            console.log(messageError);
            $target.html(messageError);
        } else {
            $target.html(data);
            $('#submit-btn-reply').show();
            $.validator.unobtrusive.parse($("#form0"));
        }
        $('#replyModal').modal('show');
    });
}


//View Detail Của Bid Reply
function viewDetails(replyId) {
    var options = {
        url: '/Reply/Details',
        type: 'GET',
        data: 'replyId=' + replyId
    };

    //$.ajax(options).done(function (data) {
    //    //console.log("Hello");
    //    var $target = $('#replyInfo');
    //    $target.html(data);
    //    $('#replyModal').modal('show');
    //});
}





