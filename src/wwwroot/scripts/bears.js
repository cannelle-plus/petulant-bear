// JavaScript source code

this.PetulantBear = this.PetulantBear || {};

(function ($) {

    var self = this;
    this.changePassword = function (password) {
        var changePasswordCmd = createCommand(guid(), 1, {
            bearPassword: password
        });
        return $.ajax({
            type: "POST",
            url: "api/bear/changePassword",
            dataType: "json",
            data: JSON.stringify(changePasswordCmd)
        })
          .fail(function (err) {
              $("#changePasswordResult").html(err);
          });
    };

    this.changeUserName = function (username) {
        var changeUsernameCmd = createCommand(guid(), null, {
            bearUsername: username
        });
        return $.ajax({
            type: "POST",
            url: "api/bear/changeUserName",
            dataType: "json",
            data: JSON.stringify(changeUsernameCmd)
        })
          .fail(function (err) {
              $("#changeUsernameResult").html(err);
          });
    };

    this.changeAvatarId = function (avatarId) {
        var changeAvatarIdCmd = createCommand(guid(), null, {
            bearAvatarId: avatarId
        });
        return $.ajax({
            type: "POST",
            url: "api/bear/changeAvatarId",
            dataType: "json",
            data: JSON.stringify(changeAvatarIdCmd)
        })
          .fail(function (err) {
              $("#changeAvatarIdResult").html(err);
          });
    };

    this.changeEmail = function (email) {
        var changeEmailCmd = createCommand(guid(), null, {
            bearEmail: email
        });
        return $.ajax({
            type: "POST",
            url: "api/bear/changeEmail",
            dataType: "json",
            data: JSON.stringify(changeEmailCmd)
        })
          .fail(function (err) {
              $("#changeEmailResult").html(err);
          });
    };

    this.getBear = function (id) {
        //extract the detail of the first bear
        loader.show();
        $.ajax({
            type: "POST",
            url: "api/bears/detail",
            dataType: "json",
            data: JSON.stringify({
                bearId: id
            })
        })
          .done(function (data) {
              if (data !== null && data !== undefined) {
                console.log(data);
                  ownerBear = data;
              } else {
                  $("#bearDetail").html("no bear found");
              }
              loader.hide();
          })
          .fail(function (err) {
              $("#bearDetail").html(err);
          });
    }

    this.getBears = function () {
        $("#bearDetail").html('');
        loader.show();
        $.ajax({
            type: "POST",
            url: "api/bears/list",
            dataType: "json",
            data: JSON.stringify({})
        })
          .done(function (data) {
              var msg = [];
              msg.push(data.length + " bears in the database currently");
              msg.push("<TABLE  border=\"1\">");
              msg.push("<TR>");
              msg.push("<TH>bearId</TH>");
              msg.push("<TH>bearUsername</TH>");
              msg.push("<TH>bearAvatarId</TH>");
              msg.push("</TR>");
              for (var i = 0; i < data.length; i++) {
                  msg.push("<TR>");
                  msg.push("<TD>" + data[i].bearId + "</TD>");
                  msg.push("<TD>" + data[i].bearUsername + "</TD>");
                  msg.push("<TD>" + data[i].bearAvatarId + "</TD>");
                  msg.push("<TD><div href='#' class='bearsDetailBtn' data-id='" + data[i].bearId + "'>detail</div></TD>");

                  msg.push("</TR>");
              };
              msg.push("</TABLE>");
              $("#bearsList").html(msg.join(''));

              $(".bearsDetailBtn").click(function (e) {
                  self.getBear($(e.target).data("id"));
              });
              loader.hide();
          })
          .fail(function (err) {
              $("#bearsList").html(err);
          });
    };

    var _currentBear = null;

    this.getcurrentBear = function () {
        if (!_currentBear) console.log("current bear not defined");
        return _currentBear;
    }

    this.setAvatar = function() {       
          loader.show();
          $.ajax({
              type: "GET",
              url: "api/bears/current"
            })
            .done(function (data) {
              $('header img').attr('src', 'images/avatar-0' + data.bearAvatarId + '.png');
                      
              //-- Set profile screen
              $('#profile #bearUsername').val(data.bearUsername);
              $('#profile #bearEmail').val(data.bearEmail);
              $('#profile .avatars li:eq(' + (data.bearAvatarId - 1) + ')').addClass('checked');
              $('#profile .avatars li:eq(' + (data.bearAvatarId - 1) + ') input').attr('checked', 'checked');
              $('.avatars input').on('click', function(){
                $('.avatars li').removeClass('checked');
                $(this).closest('li').addClass('checked');
              });
              loader.hide();
            })
              .fail(function (err) {
          });
        }

    this.init = function () {

        // Change Profile
        $("#changeBearName").on('submit', function (e) {
            doNothing(e);
            loader.show();
            self.changeUserName($('#bearUsername').val()).done(function(){
              loader.hide();
            });
        });

        $("#changeBearAvatar").on('submit', function (e) {
            doNothing(e);
            loader.show();
            self.changeAvatarId($('#changeBearAvatar input[name=bearAvatarId]:checked').val()).done(function(){
              self.setAvatar();
              loader.hide();
            });
        });

        $("#changeBearEmail").on('submit', function (e) {
            doNothing(e);
            loader.show();
            self.changeEmail($('#bearEmail').val()).done(function () {
              loader.hide();
            });
        });

        //Onload
        this.setAvatar();

        $('.logout').on('click', function (e) {
            doNothing(e);
            loader.show();
            $.ajax({
                url: "/logout"
            })
              .done(function (data) {
                  window.location.href = '/'
              })
              .fail(function (err) {
                  console.error(err);
              })
        });

        //extract the detail of the current bear
        return $.ajax({
            type: "GET",
            url: "api/bears/current",
            dataType: "json"
        })
        .done(function (data) {
            if (data !== null && data !== undefined) {
                _currentBear = data;
                var msg = [];
                msg.push("<TABLE  border=\"1\">");
                msg.push("<TR>");
                msg.push("<TH>bearId</TH>");
                msg.push("<TH>bearUsername</TH>");
                msg.push("<TH>socialId</TH>");
                msg.push("<TH>bearAvatarId</TH>");
                msg.push("</TR>");
                msg.push("<TR>");
                msg.push("<TD>" + data.bearId + "</TD>");
                msg.push("<TD>" + data.bearUsername + "</TD>");
                msg.push("<TD>" + data.socialId + "</TD>");
                msg.push("<TD>" + data.bearAvatarId + "</TD>");
                msg.push("</TR>");

                msg.push("</TABLE>");
                $("#currentBearDetail").html(msg.join(''));
            } else {
                $("#currentBearDetail").html("no current bear found");
            }
        })
        .fail(function (err) {
            $("#bearDetail").html(err);
        });       

        

    }


}).call(this.PetulantBear.Bears || (this.PetulantBear.Bears = {}), jQuery);