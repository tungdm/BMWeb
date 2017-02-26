$(function () {
    // Click on notification icon for show notification
    $('span.noti').click(function (e) {
        //alert("Hello");
        e.stopPropagation();
        $('.noti-content').show();
        var count = 0;
        count = parseInt($('span.count').html()) || 0;
        //only load notification if not already loaded
        if (count > 0) {
            updateNotification();
        }
        $('span.count', this).html('&nbsp;');
    });
    // hide notifications
    $('html').click(function () {
        $('.noti-content').hide();
    });

    // update notification count
    function updateNotificationCount() {
        var count = 0;
        count = parseInt($('span.count').html()) || 0;
        count++;
        $('span.count').html(count);
    }
    // signalr js code for start hub and send receive notification
    var notificationHub = $.connection.notificationHub;
    $.connection.hub.start().done(function () {
        console.log('Notification hub started');
    });
    //signalr method for push server message to client
    notificationHub.client.notify = function (message) {
        if (message && message.toLowerCase() === "added") {
            updateNotificationCount();
        }
    };
});