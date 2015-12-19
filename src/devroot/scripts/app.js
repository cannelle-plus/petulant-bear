Date.prototype.addHours = function (h) {
    this.setTime(this.getTime() + (h * 60 * 60 * 1000));
    return this;
};

(function ($) {

    $(function () {


        var guid = function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                  v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }

        var createCommand = function (id, version, payLoad) {
            return {
                id: id,
                idCommand : guid(),
                version: version,
                payLoad: payLoad
            };
        }


        var schedule = function (id, version) {
            var scheduleCmd = createCommand(id, 0, {
                "location": "playSoccer",
                "maxPlayers": 8,
                "name": "test",
                "startDate": "\/Date(1532802039368+0200)\/"
            });
            return $.ajax({
                type: "POST",
                url: "api/games/schedule",
                dataType: "json",
                data: JSON.stringify(scheduleCmd)
            })
              .fail(function (err) {
                  $("#scheduleResult").html(err);
              });
        }

        var cancel = function (id, version) {
            var cancelCmd = createCommand(id, version, {});
            return $.ajax({
                type: "POST",
                url: "api/games/cancel",
                dataType: "json",
                data: JSON.stringify(cancelCmd)
            })
              .fail(function (err) {
                  $("#cancelResult").html(err);
              });
        }


        var join = function (id, version) {
            var joinCmd = createCommand(id, version, {});
            return $.ajax({
                type: "POST",
                url: "api/games/join",
                dataType: "json",
                data: JSON.stringify(joinCmd)
            })
              .fail(function (err) {
                  $("#joinResult").html(err);
              });
        }

        var changeName = function (id, version) {
            var changeNameCmd = createCommand(id, version, {
                name: "new Name" + guid()
            });
            return $.ajax({
                type: "POST",
                url: "api/games/changeName",
                dataType: "json",
                data: JSON.stringify(changeNameCmd)
            })
              .fail(function (err) {
                  $("#changeNameResult").html(err);
              });
        }
        var changeStartDate = function (id, version) {
            var newStartDate = (new Date()).addHours(10);
            var changeStartDateCmd = createCommand(id, version, {
                startDate: newStartDate.toJSON()
            });
            return $.ajax({
                type: "POST",
                url: "api/games/changeStartDate",
                dataType: "json",
                data: JSON.stringify(changeStartDateCmd)
            })
                .fail(function (err) {
                    $("#changeStartDateResult").html(err);
                });
        }
        var changeLocation = function (id, version) {
            var changeLocationCmd = createCommand(id, version, {
                location: "new location" + guid()
            });
            return $.ajax({
                type: "POST",
                url: "api/games/changeLocation",
                dataType: "json",
                data: JSON.stringify(changeLocationCmd)
            })
                .fail(function (err) {
                    $("#changeLocationResult").html(err);
                });

        };
        var changeMaxPlayer = function (id, version) {
            var changeMaxPlayerCmd = createCommand(id, version, {
                maxPlayers: 7
            });
            return $.ajax({
                type: "POST",
                url: "api/games/changeMaxPlayer",
                dataType: "json",
                data: JSON.stringify(changeMaxPlayerCmd)
            })
                .fail(function (err) {
                    $("#changeMaxPlayerResult").html(err);
                });

        };
        var abandon = function (id, version) {
            var abandonCmd = createCommand(id, version, {});
            return $.ajax({
                type: "POST",
                url: "api/games/abandon",
                dataType: "json",
                data: JSON.stringify(abandonCmd)
            })
              .fail(function (err) {
                  $("#abandonResult").html(err);
              });
        };

        var kickPlayer = function (gameId,version, bearId) {
            var kickPlayerCmd = createCommand(gameId, version, {
                "kickedBearId": bearId
            });
            return $.ajax({
                type: "POST",
                url: "api/games/kickPlayer",
                dataType: "json",
                data: JSON.stringify(kickPlayerCmd)
            })
              .fail(function (err) {
                  $("#kickPlayerResult").html(err);
              });
        };

        //afterGame
        var markBear = function (gameId,version, bearId, mark) {
            var markBearCmd = createCommand(gameId, version, {
                "bearId": bearId,
                "mark": mark
            });
            return $.ajax({
                type: "POST",
                url: "api/afterGames/markBear",
                dataType: "json",
                data: JSON.stringify(markBearCmd)
            })
              .fail(function (err) {
                  $("#markBearResult").html(err);
              });
        }

        var commentBear = function (gameId,version, bearId, comment) {
            var commentBearCmd = createCommand(gameId, version, {
                "bearId": bearId,
                "comment": comment
            });
            return $.ajax({
                type: "POST",
                url: "api/afterGames/commentBear",
                dataType: "json",
                data: JSON.stringify(commentBearCmd)
            })
              .fail(function (err) {
                  $("#commentBearResult").html(err);
              });
        };

        
        

        var signalSent = function (transmitterId, version, receiverId, signalStrength, receptionDate) {
            var signalsSent = createCommand(transmitterId, version, {
                signals: [{
                    "transmitterId": transmitterId,
                    "receiverId": receiverId,
                    "signalStrength": signalStrength,
                    "receptionDate": receptionDate.toMSJSON()
                }]
            });
            return $.ajax({
                type: "POST",
                url: "api/signals/received",
                dataType: "json",
                data: JSON.stringify(signalsSent)
            })
              .fail(function (err) {
                  $("#signalsReceivedResult").html(err);
              });
        }

        var startCalibration = function (transmitterId, version,receiverId, distance) {
            var startCalibrationCmd = createCommand(transmitterId, version, {
                "transmitterId": transmitterId,
                "receiverId": receiverId,
                "distance": distance
            });
            return $.ajax({
                type: "POST",
                url: "api/signals/startCalibration",
                dataType: "json",
                data: JSON.stringify(startCalibrationCmd)
            })
              .fail(function (err) {
                  $("#startCalibrationResult").html(err);
              });
        }

        var stopCalibration = function (transmitterId,version, receiverId, distance) {
            var stopCalibrationCmd = createCommand(transmitterId, version, {
                "transmitterId": transmitterId,
                "receiverId": receiverId,
                "distance": distance
            });
            return $.ajax({
                type: "POST",
                url: "api/signals/stopCalibration",
                dataType: "json",
                data: JSON.stringify(stopCalibrationCmd)
            })
              .fail(function (err) {
                  $("#stopCalibrationResult").html(err);
              });
        }

        var postMessageToRoom = function (roomId, version, message) {
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

        var changePassword = function (password) {
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

        var changeUserName = function (username) {
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

        var changeAvatarId = function (avatarId) {
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

        var getDetailRoom = function (gameRoomId) {
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
                      postMessageToRoom(roomId, roomVersion, message);
                  });
              })
              .fail(function (err) {
                  $("#roomDetail").html(err);
              });
        };

        var getGame = function (gameId, gameVersion) {
            $.ajax({
                type: "POST",
                url: "api/games/detail",
                dataType: "json",
                data: JSON.stringify({
                    id: gameId
                })
            })
            .done(function (data) {
                if (data !== null && data !== undefined) {
                    var msg = [];

                    var msgPlayers = [];
                    msgPlayers.push("<TABLE  border=\"1\">");
                    msgPlayers.push("<TR>");
                    msgPlayers.push("<TH>bearId</TH>");
                    msgPlayers.push("<TH>bearUsername</TH>");
                    msgPlayers.push("<TH>bearAvatarId</TH>");
                    msgPlayers.push("<TH>mark</TH>");
                    msgPlayers.push("<TH>comment</TH>");
                    msgPlayers.push("<TH colspan=3>actions</TH>");
                    msgPlayers.push("</TR>");
                    for (var i = 0; i < data.players.length; i++) {
                        msgPlayers.push("<TR>");
                        msgPlayers.push("<TD>" + data.players[i].bearId + "</TD>");
                        msgPlayers.push("<TD>" + data.players[i].bearUsername + "</TD>");
                        msgPlayers.push("<TD>" + data.players[i].bearAvatarId + "</TD>");
                        msgPlayers.push("<TD>" + data.players[i].mark + "</TD>");
                        msgPlayers.push("<TD>" + data.players[i].comment + "</TD>");
                        msgPlayers.push("<TD><div href='#' class='kickPlayerBtn' data-id='" + data.players[i].bearId + "' >kickPlayer</div><div id='kickPlayerResult'></div></TD>");
                        //msgPlayers.push("<TD><div href='#' class='markBearBtn' data-id='" + data.players[i].bearId + "' data-version='" + data.players[i].version + "'>mark</div><div id='markBearResult'></div></TD>");
                        //msgPlayers.push("<TD><div href='#' class='commentBearBtn' data-id='" + data.players[i].bearId + "'>comment</div><div id='commentBearResult'></div></TD>");
                        msgPlayers.push("</TR>");
                    }
                    msgPlayers.push("</TABLE>");

                    msg.push("<TABLE  border=\"1\">");
                    msg.push("<TR>");
                    msg.push("<TH>id</TH>");
                    msg.push("<TH>name</TH>");
                    msg.push("<TH>location</TH>");
                    msg.push("<TH>starDate</TH>");
                    msg.push("<TH>players</TH>");
                    msg.push("<TH>nbPlayers</TH>");
                    msg.push("<TH>maxPlayers</TH>");
                    msg.push("</TR>");
                    msg.push("<TR>");
                    msg.push("<TD>" + data.id + "</TD>");
                    msg.push("<TD>" + data.name + "</TD>");
                    msg.push("<TD>" + data.location + "</TD>");
                    msg.push("<TD>" + data.startDate + "</TD>");
                    msg.push("<TD>" + msgPlayers.join('') + "</TD>");
                    msg.push("<TD>" + data.nbPlayers + "</TD>");
                    msg.push("<TD>" + data.maxPlayers + "</TD>");
                    msg.push("</TR>");

                    msg.push("</TABLE>");

                    //after game

                    //room of the game
                    msg.push("<p>");
                    msg.push("rooms detail 'api/rooms/detail' <a href='#' id='RoomBtn' data-id='" + data.id + "' >reload</a><br>");
                    
                    msg.push("<div id='roomDetail'></div>");
                    msg.push("</p>");
                    $("#gameDetail").html(msg.join(''));
                } else {
                    $("#gameDetail").html("no game found");
                }

                $("#RoomBtn").click(function (e) {
                    getDetailRoom($(e.target).data("id"));
                });

                

                //$(".markBearBtn").click(function (e) {
                //    var bearId = $(e.target).data("id");
                //    var mark = 4;
                //    markBear(gameId, bearId, mark).done(function (data) {
                //        $("#markBearResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
                //    });
                //});

                //$(".commentBearBtn").click(function (e) {
                //    var bearId = $(e.target).data("id");
                //    var comment = "someComment" + guid();
                //    commentBear(gameId, bearId, comment).done(function (data) {
                //        $("#commentBearResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
                //    });
                //})

                $(".kickPlayerBtn").click(function (e) {
                    var bearId = $(e.target).data("id");
                    kickPlayer(gameId, gameVersion, bearId).done(function (data) {
                        $("#kickPlayerResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
                    });
                })
                

            })
            .fail(function (err) {
                $("#gameDetail").html(err);
            });
        };

        var getGames = function () {
            $("#gameDetail").html('');
            $.ajax({
                type: "POST",
                url: "api/games/list",
                dataType: "json",
                data: JSON.stringify({
                    "from": "01/01/2001",
                    "to": "01/01/2021"
                })
            })
              .done(function (data) {
                  var msg = [];
                  msg.push(data.length + " games in the database currently");
                  msg.push("<TABLE  border=\"1\">");
                  msg.push("<TR>");
                  msg.push("<TH>id</TH>");
                  msg.push("<TH>name</TH>");
                  msg.push("<TH>location</TH>");
                  msg.push("<TH>startdate</TH>");
                  msg.push("<TH>players</TH>");
                  msg.push("<TH>nbPlayers</TH>");
                  msg.push("<TH>maxPlayers</TH>");
                  msg.push("<TH colspan=4>actions</TH>");
                  msg.push("</TR>");
                  for (var i = 0; i < data.length; i++) {
                      msg.push("<TR>");
                      msg.push("<TD>" + data[i].id + "</TD>");
                      msg.push("<TD>" + data[i].name + "<div href='#' class='changeName' ><img data-id='" + data[i].id + "' data-version='" + data[i].version + "' width=16 height=16 src ='img/edit.png' /></div><div id='changeNameResult'></div></TD>");
                      msg.push("<TD>" + data[i].location + "<div href='#' class='changeLocation' ><img data-id='" + data[i].id + "' data-version='" + data[i].version + "' width=16 height=16 src ='img/edit.png' /></div><div id='changeLocationResult'></div></TD>");
                      msg.push("<TD>" + data[i].startDate + "<div href='#' class='changeStartDate' ><img data-id='" + data[i].id + "' data-version='" + data[i].version + "' width=16 height=16 src ='img/edit.png' /></div><div id='changeStartDateResult'></div></TD>");
                      msg.push("<TD>" + data[i].players + "</TD>");
                      msg.push("<TD>" + data[i].nbPlayers + "</TD>");
                      msg.push("<TD>" + data[i].maxPlayers + "<div href='#' class='changeMaxPlayer' ><img data-id='" + data[i].id + "' data-version='" + data[i].version + "' width=16 height=16 src ='img/edit.png' /></div><div id='changeMaxPlayerResult'></div></TD>");
                      changeMaxPlayer
                      msg.push("<TD><div href='#' class='gameDetailBtn' data-id='" + data[i].id + "' data-version='" + data[i].version + "'>detail</div></TD>");

                      if (data[i].isCancellable)
                          msg.push("<TD><div href='#' class='cancelBtn' data-id='" + data[i].id + "' data-version='" + data[i].version + "'>cancel</div><div id='cancelResult'></div></TD>");
                      else
                          msg.push("<TD>&nbsp;</TD>");

                      if (data[i].isJoinable)
                          msg.push("<TD><div href='#' class='joinBtn' data-id='" + data[i].id + "' data-version='" + data[i].version + "'>join</div><div id='joinResult'></div></TD>");
                      else
                          msg.push("<TD>&nbsp;</TD>");

                      if (data[i].isAbandonnable)
                          msg.push("<TD><div href='#' class='abandonBtn' data-id='" + data[i].id + "' data-version='" + data[i].version + "'>abandon</div><div id='abandonResult'></div></TD>");
                      else
                          msg.push("<TD>&nbsp;</TD>");

                      msg.push("</TR>");
                  };
                  msg.push("</TABLE>");
                  $("#gamesList").html(msg.join(''));

                  $(".gameDetailBtn").click(function (e) {
                      getGame($(e.target).data("id"), $(e.target).data("version"));
                  });

                  var gameAction = function (f, resultDiv) {
                      return function (e) {
                          var gameId = $(e.target).data("id");
                          var version = $(e.target).data("version");
                          f(gameId, version).done(function (data) {
                              $("#" + resultDiv).html("received at " + Date.now() + ", " + JSON.stringify(data));
                          });
                      }
                  }

                  $(".changeName").click(gameAction(changeName, "changeNameResult"));
                  $(".changeStartDate").click(gameAction(changeStartDate, "changeStartDateResult"));
                  $(".changeLocation").click(gameAction(changeLocation, "changeLocationResult"));
                  $(".changeMaxPlayer").click(gameAction(changeMaxPlayer, "changeMaxPlayerResult"));
                  
                  $(".joinBtn").click(gameAction(join, "joinResult"));
                  $(".cancelBtn").click(gameAction(cancel, "cancelResult"));
                  $(".abandonBtn").click(gameAction(abandon, "abandonResult"));
               


              })
              .fail(function (err) {
                  $("#gamesList").html(err);
              });
        };

        var getBear = function (id) {
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





        var getBears = function () {
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
        }



        $("#bearsListBtn").click(getBears);

        $("#gamesListBtn").click(getGames);
        $("#gameScheduleBtn").click(function () {
            var gameBearId = $("#gameBearId").val();
            var gameId = guid();
            var version = 0;
            schedule(gameId, version).done(function (data) {
                $("#scheduleResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
            });
        });

        $("#bearChangePasswordBtn").click(function () {
            var newPassword = "newPassword" + guid();
            changePassword(newPassword).done(function (data) {
                $("#changePasswordResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
            });
        });

        $("#bearChangeUsernameBtn").click(function () {
            var newUserName = "newUserName" + guid();
            changeUserName(newUserName).done(function (data) {
                $("#changeUsernameResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
            });
        });

        var i = 1;
        $("#bearChangeAvatarIdBtn").click(function () {

            changeAvatarId(i++).done(function (data) {
                $("#changeAvatarIdResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
            });
        });







        //extract the detail of the current bear
        $.ajax({
            type: "GET",
            url: "api/bears/current",
            dataType: "json"
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
                $("#currentBearDetail").html(msg.join(''));
            } else {
                $("#currentBearDetail").html("no current bear found");
            }
        })
        .fail(function (err) {
            $("#bearDetail").html(err);
        });




    });

})(jQuery)

