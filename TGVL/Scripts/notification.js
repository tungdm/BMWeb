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
    //$('#notiContent').empty();
    //$('#notiContent').append($('<li>Loading...</li>'));

    $.ajax({
        type: 'GET',
        url: 'Home/GetNotificationReplies',
        success: function (response) {
            if (response.length === 0) {
                $('#notiContent').append($('<li>No data available</li>'));
            }

            count = $("#notiContent li").length;
            if (count !== response.length) {
                $('#notiContent').empty();
            
                $.each(response, function (index, value) {
                    $('#notiContent').append($('<li>New reply by : ' + value.Supplier + ' at : ' + new Date(parseInt(value.CreatedDate.substr(6))).format("dd/mm/yyyy HH:MM:ss") + '</li>'));
                });
            }

            
        },
        error: function (error) {
            console.log(error);
        }
    });
}

// update notification count
function updateNotificationCount() {
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

}

