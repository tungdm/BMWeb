// HIDE NOTIFICATIONS WHEN CLICKED ANYWHERE ON THE PAGE.
$(document).click(function () {
    $('#notifications').hide();
});

// Click on notification icon for show notification
$('#noti_Button').click(function (e) {
    e.stopPropagation();
    //$('.noti-content').show();



    $('#notifications').fadeToggle('fast', 'linear', function () {
        $('#notiContent').empty();
        if ($('#notifications').is(':visible')) {
            updateNotification();
        }
    });

    //$('#noti_Counter').fadeOut('slow');
    $('#noti_Counter').fadeOut(500, function () {
        $(this).empty();
    });


});


// update notification
function updateNotification() {
   
    $.ajax({
        type: 'GET',
        url: '/Home/GetNotificationReplies',
        success: function (response) {
            if (response.length === 0) {
                $('#notiContent').append($('<li>No data available</li>'));
            }

            count = $("#notiContent li").length;
            if (count !== response.length) {
                $('#notiContent').empty();
            
                $.each(response, function (index, value) {
                    var redirectUrl = "/Request/Details/" + value.RequestId + "#reply_" + value.ReplyId;
                    console.log(redirectUrl);
                   
                    $('#notiContent').append($('<li><a href="' + redirectUrl + '">New reply a by : ' + value.Supplier + ' at : ' + new Date(parseInt(value.CreatedDate.substr(6))).format("dd/mm/yyyy HH:MM:ss") + '</a></li>'));
                });
            }

            
        },
        error: function (error) {
            console.log(error);
        }
    });
}

// update notification count -SignalR
function updateNotificationCount(controllerName, actionName, requestId, userName) {
    var count = 0;
    count = parseInt($('div.count').html()) || 0;
    count++;
    console.log("updateNotificationCount:" + count);

    //$('span.count').html(count);

    // ANIMATEDLY DISPLAY THE NOTIFICATION COUNTER.
    $('#noti_Counter')
        .css({ opacity: 0 })
        .text(count)
        .css({ top: '-10px' })
        .animate({ top: '10px', opacity: 1 }, 500)
        .fadeIn('slow');

    //CALL AJAX UPDATE REPLIES
    if (controllerName === 'Request' && actionName === 'Details') {
        //updateReply(requestId);
        
        updateReply(requestId, userName);
    }
}

//Update bid table khi supplier update info - customer
function updateCustomerBidTable(controllerName, actionName, requestId) {
    if (controllerName === 'Request' && actionName === 'Details') {
        
        updateReply(requestId);
        
    }
   
}
//Supplier - update bid table khi rank thay đổi
function updateBidTable(controllerName, actionName, requestId) {
    if (controllerName === 'Request' && actionName === 'Details') {
        supplierUpdateBidReply(requestId);
    }
}

//Supplier/Customer - update reply table khi có người add new/update reply
function updateSupplierReplyTable(controllerName, actionName, requestId, userName) {
    if (controllerName === 'Request' && actionName === 'Details') {
        console.log(userName);
        updateReply(requestId, userName);
    }
}

function supplierUpdateBidReply(requestId) {
    $.ajax({
        type: 'GET',
        url: '/Request/UpdateBidRank',
        data: { requestId: requestId },
        success: function (data) {
            $("#rank").html(data.Rank);
        },
        error: function (error) {
            console.log(error);
        }
        
    });
}


//Realtime update normal reply/bid reply - customer
function updateReply(requestId, userName) {
    console.log(userName);
    $.ajax({
        type: 'GET',
        url: '/Request/UpdateReplies',
        data: { id: requestId, username: userName },
        success: function (data) {
            if (data.ReplyType === "Bid") {
                console.log("Hello from realtime update bid reply");
                var table = '<table class="table">'
                    + '<tbody><tr>'
                    + '<th>Thứ hạng</th>'
                    + '<th>Cửa hàng</th>'
                    + '<th>Tổng cộng</th>'
                    + '<th>Ngày giao hàng</th>'
                    + '<th>Thao tác</th>'
                    + '</tr>';

                $.each(data.BidReplies, function (index, value) {
                    table += '<tr>'
                + '<td>' + value.Rank + '</td>'

                + '<td><img src="/Images/UserAvatar/' + value.Avatar + '" width="100" height="100" alt="avatar"> ' + value.Fullname + '</td>'

                + '<td>' + addDot(value.Total) + ' &#x20AB;</td>'

                + '<td>' + new Date(parseInt(value.DeliveryDate.substr(6))).format("dd/mm/yyyy") + '</td>'

                + '<td><button type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + value.Id + ')">Chi tiết</button> '

                + '<button type="button" class="select btn btn-success btn-sm" style="display:none" onclick="select(' + value.Id + ')">Lựa chọn</button>'

                + '</td></tr>';
                });

                table += '</tbody></table>';

                //console.log(table);
                $("#bidtable").html(table);
            } else {
                $("#replyCount").empty();
                var reply = "";
                $.each(data, function (index, value) {

                reply += '<li class="comment" id="reply_' + value.Id + '">'
                +           '<div class="comment-wrapper">'
                +               '<div class="comment-author vcard">'
                +                  '<p class="gravatar">'
                +                      '<a href="#">'
                +                          '<img src="/Images/UserAvatar/' + value.Avatar + '" width="100" height="100" alt="avatar" />'
                +                      '</a>'
                +                  '</p>'
                +                  '<span class="author">' + value.Fullname + '</span>'
                +               '</div>'
                +               '<div class="comment-meta" id="total_' + value.Id + '">'
                +                  '<p><strong>' +  addDot(value.Total) + ' &#x20AB;</strong></p>'
                +               '</div>'
                +               '<div class="comment-body">'
                +                   '<p style="color:#b6b6b6">' + value.Address + '</p>'
                +               '</div>'
                +               '<div>'
                +                    '<button type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + value.Id + ')">Chi tiết</button> '
                +                    '<button type="button" class="btn btn-success btn-sm" onclick="select(' + value.Id + ')">Lựa chọn</button>'                                                   
                +               '</div>'
                +           '</div>'
                +       '</li>';
                
                
                //$("#reply-content").prepend(reply);
                });
                console.log(reply);

                $("#reply-content").html(reply);
            }
                 
        },
        error: function (error) {
            console.log(error);
        }
    });
}

