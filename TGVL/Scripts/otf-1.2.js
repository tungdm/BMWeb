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
function selectedSuccess(data) {
    if (data.Success == "Nothing") {
        console.log(data);
    } else {
        console.log("SelectedSuccess");
        var table = $("#productList").children().clone(true, true);

        $("#clone").html(table);

        $("#clone").find("#my-orders-table").attr('id', 'clonetable');
        $("#clone").find(".qty").attr('name', 'quantity');
        $("#clone").find(".qty").removeClass("qty");

        $("#listProduct").empty();
        $("#searchString").val('');
        $('#select').hide();


        $('#mydiv').show();
        $('#searchProduct').modal('toggle');
    }  
}

//Create request
function checkValidate(data, status, xhr) {
    if (data.Success == "Fail") {
        var message = "";
        $('#mydiv').show();
        if (data.ErrorType == "RequireProduct") {
            message = "<span id='error' tabindex='1' class='text-danger field-validation-error'><span>" + data.Message + "</span></span>";
            console.log(message);
            $("#productList").html(message);
            $('#error').focus();
        } else if (data.ErrorType == "GreaterThanMin") {
            message = "<span class='text-danger field-validation-error'><span>Must greater than " + addDot(data.Min) + " &#x20AB; (Now: " + addDot(data.Sum) + " &#x20AB;)</span></span>";
            console.log(message);
            $("#errorTotal").html(message);
            $("#min").html(data.Min);
            $('#errorTotal').focus();
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

function updateCart(data, status, xhr) {
    if (data.Success == "Success") {
        $('#cartCount').html(data.Count);
    }
}

function removefromcart(type, id) {
    var options = {
        url: '/Home/RemoveFromCart',
        type: 'GET',
        data: { type: type, id: id }
    };

    $.ajax(options).done(function (data) {
        if (data.Success == "Success") {
            var removeElement = "#shopping-cart-table tr#" + data.RemoveElement;
            console.log(data.RemoveElement);
            
            $(removeElement).remove();
            var sum = 0.0;

            $('#shopping-cart-table > tbody  > tr').each(function () {
                var qty = $(this).find('.qty').val();
                if (qty == null) {
                    qty = $(this).find('.qtyBid').text();
                }
                var price = $(this).find('.price').text();
                var price2 = Number(price.replace(/[^0-9]+/g, ""));

                console.log("qty:" + qty + ", price:" + price + ", price2:" + price2);

                var amount = (qty * price2);
                var miniTotal = addDot(amount);
                $(this).find('.minitotal').html("<strong>" + miniTotal + " &#x20AB;</strong>");
                sum += amount;
            });

            console.log(sum);

            $('.total').html(addDot(sum) + " &#x20AB;");
        }

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

