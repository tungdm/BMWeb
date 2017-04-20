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
function checkValidate(data) {
    var type = typeof data;
    console.log(type);
    if (type != "object") {
        data = jQuery.parseJSON(data);
    }
    

    //console.log(data.Success);
    if (data.Success == "Fail") {
        var message = "";
        $('#mydiv').show();
        if (data.ErrorType == "RequireProduct") {
            message = "<span id='error' tabindex='1' class='text-danger field-validation-error'><span>" + data.Message + "</span></span>";
            console.log(message);
            //$("#productList").html(message);
            $('#errorTotal').focus();
        } else if (data.ErrorType == "GreaterThanMin") {
            message = "<span class='text-danger field-validation-error'><span>Giá thầu phải lớn hơn " + addDot(data.Min) + " &#x20AB; (Giá hiện tại: " + addDot(data.Sum) + " &#x20AB;)</span></span>";
            console.log(message);
            $("#errorTotal").html(message);
            $("#min").html(data.Min);
            $('#errorTotal').focus();
        } 

    } else
    {
        //swal("Tạo thành công", "Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!", "success")
        swal({
            title: 'Tạo thành công',
            text: 'Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!!',
            type: 'success',
            showCancelButton: false,
            confirmButtonClass: "btn-success"
        }).then(function (isConfirm) {
            if (isConfirm) {
                //var url = "/Request/Details/" + data.RequestId;
                var url = "/Request/Details/" + data.SeoUrl;
                window.location.href = url;
            }
        });        
    }
}

function isSuccess(data, status, xhr) {
    if (data.Success == "Success") {
        swal({
            title: "Đặt hàng thành công",
            text: "Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!!",
            type: "success",
            showCancelButton: false,
            confirmButtonClass: "btn-success",
            confirmButtonText: "Đồng ý",
        })
    };
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
            $(".modal-title").html("Tạo đơn thầu mới");
            
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
        $(".modal-title").html("Nội dung đơn thầu");
        var $target = $('#replyInfo');
        $target.html(data);
        $('#replyModal').modal('show');
    });
}


function select(replyId) {
    var url = "/Request/Confirm/" + replyId;
    
    window.location.href = url;
}

//Ban
function ban(replyId) {
    var options = {
        url: '/Request/Ban',
        type: 'GET',
        data: { replyId: replyId }
    };

    $.ajax(options).done(function (data) {
        if (data.Message == "Success") {
            swal({
                title: "Bạn có chắc chắn muốn chặn nhà cung cấp này?",
                //text: "Nhà cung cấp sẽ không thể tiếp tục đặt thầu cho yêu cầu này của bạn",
                type: "warning",
                showCancelButton: true,
                confirmButtonText: "ĐỒNG Ý",
                cancelButtonText: 'HỦY',
                confirmButtonColor: '#d33'
            }).then(function () {
                var options = {
                    url: '/Reply/Ban',
                    type: 'GET',
                    data: { replyId: replyId }
                };

                $.ajax(options).done(function (data) {
                    console.log("ban success");
                });
            }, function (dismiss) {
                    if (dismiss === 'cancel') {
                        console.log("cancel");
                    }
               }
            )
        }
    });

}


//Edit Reply - Supplier
function edit(replyId) {
    if (typeof myTimeOut !== "undefined") {
        console.log("myTimeOut existed");
        clearTimeout(myTimeOut);
    }
    
    var options = {
        url: '/Reply/Details',
        type: 'GET',
        data: {replyId:replyId, type:"Edit"}
    };

    $.ajax(options).done(function (data) {
        var $target = $('#replyInfo');
        $target.html(data);
        $(".modal-title").html("Cập nhật đơn thầu");
        $('#submit-btn-edit').show();

        var form = $("#form0");
        $.validator.unobtrusive.parse(form);

        $('#replyModal').modal('show');
    });
}

//auto bid - supplier
function autobid(replyId) {
    if (typeof myTimeOut !== "undefined") {
        console.log("myTimeOut existed");
        clearTimeout(myTimeOut);
    }

    var options = {
        url: '/Reply/AutoBid',
        type: 'GET',
        data: { replyId: replyId }
    };

    $.ajax(options).done(function (data) {
        var $target = $('#autoBidInfo');
        $target.html(data);

        var form = $("#form0");
        $.validator.unobtrusive.parse(form);

        //$('#replyModal').modal('show');
        $('#autoBidModal').modal({
            backdrop: 'static',
            keyboard: false
        })
    });
}

