Date.prototype.toMSJSON = function () {
    var date = '/Date(' + this.getTime() + ')/'; //CHANGED LINE
    return date;
};

(function ($) {

    $(function () {

        var createCommand = function (id, version, payLoad) {
            return {
                id: id,
                version: version,
                payLoad: payLoad
            };
        }        
        
        var guid = function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                  v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }

        var signin = function (bearId, bearUsername, bearAvatarId) {
            var signinCmd = createCommand(bearId, 1, {
                "bearAvatarId": bearAvatarId,
                "bearUsername": bearUsername
            });            
            return $.ajax({
                type: "POST",
                url: "api/bears/signin",
                dataType: "json",
                data: JSON.stringify(signinCmd)
            });
        };

        //New******************
        var signinBear = function (bearId, bearUsername, bearPassword, bearAvatarId) {
            var signinCmd = createCommand(bearId, 1, {
                "bearPassword": bearPassword,
                "bearAvatarId": bearAvatarId,
                "bearUsername": bearUsername
            });
            return $.ajax({
                type: "POST",
                url: "api/bears/signinBear",
                dataType: "json",
                data: JSON.stringify(signinCmd)
            });
        }


        $("#signIn form").on('submit', function (e) {
            doNothing(e);

            var bearId = guid(); //fake bear id used only to provide an id to the commmand
            var bearUsername = $('#bearUsername').val();
            var bearAvatarId = $('input[name=bearAvatarId]:checked').val();
            signin(bearId, bearUsername, bearAvatarId).done(function (data) {
               document.location.replace(data.msg);
            }).fail(function (err) {
                $("#signinResult").html(err);
            });

        }); 

        $("#signUp form").on('submit', function (e) {
            doNothing(e);

            var bearId = guid(); //fake bear id used only to provide an id to the commmand
            var bearUsername = $('#bearUsernameUp').val();
            var bearPassword = $('#bearPasswordUp').val();
            var bearAvatarId = $('input[name=bearAvatarIdUp]:checked').val();
            signinBear(bearId, bearUsername, bearPassword, bearAvatarId).done(function (data) {
               document.location.replace(data.msg);
            }).fail(function (err) {
                $("#signinResult").html(err);
            });

        });     


    });


    $('.avatars input').on('click', function(){
      $('.avatars li').removeClass('checked');
      $(this).closest('li').addClass('checked');
    });

    var _hash = window.location.href.split('/')[window.location.href.split('/').length - 1];
    if(_hash === 'signUp')
        showSection(_hash);

})(jQuery)
