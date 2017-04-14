// HIDE NOTIFICATIONS WHEN CLICKED ANYWHERE ON THE PAGE.
$(document).click(function (e) {
    var container = $('#notifications');
    if (!container.is(e.target) && container.has(e.target).length === 0) {
        container.hide();
    }
});

// Click on notification icon for show notification
$('#noti_Button').click(function (e) {
    e.stopPropagation();
    //$('.noti-content').show();

    $('#notifications').fadeToggle('fast', 'linear', function () {
        
        var page = $("#page-noti").text();
        console.log(page);
        if (page == "") {
            $('#notiContent').empty();
            $("#page-noti").text(1);
            page = parseInt($("#page-noti").text()); //set page = 1

            if ($('#notifications').is(':visible')) {
                updateNotification(page);
            }
        }

        
    });

    //$('#noti_Counter').fadeOut('slow');
    $('#noti_Counter').fadeOut(500, function () {
        $(this).empty();
    });


});

//update numbidder - all client
function updateNumbidder(controllerName, actionName, requestId) {
    if (controllerName === 'Request' && actionName === 'Details') {
        var options = {
            url: '/Request/GetNumBidder',
            type: 'GET',
            data: { requestId: requestId },
        };

        $.ajax(options).done(function (data) {
            if (data.Message == "Success") {
                console.log("Update Numbidder all");
                $("#numBidder").text(data.NumBidder);
            }
        });
    }
}