function retract(replyId) {
    swal({
        title: "Bạn có chắc chắn muốn rút thầu?",
        text: "Bạn sẽ không thể đặt thầu này nữa ",
        type: "warning",
        showCancelButton: true,
        confirmButtonText: "ĐỒNG Ý",
        cancelButtonText: 'HỦY',
        confirmButtonColor: '#d33'
    }).then(function () {
        var options = {
            url: '/Reply/Retract',
            type: 'GET',
            data: { replyId: replyId }
        };

        $.ajax(options).done(function (data) {
            if (data.Success == "Success") {
                $("#bidtable").remove();

                swal("Hoàn tất", "Rút thầu thành công", "success");
            } else {
                swal("Lỗi", data.Message, "error");
            }
            
        });
    },  function (dismiss) {
            if (dismiss === 'cancel') {
                console.log("cancel");
            }
        }
    )   
}

function cancelRequest(requestId) {
    var options = {
        url: '/Request/Cancel',
        type: 'GET',
        data: { requestId: requestId }
    };

    $.ajax(options).done(function (data) {
        console.log("cancelRequest");

        var $target = $('#reasonText');
        $target.html(data);

        var form = $("#form0");
        $.validator.unobtrusive.parse(form);

        $('#cancelRequestModal').modal('show');
    });
}

function updateCart(data, status, xhr) {
    if (data.Success == "Success") {
        $('#cartCount').html(data.Count);
        if (data.Type == "Muangay") {
            $('#mua_ngay').modal('toggle');
        }
        swal({
            title: 'Thêm vào giỏ hàng thành công',
            type: 'success',
            timer: 5000,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Xem giỏ hàng và thanh toán'
        }).then(
            function () {
                var url = "/Home/ShoppingCart";
                window.location.href = url;
            },
            
            function (dismiss) {
                if (dismiss === 'timer') {
                    console.log('I was closed by the timer')
                }
            }
        )
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
            $('#cartCount').html(data.Count);

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
    $("form[id='form1']").submit();

    //var options = {
    //    url: '/Request/Create',
    //    data: { searchString: searchString },
    //    type: 'GET',
    //};
    //$.ajax(options).done(function (data) {
    //    var $target = $('#listProduct');
    //    $target.html(data);
    //});
}

function viewProductDetails(sysProductId, sysProductName) {
    var options = {
        url: '/Home/CreateMapFromAjax',
        data: { sysProductId: sysProductId, sysProductName: sysProductName },
        type: 'GET',
    };

    $.ajax(options).done(function (data) {
        if (data.Message == "Success") {
            var url = "/Home/ViewDetail/" + data.SeoUrl;
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

//function viewOnMap(lat, lng) {
//    var map = "<iframe width='870' height='500' frameborder='0' scrolling='no' marginheight='0' marginwidth='0' src='https://maps.google.com/maps?q=" + lat + "," + lng + "&hl=es;z=14&amp;output=embed'></iframe>";
//    console.log(map);

//    var $target = $('#mapsInfo');
//    $target.html(map);

//    $('#maps').modal('show');
//}

function viewOnMap(supplierId, productId) {
    var options = {
        url: '/Home/GetAddresses',
        data: { supplierId: supplierId, productId: productId },
        type: 'GET',
    };

    $.ajax(options).done(function (data) {
        var $target = $('#mapsInfo');
        $target.html(data);
        
        $('#maps').modal('show');

    });
}

function createOrderSuccess(data) {
    if (data.Success == "Success") {
        console.log("Checkout success");
        swal({
            title: 'Đơn hàng của bạn đã được đặt thành công',
            text: "Cửa hàng sẽ liên hệ thông báo giao hàng với bạn",
            type: 'success',
            showCancelButton: true,
            confirmButtonText: 'Xem chi tiết đơn hàng',
            confirmButtonClass: 'btn btn-success',
            cancelButtonClass: 'btn btn-default',
            cancelButtonText: 'Quay lại trang chủ',
        }).then(function () {
            //var url = "/Order/Details/" + data.OrderId;
            var url = "/Order/Index/";
            window.location.href = url;
        }, function (dismiss) {
            var url = "/Home/Index";
            window.location.href = url;
        })
    }
    
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

function clock(callback) {
    $(".count-down").each(function () {
        console.log("callback:" + callback);
        var countDownDate = new Date($(this).data("time")).getTime();
        var dealId = $(this).data("deal");

        var $target = $(this);
        var x = setInterval(function () {
            var now = new Date().getTime();
            var distance = countDownDate - now;
            var text = "";
            if (distance < 0) {
                clearInterval(x);
                text = "EXPIRED"
                $target.html(text);
                if (callback) {
                    //var dealId = $(this).data("deal");
                    console.log("dealId=" + dealId);
                    dealExpired(dealId);
                }
            } else {
                var days = Math.floor(distance / (1000 * 60 * 60 * 24));
                var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
                var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
                var seconds = Math.floor((distance % (1000 * 60)) / 1000);
                text = "Còn " + days + " ngày " + hours + ":" + ('0' + minutes).slice(-2) + ":" + ('0' + seconds).slice(-2);

                $target.html(text);
            }
        }, 1000);
    });
}
