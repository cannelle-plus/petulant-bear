(function ($) {

    $(function () {

        var login = function (username, password) {
            var loginCmd = {
                "username": username,
                "password": password
            };
            return $.ajax({
                type: "POST",
                url: "bearLogin",
                dataType: "json",
                data: JSON.stringify(loginCmd)
            })
            .done(function (data) {
                document.location.replace(data.msg);
            }).fail(function (err) {
                $("#signinResult").html(err);
            });
        }

    });

})(jQuery)