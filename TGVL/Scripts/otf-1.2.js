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

function selectedSuccess() {
    var table = $("#productList").children().clone(true, true);

    //console.log("Start");
    //var table = $("#cloneDest").children().clone(true, true);
    
    $("#clone").html(table);
    $("#listProduct").empty();
    $("#searchString").val('');
    $('#select').hide();
    $('#searchProduct').modal('toggle');
}

//chức năng reply ở trang request/details
function reply(requestId) {
    var options = {
        url: '/Reply/Create',
        type: 'GET',
        data: 'requestId=' + requestId
    };

    $.ajax(options).done(function (data) {
        //console.log("Hello");
        var $target = $('#replyInfo');
        $target.html(data);
        $('#replyModal').modal('show');
        
    });
}


function updateReplies(data) {
    var reply = '<li class="comment" id="reply_' + data.replyId + '">'
            +       '<div class="comment-wrapper">'
            +           '<div class="comment-author vcard">'
            +               '<p class="gravatar">'
            +                   '<a href="#">'
            +                       '<img src="/Images/UserAvatar/' +data.avatar +'" width="100" height="100" alt="avatar" />'      
            +                   '</a>'
            +               '</p>'
            +               '<span class="author">' + data.name +'</span>'
            +           '</div>'
            +           '<div class="comment-meta">'
            +               '<p><strong>' + data.total + ' VNĐ</strong></p>'
            +           '</div>'
            +           '<div class="comment-body">'
            +               data.description
            +               '<p style="color:#b6b6b6">'+ data.address +'</p>'
            +           '</div>'
            +       '</div>'
            + '</li>'


    $("#reply-content").prepend(reply);
}


