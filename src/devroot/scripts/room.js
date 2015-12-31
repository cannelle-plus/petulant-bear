

//Room
(function ($, bears) {

    var self = this;

    this.postMessageToRoom = function (roomId, version, message) {
        var postMessageCmd = createCommand(roomId, version, {
            "message": message
        });
        return $.ajax({
            type: "POST",
            url: "api/rooms/postmessage",
            dataType: "json",
            data: JSON.stringify(postMessageCmd)
        })
            .fail(function (err) {
                $("#postMessageToRoomResult").html(err);
            });
    };



    this.getDetailRoom = function (gameRoomId) {
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
                    var msg = [];
                    msg.push("postMessage <input id='msgRoom' type=text > <a id='postMessageToRoomBtn' href='#'  data-id='" + data.roomId + "' data-version='" + data.version + "' > postMessageToRoom </a><div id='postMessageToRoomResult'></div><br>");
                    msg.push("<TABLE  border=\"1\">");
                    msg.push("<TR>");
                    msg.push("<TH>roomId</TH>");
                    msg.push("<TH>name</TH>");
                    msg.push("<TH>message</TH>");
                    msg.push("</TR>");
                    msg.push("<TR>");
                    msg.push("<TD>" + data.roomId + "</TD>");
                    msg.push("<TD>" + data.name + "</TD>");
                    msg.push("<TD>");
                    msg.push("<TABLE  border=\"1\">");
                    msg.push("<TR>");
                    msg.push("<TH>roomId</TH>");
                    msg.push("<TH>bear</TH>");
                    msg.push("<TH>message</TH>");
                    msg.push("<TH>type message</TH>");
                    msg.push("</TR>");
                    for (var i = 0; i < data.messages.length; i++) {
                        msg.push("<TR>");
                        msg.push("<TD>" + data.messages[i].roomId + "</TD>");
                        msg.push("<TD>");
                        msg.push("<TABLE  border=\"1\">");
                        msg.push("<TR>");
                        msg.push("<TH>bearId</TH>");
                        msg.push("<TH>bearUsername</TH>");
                        msg.push("<TH>socialId</TH>");
                        msg.push("<TH>bearAvatarId</TH>");
                        msg.push("</TR>");
                        msg.push("<TR>");
                        msg.push("<TD>" + data.messages[i].bear.bearId + "</TD>");
                        msg.push("<TD>" + data.messages[i].bear.bearUsername + "</TD>");
                        msg.push("<TD>" + data.messages[i].bear.socialId + "</TD>");
                        msg.push("<TD>" + data.messages[i].bear.bearAvatarId + "</TD>");
                        msg.push("</TR>");
                        msg.push("</TABLE>");
                        msg.push("</TD>");
                        msg.push("<TD>" + data.messages[i].message + "</TD>");
                        msg.push("<TD>" + data.messages[i].typeMessage + "</TD>");
                        msg.push("</TR>");
                    };
                    msg.push("</TABLE>");
                    msg.push("</TD>");
                    msg.push("</TR>");

                    msg.push("</TABLE>");

                    $("#roomDetail").html(msg.join(''));
                }
                else {
                    $("#roomDetail").html("room not found");
                }

                $("#postMessageToRoomBtn").click(function (e) {
                    var roomId = $(e.target).data("id");
                    var roomVersion = $(e.target).data("version")
                    var message = $("#msgRoom").val();
                    self.postMessageToRoom(roomId, roomVersion, message);
                });
            })
            .fail(function (err) {
                $("#roomDetail").html(err);
            });
    };


}).call(this.PetulantBear.Room || (this.PetulantBear.Room = {}), jQuery, this.PetulantBear.Bears);





