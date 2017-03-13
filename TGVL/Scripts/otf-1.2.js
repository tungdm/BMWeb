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
    console.log("SelectedSuccess");
    var table = $("#productList").children().clone(true, true);

    $("#clone").html(table);

    $("#clone").find("#my-orders-table").attr('id', 'clonetable');
    $("#clone").find(".qty").attr('name', 'quantity');
    $("#clone").find(".qty").removeClass("qty");

    $("#listProduct").empty();
    $("#searchString").val('');
    $('#select').hide();

    $('#searchProduct').modal('toggle');
}

//Create request
function checkValidate(data, status, xhr) {
    if (data.Success == "Fail") {
        var message = "";
        if (data.ErrorType == "RequireProduct") {
            message = "<span class='text-danger field-validation-error'><span>" + data.Message + "</span></span>";
            console.log(message);
            $("#productList").html(message);
        } else if (data.ErrorType == "GreaterThanMin") {
            message = "<span class='text-danger field-validation-error'><span>Must greater than " + addDot(data.Min) + " &#x20AB; (Now: " + addDot(data.Sum) + " &#x20AB;)</span></span>";
            console.log(message);
            $("#errorTotal").html(message);
            $("#min").html(data.Min);
        } 
    }
}

//Tạo reply - supplier
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
            var form = $("#form0");
            $.validator.unobtrusive.parse(form);

        }
        $('#replyModal').modal('show');
    });
}


//View Detail Của Reply - All user
function viewDetails(replyId) {
    var options = {
        url: '/Reply/Details',
        type: 'GET',
        data: { replyId: replyId, type: "Details" }
    };

    $.ajax(options).done(function (data) {
        var $target = $('#replyInfo');
        $target.html(data);
        $('#replyModal').modal('show');
    });
}


function select(replyId) {
    var url = "/Request/Confirm/" + replyId;
    
    window.location.href = url;
}

//Edit Reply - Supplier
function edit(replyId) {
    var options = {
        url: '/Reply/Details',
        type: 'GET',
        data: {replyId:replyId, type:"Edit"}
    };

    $.ajax(options).done(function (data) {
        var $target = $('#replyInfo');
        $target.html(data);

        
        $('#submit-btn-edit').show();

        var form = $("#form0");
        $.validator.unobtrusive.parse(form);

        $('#replyModal').modal('show');
    });
}

function addDot(nStr) {
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + '.' + '$2');
    }
    return x1 + x2;
}