//realtime update reply list - supplier
function updateReplies(data) {
    //console.log("Hello from updateReplies");
    if (data.Success === "Fail") {
        console.log("Error");
    } else {
        if (data.ReplyType === "Bid") {
            //console.log("Bid");
            $.ajax({
                type: 'GET',
                url: '/Reply/GetRank',
                data: 'id=' + data.ReplyId,
                success: function (data) {
                    updateBid(data);
                }
            });
        } else {
            var reply = '<li class="comment" id="reply_' + data.ReplyId + '">'
                +           '<div class="comment-wrapper">'
                +               '<div class="comment-author vcard">'
                +                  '<p class="gravatar">'
                +                      '<a href="#">'
                +                          '<img src="/Images/UserAvatar/' + data.Avatar + '" width="100" height="100" alt="avatar" />'
                +                      '</a>'
                +                  '</p>'
                +                  '<span class="author">' + data.SupplierName + '</span>'
                +               '</div>'
                +               '<div class="comment-meta" id="total_' + data.Id + '">'
                +                  '<p><strong>' + addDot(data.Total) + ' &#x20AB;</strong></p>'
                +               '</div>'
                +               '<div class="comment-body">'
                +                   '<p style="color:#b6b6b6">' + data.Address + '</p>'
                +               '</div>'
                +               '<div>'                
                +                    '<button id="edit" type="button" class="btn btn-primary btn-sm" onclick="edit(' + data.ReplyId + ')">Chỉnh sửa</button>'                                                   
                +               '</div>'
                +           '</div>'
                +       '</li>';

            $("#replyCount").empty();
            $("#replyBtn").remove();
            $("#reply-content").prepend(reply);
        }
    }
}

function updateBid(data) {
    var reply = '<table class="table">'
	+'<tbody><tr>'
	+ '<th>Thứ hạng</th>'
	+ '<th>Cửa hàng</th>'
	+ '<th>Tổng cộng</th>'
	+ '<th>Ngày giao hàng</th>'
	+ '<th>Thao tác</th>'
	+'</tr>'
	+'<tr>'
	+ '<td id="rank">' + data.Rank + '</td>'

	+ '<td><img src="/Images/UserAvatar/' + data.Avatar + '" width="100" height="100" alt="avatar"> ' + data.Fullname + '</td>'

	+'<td>'+  addDot(data.Total) + ' &#x20AB;</td>'

	+ '<td>' + new Date(parseInt(data.DeliveryDate.substr(6))).format("dd/mm/yyyy") + '</td>'

	+ '<td><button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="edit(' + data.Id + ')">Chỉnh sửa</button>'

			
	+'</td></tr></tbody></table>';

    $("#bidBtn").remove();
    $("#bidtable").html(reply);

}

function updateClientReplyTable(controllerName, actionName, requestId, userName, newreplyId) {
    if (controllerName === 'Request' && actionName === 'Details') {
        console.log("updateClientReplyTable");
        updateClientReply(requestId, userName, newreplyId);
    }
}

