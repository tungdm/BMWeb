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
                    var message = '<li><a href="' + redirectUrl + '">' + value.Message + '<br/>' + new Date(parseInt(value.CreatedDate.substr(6))).format("dd/mm/yyyy HH:MM:ss") + '</a></li><hr/>';
                    
                    $('#notiContent').append(message);
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
    
    //ajax update session
    var options = {
        url: '/Request/UpdateNumOfUnseen',
        type: 'GET',
    };

    $.ajax(options).done(function (data) {
        console.log("Done");
    });

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
            var rank = "";
            if (data.Rank <= 3) {
                rank = '<img src="/Images/rank_' + data.Rank + '.png" style="max-height:75px; max-width:75px" />';
            } else {
                rank = data.Rank;
            }

            $("#rank").html(rank);
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
                    + '<th>Giá thầu</th>'
                    + '<th>Ngày giao hàng</th>'
                    + '<th>Thao tác</th>'
                    + '</tr>';

                $.each(data.BidReplies, function (index, value) {
                    var rank = "";
                    if (value.Rank <= 3) {
                        rank = '<img src="/Images/rank_' + value.Rank + '.png" style="max-height:75px; max-width:75px" />';
                    } else {
                        rank = value.Rank;
                    }
                    console.log(rank);

                    table += '<tr id="reply_' + value.Id + '">'
                + '<td>' + rank + '</td>'

                + '<td><img src="/Images/UserAvatar/' + value.Avatar + '" width="100" height="100" alt="avatar"> ' + value.Fullname + '</td>'

                + '<td><strong><span style="color: #ff1341; font-size: 20px">' + addDot(value.Total) + ' &#x20AB;</span></strong></td>'

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
                +                          '<img src="/Images/UserAvatar/' + value.Avatar + '" width="170" height="170" alt="avatar" />'
                +                      '</a>'
                +                  '</p>'
                +                  '<span class="author" style="color: #337ab7; font-size: 28px"><strong>' + value.Fullname + '</strong></span>'
                +               '</div>'
                +               '<div class="comment-meta" id="total_' + value.Id + '">'
                +                  '<p><strong>Giá đề xuất: <span style="color: #ff1341; font-size: 28px">' +  addDot(value.Total) + ' &#x20AB;</span></strong></p>'
                +               '</div>'
                +               '<div class="comment-body">'
                +                   '<p style="color:#333"><strong>Địa chỉ: </strong> <span style="color:#999">' + value.Address + '</span></p>'
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
            console.log("Bid");
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
                +                          '<img src="/Images/UserAvatar/' + data.Avatar + '" width="170" height="170" alt="avatar" />'
                +                      '</a>'
                +                  '</p>'
                +                  '<span class="author" style="color: #337ab7; font-size: 28px"><strong>' + data.SupplierName + '</strong></span>'
                +               '</div>'
                +               '<div class="comment-meta" id="total_' + data.ReplyId + '">'
                +                  '<p><strong>Giá đề xuất: <span style="color: #ff1341; font-size: 28px">' + addDot(data.Total) + ' &#x20AB;</span></strong></p>'
                +               '</div>'
                +               '<div class="comment-body">'
                +                   '<p style="color:#333"><strong>Địa chỉ: </strong> <span style="color:#999">' + data.Address + '</span></p>'
                +               '</div>'
                +               '<div>'                
                +                    '<button id="edit" type="button" class="btn btn-warning btn-sm" onclick="edit(' + data.ReplyId + ')">Chỉnh sửa</button>'                                                   
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
    var rank = "";
    if (data.Rank <= 3) {
        rank = '<img src="/Images/rank_' + data.Rank + '.png" style="max-height:75px; max-width:75px" />';
    } else {
        rank = data.Rank;
    }
    console.log(rank);

    var reply = '<table class="table">'
	+'<tbody><tr>'
	+ '<th>Thứ hạng</th>'
	+ '<th>Cửa hàng</th>'
	+ '<th>Giá thầu</th>'
	+ '<th>Ngày giao hàng</th>'
	+ '<th>Thao tác</th>'
	+'</tr>'
	+'<tr>'
	+ '<td id="rank">' + rank + '</td>'

	+ '<td><img src="/Images/UserAvatar/' + data.Avatar + '" width="100" height="100" alt="avatar"> ' + data.Fullname + '</td>'

	+ '<td><strong><span style="color: #ff1341; font-size: 20px">' + addDot(data.Total) + ' &#x20AB;</span></strong></td>'

	+ '<td>' + new Date(parseInt(data.DeliveryDate.substr(6))).format("dd/mm/yyyy") + '</td>'

	+ '<td><button id="viewDetails" type="button" class="btn btn-warning btn-sm" onclick="edit(' + data.Id + ')">Chỉnh sửa</button> '
    + '<button id="retract" type="button" class="btn btn-danger btn-sm" onclick="retract(' + data.Id + ')">Rút thầu</button>'
			
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
                    + '<img src="/Images/UserAvatar/' + value.Avatar + '" width="170" height="170" alt="avatar" />'
                    + '</a>'
                    + '</p>'
                    + '<span class="author" style="color: #337ab7; font-size: 28px"><strong>' + value.Fullname + '</strong></span>'
                    + '</div>'
                    + '<div class="comment-meta" id="total_' + value.Id + '">'
                    + '<p><strong>Giá đề xuất: <span style="color: #ff1341; font-size: 28px">' + addDot(value.Total) + ' &#x20AB;</span></strong></p>'
                    + '</div>'
                    + '<div class="comment-body">'
                    + '<p style="color:#333"><strong>Địa chỉ: </strong> <span style="color:#999">' + value.Address + '</span></p>'
                    + '</div>'
                    + '<div>';
                    
                    if (replyId === value.Id) {
                        reply += '<button id="viewDetails" type="button" class="btn btn-warning btn-sm" onclick="edit(' + value.Id + ')">Chỉnh sửa</button>';
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
                    + '<img src="/Images/UserAvatar/' + value.Avatar + '" width="170" height="170" alt="avatar" />'
                    + '</a>'
                    + '</p>'
                    + '<span class="author" style="color: #337ab7; font-size: 28px"><strong>' + value.Fullname + '</strong></span>'
                    + '</div>'
                    + '<div class="comment-meta" id="total_' + value.Id + '">'
                    + '<p><strong>Giá đề xuất: <span style="color: #ff1341; font-size: 28px">' + addDot(value.Total) + ' &#x20AB;</span></strong></p>'
                    + '</div>'
                    + '<div class="comment-body">'
                    + '<p style="color:#333"><strong>Địa chỉ: </strong> <span style="color:#999">' + value.Address + '</span></p>'
                    + '</div>'
                    + '<div>';
                    if (flag === "owner") {
                        reply += '<button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + value.Id + ')">Chi tiết</button> ';
                        reply += '<button id="viewDetails" type="button" class="btn btn-success btn-sm" onclick="select(' + value.Id + ')">Lựa chọn</button> ';
                    }
                    else if (replyId === value.Id) {
                        reply += '<button id="viewDetails" type="button" class="btn btn-warning btn-sm" onclick="edit(' + value.Id + ')">Chỉnh sửa</button>';
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
        $('#replyModal').modal('toggle');
        $("#replyInfo").empty();

        if (data.ReplyType === "Bid") {
            console.log("get new rank");
            $.ajax({
                type: 'GET',
                url: '/Reply/GetRank',
                data: 'id=' + data.ReplyId,
                success: function (data) {
                    updateBid(data);
                }
            });
        } else {
            var newTotal = '<p><strong>Giá đề xuất: <span style="color: #ff1341; font-size: 28px">' + addDot(data.NewTotal) + ' &#x20AB;</span></strong></p>';
            var totalId = "#total_" + data.ReplyId;
            console.log("totalId:" + totalId, ", newTotal:" + data.NewTotal);
            $(totalId).html(newTotal);
        }
        
    }
}


function showExpired() {
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

function countdownRequest(requestId) {
    var options = {
        url: '/Request/Expired',
        data: { requestId: requestId },
        type: 'GET',
    };
    $.ajax(options).done(function (data) {
        if (data.Message == "Success") {
            if (data.IsOwner) {
                $('.select').show();
            }
            
            $("#bidBtn").hide();
            $("#replyBtn").hide();
            $("#edit").hide();
            $('#expiredMessage').html("(Expired)");
        }

    });

}

function ExpiredOutside() {
    console.log("ExpiredOutside");
    var options = {
        url: '/Request/ExpiredOutside',
        type: 'GET',
    };

    $.ajax(options).done(function (data) {
        if (data.Message == "Success") {
            //if (data.IsOwner) {
            //    $('.select').show();
            //}

            //$("#bidBtn").hide();

            //$('#expiredMessage').html("(Expired)");
            console.log("ExpiredOutside functioni return");
        }

    });
}

