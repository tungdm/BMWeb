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
            message = "<span class='text-danger field-validation-error'><span>Giá thầu phải lớn hơn " + addDot(data.Min) + " &#x20AB; (Giá hiện tại: " + addDot(data.Sum) + " &#x20AB;)</span></span>";
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

function updateQuantitySession(newQty, type, id) {
    var options = {
        url: '/Home/UpdateQuantity',
        type: 'GET',
        data: { type: type, id: id, newQty: newQty }
    };
    $.ajax(options).done(function (data) {

    });
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

            var rowCount = $('#shopping-cart-table > tbody > tr').length;
            if (rowCount == 0) {
                $('#checkout').remove();
            }

            var sum = 0.0;

            $('#shopping-cart-table > tbody  > tr').each(function () {
                var qty = $(this).find('.qty').val();
                if (qty == null) {
                    qty = $(this).find('.qtyBid').text();
                }
                var price = $(this).find('.price').text();
                var price2 = Number(price.replace(/[^0-9]+/g, ""));

                console.log("qty:" + qty + ", price:" + price + ", price2:" + price2);

                var amount = qty * price2;
                var miniTotal = addDot(amount);
                $(this).find('.minitotal').html("<strong>" + miniTotal + " &#x20AB;</strong>");
                sum += amount;
            });

            console.log(sum);

            $('.total').html(addDot(sum) + " &#x20AB;");
        }

    });
}

function updateDeliveryAddress(data, status, xhr) {
    $("#deliveryAddress").html(data);
    if ($('#messageModal').hasClass('in')) {
        $('#messageModal').modal('toggle');
    }
}



function updateExistedAddress() {
    var options = {
        url: '/Home/UpdateExistedAddress',
        data : {type : "update"},
        type: 'GET',
    };

    $.ajax(options).done(function (data) {
        $("#message").html(data);
        var form = $("#form1");
        $.validator.unobtrusive.parse(form);
        $('#messageModal').modal('show');
    });
}

function gotToCheckOut() {
    var url = "/Home/CheckOut";

    window.location.href = url;
}

function review(orderId) {
    console.log("review");
    var options = {
        url: '/Order/Review',
        data: { id: orderId },
        type: 'GET',
    };

    $.ajax(options).done(function (data) {
        var $target = $('#reviewForm');
        $target.html(data);

        var form = $("#form0");
        $.validator.unobtrusive.parse(form);

        $('#review').modal('show');
    });
}

function updateReview(data, status, xhr) {
    if (data.Message == "Success") {
        $("#review-btn").remove();
        $('#review').modal('toggle');
    }
    
}

function searchSuggest(searchString) {
    $("#searchString").val(searchString);
    var options = {
        url: '/Request/Create',
        data: { searchString: searchString },
        type: 'GET',
    };
    $.ajax(options).done(function (data) {
        var $target = $('#listProduct');
        $target.html(data);
    });
}

function viewProductDetails(sysProductId) {
    var options = {
        url: '/Home/CreateMapFromAjax',
        data: { sysProductId: sysProductId },
        type: 'GET',
    };

    $.ajax(options).done(function (data) {
        if (data.Message == "Success") {
            var url = "/Home/ViewDetail/" + data.SysProductId;

            window.location.href = url;
        }
        
    });
}

function datmua(productId) {
    var options = {
        url: '/Home/Muangay',
        data: { productId: productId },
        type: 'GET',
    };
    $.ajax(options).done(function (data) {
        var $target = $('#bodyinfo');
        $target.html(data);

        var form = $("#form0");
        $.validator.unobtrusive.parse(form);

        $('#mua_ngay').modal('show');
        
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

if (window.location.hash && window.location.hash == '#_=_') {
    if (window.history && history.pushState) {
        window.history.pushState("", document.title, window.location.pathname);
    } else {
        // Prevent scrolling by storing the page's current scroll offset
        var scroll = {
            top: document.body.scrollTop,
            left: document.body.scrollLeft
        };
        window.location.hash = '';
        // Restore the scroll offset, should be flicker free
        document.body.scrollTop = scroll.top;
        document.body.scrollLeft = scroll.left;
    }
}