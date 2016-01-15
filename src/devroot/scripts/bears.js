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
                  $("#bearDetail").html(msg.join(''));
              } else {
                  $("#bearDetail").html("no bear found");
              }


          })
          .fail(function (err) {
              $("#bearDetail").html(err);
          });
    }

    this.isBearKnown = function (socialId) {
        //extract the detail of the first bear
        $.ajax({
            type: "POST",
            url: "/api/bears/isBearKnown",
            dataType: "json",
            data: JSON.stringify({
                "socialId": socialId
            })
        })
          .done(function (data) {
              if (data !== null && data !== undefined) {
                  var msg = [];
                  msg.push(data);
                  $("#isBearKnown").html(msg.join(''));
              } else {
                  $("#isBearKnown").html("no bear found");
              }


          })
          .fail(function (err) {
              $("#bearDetail").html(err);
          });
    }





    this.getBears = function () {
        $("#bearDetail").html('');
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
                  getBear($(e.target).data("id"));
              });
          })
          .fail(function (err) {
              $("#bearsList").html(err);
          });
    };

    var _currentBear = null;
    this.getcurrentBear = function () {
        if (!_currentBear) console.log("curretn bear not defined");
        return _currentBear;
    }

    this.init = function () {
        

        $("#bearsListBtn").click(self.getBears);
        $("#isBearKnownBtn").click(function (e) { self.isBearKnown('1388221448136137') });

        

        
        $("#bearChangeEmailBtn").click(function () {
            var newEmail = "artissae@gmail.com";
            self.changeEmail(newEmail).done(function (data) {
                $("#changeEmailResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
            });
        });

        $("#bearChangePasswordBtn").click(function () {
            var newPassword = "newPassword" + guid();
            self.changePassword(newPassword).done(function (data) {
                $("#changePasswordResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
            });
        });

        $("#bearChangeUsernameBtn").click(function () {
            var newUserName = "newUserName" + guid();
            self.changeUserName(newUserName).done(function (data) {
                $("#changeUsernameResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
            });
        });

        var i = 1;
        $("#bearChangeAvatarIdBtn").click(function () {

            self.changeAvatarId(i++).done(function (data) {
                $("#changeAvatarIdResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
            });
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