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
function updateNotificationCount(controllerName, actionName, requestId) {
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
        
        var rId = $("#requestID").text();
        var rType = $("#requestType").text();
        
        //Realtime update reply khi customer đang ở chính trang view detail request của mình
        if (rId === requestId) {
            console.log("Hello from updateNotificationCount");
            updateReply(requestId, rType);
        }
    }
}

//Supplier - update bid table khi rank thay đổi
function updateBidTable(controllerName, actionName, requestId) {
    if (controllerName === 'Request' && actionName === 'Details') {       
        //Realtime update bid reply khi supplier đang ở chính trang view detail request của mình
        console.log("Hello from updateBidTable");
        supplierUpdateBidReply(requestId);   
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
function updateReply(requestId, rType) {
    $.ajax({
        type: 'GET',
        url: '/Request/UpdateReplies',
        //data: 'id=' + requestId,
        data: {id:requestId, type:rType},
        success: function (data) {
            if (data.ReplyType === "Bid") {
                console.log("Hello from realtime update bid reply");
                var table = '<table class="table">'
                    + '<tbody><tr>'
                    + '<th>Rank</th>'
                    + '<th>Supplier</th>'
                    + '<th>Total</th>'
                    + '<th>Delivery Date</th>'
                    + '<th>Action</th>'
                    + '</tr>';

                $.each(data.BidReplies, function (index, value) {
                    table += '<tr>'
                + '<td>' + value.Rank + '</td>'

                + '<td><img src="/Images/UserAvatar/' + value.Avatar + '" width="100" height="100" alt="avatar"> ' + value.Fullname + '</td>'

                + '<td>' + value.Total + '</td>'

                + '<td>' + new Date(parseInt(value.DeliveryDate.substr(6))).format("dd/mm/yyyy") + '</td>'

                + '<td><button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + value.ReplyId + ')">Details</button>'


                + '</td></tr>';
                });

                table += '</tbody></table>';

                //console.log(table);
                $("#bidtable").html(table);
            } else {
                $("#replyCount").empty();
                $.each(data, function (index, value) {

                var reply = '<li class="comment" id="reply_' + value.ReplyId + '">'
                +           '<div class="comment-wrapper">'
                +               '<div class="comment-author vcard">'
                +                  '<p class="gravatar">'
                +                      '<a href="#">'
                +                          '<img src="/Images/UserAvatar/' + value.Avatar + '" width="100" height="100" alt="avatar" />'
                +                      '</a>'
                +                  '</p>'
                +                  '<span class="author">' + value.SupplierName + '</span>'
                +               '</div>'
                +               '<div class="comment-meta">'
                +                  '<p><strong>' + value.Total + ' VNĐ</strong></p>'
                +               '</div>'
                +               '<div class="comment-body">'
                +                   value.Description
                +                   '<p style="color:#b6b6b6">' + value.Address + '</p>'
                +               '</div>'
                +               '<div>'
                +                    '<button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + value.ReplyId + ')">Details</button> '
                +                    '<button id="Select" type="button" class="btn btn-primary btn-sm" onclick="edit(' + value.ReplyId + ')">Select</button>'                                                   
                +               '</div>'
                +           '</div>'
                +       '</li>';
                
                
                $("#reply-content").prepend(reply);
            });
            }
                 
        },
        error: function (error) {
            console.log(error);
        }
    });
}

//update reply - supplier
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

            //updateBid(data);
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
                +               '<div class="comment-meta">'
                +                  '<p><strong>' + data.Total + ' VNĐ</strong></p>'
                +               '</div>'
                +               '<div class="comment-body">'
                +                   data.Description
                +                   '<p style="color:#b6b6b6">' + data.Address + '</p>'
                +               '</div>'
                +               '<div>'
                +                    '<button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + data.ReplyId + ')">Details</button> '
                +                    '<button id="edit" type="button" class="btn btn-primary btn-sm" onclick="edit(' + data.ReplyId + ')">Edit</button>'                                                   
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
	+'<th>Rank</th>'
	+'<th>Supplier</th>'
	+'<th>Total</th>'
	+'<th>Delivery Date</th>'
	+'<th>Action</th>'
	+'</tr>'
	+'<tr>'
	+ '<td id="rank">' + data.Rank + '</td>'

	+ '<td><img src="/Images/UserAvatar/' + data.Avatar + '" width="100" height="100" alt="avatar"> ' + data.Fullname + '</td>'

	+'<td>'+ data.Total +'</td>'

	+ '<td>' + new Date(parseInt(data.DeliveryDate.substr(6))).format("dd/mm/yyyy") + '</td>'

	+ '<td><button id="viewDetails" type="button" class="btn btn-primary btn-sm" onclick="viewDetails(' + data.ReplyId + ')">Details</button>'

			
	+'</td></tr></tbody></table>';


    $("#bidtable").html(reply);
}