

//Room
(function ($, bears) {

    var self = this;

    this.postMessageToRoom = function (roomId, version, message) {
        var postMessageCmd = createCommand(roomId, version, {
            "message": message
        });
        if(message !== "") {
          loader.show();
          return $.ajax({
            type: "POST",
            url: "api/rooms/postmessage",
            dataType: "json",
            data: JSON.stringify(postMessageCmd)
          })
          .done(function(data){                                
              setTimeout(function () {
                  loader.hide();
              }, 2000);
          })
          .fail(function (err) {
              $("#postMessageToRoomResult").html(err);
          });
        }
    };

    this.getDetailRoom = function (gameRoomId) {
            loader.show();
            $.ajax({
                type: "POST",
                url: "api/rooms/detail",
                dataType: "json",
                data: JSON.stringify({
                    roomId: gameRoomId
                })
            })
              .done(function (data) {
                  if (data !== null && data !== undefined) {
                      $('#chat').data('id', data.roomId)
                      $('#chat').data('version', data.version)
                      var msg = [];
                      msg.push("<dl class='chat'>");
                      //msg.push("<TD>" + data.roomId + "</TD>");
                      //msg.push("<TD>" + data.name + "</TD>");
                      for (var i = data.messages.length - 1; i >= ((data.messages.length < 50) ? 0 : data.messages.length - 9); i--) {
                          //msg.push("<TD>" + data.messages[i].bear.bearId + "</TD>");
                          msg.push("<dt style='background-image:url(images/avatar-0" + data.messages[i].bear.bearAvatarId + ".png);'>" + data.messages[i].bear.bearUsername + "</dt>");
                          //msg.push("<TD>" + data.messages[i].bear.socialId + "</TD>");
                          //msg.push("<TD>" + data.messages[i].bear.bearAvatarId + "</TD>");
                          msg.push("<dd class='" + data.messages[i].typeMessage + "'>" + data.messages[i].message + "</dd>");
                      };

                      msg.push("</dl>");

                      $(".roomDetail").html(msg.join(''));
                  }
                  else {
                      $(".roomDetail").html("room not found");
                  }
                  loader.hide();
              })
              .fail(function (err) {
                  $(".roomDetail").html(err);
              });
    };


}).call(this.PetulantBear.Room || (this.PetulantBear.Room = {}), jQuery, this.PetulantBear.Bears);