// update notification
function updateNotification(page) {
    console.log(page);
    $.ajax({
        type: 'GET',
        url: '/Home/GetNotificationReplies',
        data: { page: page },
        success: function (response) {
            if (response.length === 0) {
                $('#notiContent').append($('<li>No data available</li>'));
            }

            count = $("#notiContent li").length;
            if (count !== response.length) {
                $('#notiContent').empty();
                $("#page-noti").text(page+1); //next page

                $.each(response, function (index, value) {
                    var flag = value.Flag;
                    var redirectUrl = "";
                    switch (flag) {
                        case 1:
                            redirectUrl = "/Request/Details/" + value.RequestId + "#reply_" + value.ReplyId;
                            break;
                        case 2:
                            redirectUrl = "/Request/Details/" + value.RequestId;
                            break;
                        case 3:
                            redirectUrl = "#";
                            break;
                        case 4:
                            redirectUrl = "/Request/Details/" + value.RequestId;
                            break;
                        case 5:
                            redirectUrl = "/Order/Details/" + value.OrderId;
                        case 6:
                            redirectUrl = "/Request/Details/" + value.RequestId;
                    }
                    
                    console.log(redirectUrl);
                    var day = new Date(parseInt(value.CreatedDate.substr(6))).format("yyyy/mm/dd HH:MM:ss");
                    //console.log(day);
                    //console.log($.timeago(day));
                   
                    var message = '<li><a href="' + redirectUrl + '">'
                                  + '<span class="image"><img src="/Images/UserAvatar/' + value.Avatar + '" /></span>'
                                  + '<span>'
                                  + '<span><strong>' + value.Fullname + '</strong></span>'
                                  + '<span class="time">' + $.timeago(day) + '</span>'
                                  + '</span>'
                                  + '<span class="message">' + value.Message + '</span>'
                                  + '</a></li>';

                    
                    $('#notiContent').append(message);
                });
            }

            
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function updateNotification2(page) {
    console.log(page);
    $.ajax({
        type: 'GET',
        url: '/Home/GetNotificationReplies',
        data: { page: page },
        success: function (response) {
            if (response.Success == "End") {
                console.log("End");
                $("#page-noti").text(0); //end page
            } else {
                
                $("#page-noti").text(page+1); //next page
                $.each(response, function (index, value) {
                    var flag = value.Flag;
                    var redirectUrl = "";
                    switch (flag) {
                        case 1:
                            redirectUrl = "/Request/Details/" + value.RequestId + "#reply_" + value.ReplyId;
                            break;
                        case 2:
                            redirectUrl = "/Request/Details/" + value.RequestId;
                            break;
                        case 3:
                            redirectUrl = "#";
                            break;
                        case 4:
                            redirectUrl = "/Request/Details/" + value.RequestId;
                            break;
                        case 5:
                            redirectUrl = "/Order/Details/" + value.OrderId;
                    }

                    //console.log(redirectUrl);
                    var day = new Date(parseInt(value.CreatedDate.substr(6))).format("yyyy/mm/dd HH:MM:ss");
                    //console.log(day);
                    //console.log($.timeago(day));

                    var message = '<li><a href="' + redirectUrl + '">'
                                    + '<span class="image"><img src="/Images/UserAvatar/' + value.Avatar + '" /></span>'
                                    + '<span>'
                                    + '<span><strong>' + value.Fullname + '</strong></span>'
                                    + '<span class="time">' + $.timeago(day) + '</span>'
                                    + '</span>'
                                    + '<span class="message">' + value.Message + '</span>'
                                    + '</a></li>';


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

//Update bid table khi customer ban supplier - customer
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
            if (data.Rank == 1) {
                rank = '<img src="/Images/rank_' + data.Rank + '.png" style="max-height:75px; max-width:75px" />';
            } else if (data.Rank <= 5 && data.Rank > 1) {
                rank = "Nhóm dẫn đầu";
            } else {
                rank = data.Rank;
            }

            if (data.Rank == null) {
                console.log("Ban");
                $("#bidtable").remove();
            }

            $("#rank").html(rank);

            var pending = $('#autoBidModal').is(':visible'); //pending = true <=> dang cap nhat, tam thoi k update
            console.log("pending=" + pending);

            if (data.Rank > data.OldRank && data.Min != null && !pending) {
                
                var replyId = data.ReplyId;
                
                myTimeOut = setTimeout(function () {
                    autobidAgain(replyId);
                }, 3000)
            }
        },
        error: function (error) {
            console.log(error);
        }
        
    });
}

function autobidAgain(replyId) {
    var options = {
        url: '/Reply/AutoBidAgain',
        type: 'GET',
        data: { replyId: replyId }
    };

    $.ajax(options).done(function (data) {
        $.ajax({
            type: 'GET',
            url: '/Reply/GetRank',
            data: 'id=' + data.ReplyId,
            success: function (data) {
                updateBid(data);
            }
        });
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
                //$("#numBidder").text(data.NumBidder);
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

                + '<td><img src="/Images/UserAvatar/' + value.Avatar + '" width="100" height="100" alt="avatar" style="display: block; margin-bottom: 5px;"><span class="supplierName"><strong> ' + value.Fullname + '</strong></span></td>'

                + '<td><strong><span style="color: #ff1341; font-size: 20px">' + addDot(value.Total) + ' &#x20AB;</span></strong></td>'

                + '<td>' + new Date(parseInt(value.DeliveryDate.substr(6))).format("dd/mm/yyyy") + '</td>'

                + '<td><button type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + value.Id + ')">Chi tiết</button> '

                + '<button type="button" class="select btn btn-success btn-sm" style="display:none" onclick="select(' + value.Id + ')">Lựa chọn</button> ';

                    if (data.Banable == "true") {
                        table += '<button type="button" class="select btn btn-danger btn-sm ban-btn" onclick="ban(' + value.Id + ')">Cấm đặt thầu</button> ';
                    }
                

                table += '</td></tr>';
                });

                table += '</tbody></table>';

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
            //var numBidder = parseInt($("#numBidder").text()) + 1;
            //$("#numBidder").text(numBidder); //increase numbidder + 1

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
    //if (data.Rank <= 3) {
    //    rank = '<img src="/Images/rank_' + data.Rank + '.png" style="max-height:75px; max-width:75px" />';
    //} else {
    //    rank = data.Rank;
    //}

    if (data.Rank == 1) {
        rank = '<img src="/Images/rank_' + data.Rank + '.png" style="max-height:75px; max-width:75px" />';
    } else if (data.Rank <=5 && data.Rank > 1) {
        rank = "Nhóm dẫn đầu";
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

	+ '<td><img src="/Images/UserAvatar/' + data.Avatar + '" width="100" height="100" alt="avatar" style="display: block; margin-bottom: 5px;" /><span class="supplierName"><strong> ' + data.Fullname + '</strong></span></td>'

	+ '<td><strong><span style="color: #ff1341; font-size: 20px">' + addDot(data.Total) + ' &#x20AB;</span></strong></td>'

	+ '<td>' + new Date(parseInt(data.DeliveryDate.substr(6))).format("dd/mm/yyyy") + '</td>'

	+ '<td><button id="viewDetails" type="button" class="btn btn-warning btn-sm" onclick="edit(' + data.Id + ')">Chỉnh sửa</button> '
    + '<button id="autobid" type="button" class="btn btn-info btn-sm" onclick="autobid(' + data.Id + ')">Tự đặt giá</button> '
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
    console.log("updateExistReplies");
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
        if ($('#replyModal').is(':visible')) {
            $('#replyModal').modal('toggle');
            $("#replyInfo").empty();
            $('#submit-btn-reply').hide();
        } else if ($('#autoBidModal').is(':visible')) {
            $('#autoBidModal').modal({
                backdrop: true,
                keyboard: true
            });
            $('#autoBidModal').modal('toggle');
            $("#autoBidInfo").empty();
        }

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

function updateExistRepliesAutoBid(data, status, xhr) {
    console.log("updateExistRepupdateExistRepliesAutoBidlies");
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
       
        $('#autoBidModal').modal({
            backdrop: true,
            keyboard: true
        });

        $('#autoBidModal').modal('toggle');
       
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
            $('#expired').html("Hết hạn");

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
    console.log("Inside");

    var options = {
        url: '/Request/Expired',
        data: { requestId: requestId },
        type: 'GET',
    };
    $.ajax(options).done(function (data) {
        if (data.Message == "Success") {
            if (data.IsOwner) {
                $('.select').show();
                $(".ban-btn").hide();
            } else if (!data.IsOwner)
            {
                console.log("Hello from realtime update bid reply after expired");
                var table = '<table class="table">'
                    + '<tbody><tr>'
                    + '<th>Thứ hạng</th>'
                    + '<th>Cửa hàng</th>'
                    + '<th>Giá thầu</th>'
                    + '<th>Ngày giao hàng</th>'
                    + '<th>Thao tác</th>'
                    + '</tr>';

                $.each(data.ListBid, function (index, value) {
                    var rank = "";
                    if (value.Rank <= 3) {
                        rank = '<img src="/Images/rank_' + value.Rank + '.png" style="max-height:75px; max-width:75px" />';
                    } else {
                        rank = value.Rank;
                    }
                    console.log(rank);

                    table += '<tr id="reply_' + value.Id + '">'
                + '<td>' + rank + '</td>'

                + '<td><img src="/Images/UserAvatar/' + value.Avatar + '" width="100" height="100" alt="avatar" style="display: block; margin-bottom: 5px;"><span class="supplierName"><strong> ' + value.Fullname + '</strong></span></td>'

                + '<td><strong><span style="color: #ff1341; font-size: 20px">' + addDot(value.Total) + ' &#x20AB;</span></strong></td>'

                + '<td>' + new Date(parseInt(value.DeliveryDate.substr(6))).format("dd/mm/yyyy") + '</td>'

                + '<td><button type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + value.Id + ')">Chi tiết</button> '

                + '</td></tr>';
                });

                table += '</tbody></table>';

                //console.log(table);
                $("#bidtable").html(table);
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

