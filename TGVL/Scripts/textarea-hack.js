
//$('#submit-btn').click(function () {
//    $('#hidden-editor').html(
//        $("#editor").html()
//    );
//});

$('#submit-btn').bind('click', function () {
    $("#Description").val($("#editor").html().replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;'));
});