function updateClientReply(requestId, userName, newreplyId) {
    $.ajax({
        type: 'GET',
        url: '/Request/UpdateClientReplies',
        data: { id: requestId, username: userName, newreplyId: newreplyId },
        success: function (data) {
            console.log(data.Message);
            if (data.Message === "Ok") {

            } else {
                var reply = "";
                var replyId = data.replyId;
                console.log(replyId);
                $.each(data.data, function (index, value) {
                    reply += '<li class="comment" id="reply_' + value.Id + '">'
                    + '<div class="comment-wrapper">'
                    + '<div class="comment-author vcard">'
                    + '<p class="gravatar">'
                    + '<a href="#">'
                    + '<img src="/Images/UserAvatar/' + value.Avatar + '" width="100" height="100" alt="avatar" />'
                    + '</a>'
                    + '</p>'
                    + '<span class="author">' + value.Fullname + '</span>'
                    + '</div>'
                    + '<div class="comment-meta" id="total_' + value.Id + '">'
                    + '<p><strong>' + addDot(value.Total) + ' &#x20AB;</strong></p>'
                    + '</div>'
                    + '<div class="comment-body">'
                    + '<p style="color:#b6b6b6">' + value.Address + '</p>'
                    + '</div>'
                    + '<div>';
                    
                    if (replyId === value.Id) {
                        reply += '<button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="edit(' + value.Id + ')">Chỉnh sửa</button>';
                    } else {
                        reply += '<button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + value.Id + ')">Chi tiết</button> ';
                    }
                    reply += '</div>'
                    + '</div>'
                    + '</li>';
                });

                $("#reply-content").html(reply);
            }
            
        },
        error: function (error) {
            console.log(error);
        }
    });
}

//Realtime update when supplier edit reply

function updateClientReplyTable2(controllerName, actionName, requestId, userName, newreplyId) {
    if (controllerName === 'Request' && actionName === 'Details') {
        console.log("updateClientReplyTable2");
        updateClientReply2(requestId, userName, newreplyId);
    }
}

function updateClientReply2(requestId, userName, newreplyId) {
    $.ajax({
        type: 'GET',
        url: '/Request/UpdateClientRepliesEdit',
        data: { id: requestId, username: userName, newreplyId: newreplyId },
        success: function (data) {
            console.log(data.Message);
            if (data.Message === "Ok") {

            } else {
                var reply = "";
                var replyId = data.replyId;
                var flag = data.flag;

                console.log(flag);

                $.each(data.data, function (index, value) {
                    reply += '<li class="comment" id="reply_' + value.Id + '">'
                    + '<div class="comment-wrapper">'
                    + '<div class="comment-author vcard">'
                    + '<p class="gravatar">'
                    + '<a href="#">'
                    + '<img src="/Images/UserAvatar/' + value.Avatar + '" width="100" height="100" alt="avatar" />'
                    + '</a>'
                    + '</p>'
                    + '<span class="author">' + value.Fullname + '</span>'
                    + '</div>'
                    + '<div class="comment-meta" id="total_' + value.Id + '">'
                    + '<p><strong>' + addDot(value.Total) + ' &#x20AB;</strong></p>'
                    + '</div>'
                    + '<div class="comment-body">'
                    + '<p style="color:#b6b6b6">' + value.Address + '</p>'
                    + '</div>'
                    + '<div>';
                    if (flag === "owner") {
                        reply += '<button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + value.Id + ')">Chi tiết</button> ';
                        reply += '<button id="viewDetails" type="button" class="btn btn-success btn-sm" onclick="select(' + value.Id + ')">Lựa chọn</button> ';
                    }
                    else if (replyId === value.Id) {
                        reply += '<button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="edit(' + value.Id + ')">Chỉnh sửa</button>';
                    } else {
                        reply += '<button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + value.Id + ')">Chi tiết</button> ';
                    } 
                    reply += '</div>'
                    + '</div>'
                    + '</li>';
                });

                $("#reply-content").html(reply);
            }

        },
        error: function (error) {
            console.log(error);
        }
    });
}
function updateExistReplies(data, status, xhr) {
    if (data.Success === "Fail") {
        console.log("Fail");
        if (data.ReplyType === "Bid") {
            console.log(data.OldTotal);
            var message = data.Message + " (" + addDot(data.OldTotal) + " &#x20AB;)";
            console.log(message);
            $("#errorTotal").html(message);
            $("#oldTotal").html(data.OldTotal);
        }
    } else {
        if (data.ReplyType === "Bid") {
            $.ajax({
                type: 'GET',
                url: '/Reply/GetRank',
                data: 'id=' + data.ReplyId,
                success: function (data) {
                    updateBid(data);
                }
            });
        } else {
            var newTotal = '<p><strong>' + addDot(data.NewTotal) + ' &#x20AB;</strong></p>';
            var totalId = "#total_" + data.ReplyId;
            console.log("totalId:" + totalId, ", newTotal:" + data.NewTotal);
            $(totalId).html(newTotal);
        }
        $('#replyModal').modal('toggle');
        $("#replyInfo").empty();
    }
}


function showExpired() {
    $.ajax({
        type: 'GET',
        url: '/Request/Expired',
        success: function (data) {
            console.log("Heelo");

            var message = '<p>' + data.Message + '</p>';
            message += '<p><button type="button" class="btn btn-primary btn-sm" onclick="viewRequest(' + data.RequestId + ')">Go there...</button></p>';
            $('#message').html(message);
            $('#messageModal').modal('show');
            $('.select').show();
            $('#expired').html("Expired");

        },
        error: function (error) {
            console.log(error);
        }
    });
}

function viewRequest(requestId) {
    var url = "/Request/Details/" + requestId;
    console.log(url);
    window.location.href = url;
}