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
     

        var schedule = function (id) {
            var scheduleCmd = createCommand(id, 1, {
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

        var cancel = function (id) {
            var cancelCmd = createCommand(id, 1, { });
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


        var join = function (id) {
            var joinCmd = createCommand(id, 1, {});
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

        var abandon = function (id) {
            var abandonCmd = createCommand(id, 1, { });
            return $.ajax({
                type: "POST",
                url: "api/games/abandon",
                dataType: "json",
                data: JSON.stringify(abandonCmd)
            })
              .fail(function (err) {
                  $("#abandonResult").html(err);
              });
        }

        var markBear = function (gameId,  mark) {
            var markBearCmd = createCommand(gameId, 1, {
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

        var commentBear = function (gameId,  comment) {
            var commentBearCmd = createCommand(gameId, 1, {
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
        }

        var signalSent = function (transmitterId, receiverId, signalStrength, receptionDate) {
            var signalsSent = createCommand(transmitterId, 1, {
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

        var startCalibration = function (transmitterId, receiverId, distance) {
            var startCalibrationCmd = createCommand(transmitterId, 1, {
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

        var stopCalibration = function (transmitterId, receiverId, distance) {
            var stopCalibrationCmd = createCommand(transmitterId, 1, {
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



        var postMessageToRoom = function (roomId,  message) {
            var postMessageCmd = createCommand(roomId, 1, {
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
              })
              .fail(function (err) {
                  $("#roomDetail").html(err);
              });
        };


        var getGame = function (gameId) {
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
                      msgPlayers.push("</TR>");
                      for (var i = 0; i < data.players.length; i++) {
                          msgPlayers.push("<TR>");
                          msgPlayers.push("<TD>" + data.players[i].bearId + "</TD>");
                          msgPlayers.push("<TD>" + data.players[i].bearUsername + "</TD>");
                          msgPlayers.push("<TD>" + data.players[i].bearAvatarId + "</TD>");
                          msgPlayers.push("</TR>");
                      }
                      msgPlayers.push("</TABLE>");

                      msg.push("<TABLE  border=\"1\">");
                      msg.push("<TR>");
                      msg.push("<TH>id</TH>");
                      msg.push("<TH>name</TH>");
                      msg.push("<TH>location</TH>");
                      msg.push("<TH>starTHate</TH>");
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

                      //room of the game
                      msg.push("<p>");
                      msg.push("rooms detail 'api/rooms/detail' <a href='#' id='RoomBtn' data-id='" + data.id + "'>reload</a><br>");
                      msg.push("postMessage <input id='msgRoom' type=text > <a id='postMessageToRoomBtn' href='#'  data-id='" + data.id + "'> postMessageToRoom </a><div id='postMessageToRoomResult'></div><br>");
                      msg.push("<div id='roomDetail'></div>");
                      msg.push("</p>");
                      $("#gameDetail").html(msg.join(''));
                  } else {
                      $("#gameDetail").html("no game found");
                  }

                  $("#RoomBtn").click(function (e) {
                      getDetailRoom($(e.target).data("id"));
                  });

                  $("#postMessageToRoomBtn").click(function (e) {
                      var roomId = $(e.target).data("id");
                      var message = $("#msgRoom").val();
                      postMessageToRoom(roomId,  message);
                  });

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
                      msg.push("<TD>" + data[i].name + "</TD>");
                      msg.push("<TD>" + data[i].location + "</TD>");
                      msg.push("<TD>" + data[i].startDate + "</TD>");
                      msg.push("<TD>" + data[i].players + "</TD>");
                      msg.push("<TD>" + data[i].nbPlayers + "</TD>");
                      msg.push("<TD>" + data[i].maxPlayers + "</TD>");
                      msg.push("<TD><div href='#' class='gameDetailBtn' data-id='" + data[i].id + "'>detail</div></TD>");

                      if (data[i].isCancellable)
                          msg.push("<TD><div href='#' class='cancelBtn' data-id='" + data[i].id + "'>cancel</div><div id='cancelResult'></div></TD>");
                      else
                          msg.push("<TD>&nbsp;</TD>");

                      if (data[i].isJoinable)
                          msg.push("<TD><div href='#' class='joinBtn' data-id='" + data[i].id + "'>join</div><div id='joinResult'></div></TD>");
                      else
                          msg.push("<TD>&nbsp;</TD>");

                      if (data[i].isAbandonnable)
                          msg.push("<TD><div href='#' class='abandonBtn' data-id='" + data[i].id + "'>abandon</div><div id='abandonResult'></div></TD>");
                      else
                          msg.push("<TD>&nbsp;</TD>");
                      
                      //these should on the bears in games
                      //msg.push("<TD><div href='#' class='markBtn' data-id='" + data[i].id + "'>mark</div><div id='markResult'></div></TD>");
                      //msg.push("<TD><div href='#' class='commentBtn' data-id='" + data[i].id + "'>comment</div><div id='commentResult'></div></TD>");
                      msg.push("</TR>");
                  };
                  msg.push("</TABLE>");
                  $("#gamesList").html(msg.join(''));

                  $(".gameDetailBtn").click(function (e) {
                      getGame($(e.target).data("id"));
                  });

                  var gameAction = function (f, resultDiv) {
                      return function (e) {
                          var gameId = $(e.target).data("id");
                          f(gameId).done(function (data) {
                              $("#" + resultDiv).html("received at " + Date.now() + ", " + JSON.stringify(data));
                          });
                      }
                  }

                  $(".joinBtn").click(gameAction(join, "joinResult"));
                  $(".cancelBtn").click(gameAction(cancel, "cancelResult"));
                  $(".abandonBtn").click(gameAction(abandon, "abandonResult"));
                  $(".markBtn").click(function (e) {
                      var gameId = $(e.target).data("id");
                      markBear(gameId,7).done(function (data) {
                          $("#markResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
                      })
                  });
                  $(".commentBtn").click(function (e) {
                        var gameId = $(e.target).data("id");
                        commentBear(gameId,"some comment" +guid()).done(function (data) {
                            $("#commentResult" ).html("received at " + Date.now() + ", " + JSON.stringify(data));
                        });
                  });
                  

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


     


        var guid = function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                  v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }


        $("#bearsListBtn").click(getBears);
        


        $("#gamesListBtn").click(getGames);
        $("#gameScheduleBtn").click(function () {
            var gameBearId = $("#gameBearId").val();
            var gameId = guid();
            schedule(gameId, gameBearId).done(function (data) {
                $("#scheduleResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
            });
        });



        








    });

})(jQuery)






























//var bearTests = [

//    function We_signin_a_new_bear() {
//        //test signin command
//        var bearId = guid();
//        var socialId = guid();
//        var bearUsername = "bear_"+guid();
//        signin(bearId,socialId,bearUsername).done(function(data){
//              $("#signinResult").append(JSON.stringify(data));
//        });
//    }

//]

//var GivenASignedBear = function(){
//  var bearId = guid();
//  var socialId = guid();
//  var bearUsername = "bear_"+guid();

//  var deferred = $.Deferred();
//  signin(bearId,socialId,bearUsername)
//  .done(function(data){
//    deferred.resolve(bearId,socialId,bearUsername);
//  })
//  return deferred.promise();
//}

//var gameTests = [


//    function Given_a_signed_in_bear_We_can_fetch_games_list() {

//        GivenASignedBear()
//        .done(function(bearId,socialId,bearUsername){
//          fetchGamesList();
//        });
//    },

//    function Given_a_signed_in_bear_We_can_fetch_details_of_a_game() {

//        GivenASignedBear()
//        .done(function(bearId,socialId,bearUsername){
//          fetchGameDetail();
//        });
//    },



//    function Given_a_signed_in_bear_We_schedule_a_new_game() {

//        GivenASignedBear()
//        .done(function(bearId,socialId,bearUsername){
//          //test schedule command
//          var id = guid();
//          schedule(id, bearId)
//          .done(function (data) {
//            $("#scheduleResult").append(JSON.stringify(data));
//          })

//        })



//    },

//    function Given_a_scheduled_game_we_cancel_it() {
//        //test cancel command
//        var id = guid();
//        var bearId = guid();

//        schedule(id, bearId)
//        .done(function (data) {
//            cancel(id, bearId)
//            .done(function (data) {
//                $("#cancelResult").append(JSON.stringify(data));
//            });
//        });
//    },

//    function Given_a_scheduled_game_we_abandon_it() {
//        //test cancel command
//        var id = guid();
//        var bearId = guid();

//        schedule(id, bearId)
//        .done(function (data) {
//            abandon(id, bearId)
//            .done(function (data) {
//                $("#abandonResult").append(JSON.stringify(data));
//            });
//        });
//    },

//    function Given_a_scheduled_game_we_mark_a_bear() {
//        //test markBear command
//        var gameId = guid();
//        var bearId = guid();
//        var mark = 7;

//        schedule(gameId, bearId)
//        .done(function (data) {
//            markBear(gameId, bearId, mark)
//            .done(function (data) {
//                $("#markBearResult").append(JSON.stringify(data));
//            });
//        });
//    },

//    function Given_a_scheduled_game_we_comment_a_bear() {
//        //test markBear command
//        var gameId = guid();
//        var bearId = guid();
//        var comment = "some somment";

//        schedule(gameId, bearId)
//        .done(function (data) {
//            commentBear(gameId, bearId, comment)
//            .done(function (data) {
//                $("#commentBearResult").append(JSON.stringify(data));
//            });
//        });
//    },

//];

//var roomTests = [

//    function Given_a_signed_in_bear_it_can_fetch_rooms_detail() {
//      GivenASignedBear()
//      .done(function(bearId,socialId,bearUsername){
//        fetchDetailRooms()
//      });
//    },

//    function Given_a_signed_in_bear_and_a_scheduled_game_We_post_a_message_in_the_room() {

//        GivenASignedBear()
//        .done(function(bearId,socialId,bearUsername){
//          //test schedule command
//          var gameId = guid();
//          schedule(gameId, bearId)
//          .done(function (data) {
//            var roomId = gameId;
//            var message = "message aleatoire - " + guid();
//            postMessageToRoom(roomId,bearId, message)
//            .done(function(data){
//              $("#postMessageResult").append(JSON.stringify(data));
//            });

//          });
//        });
//    },
//];


//var signalTests = [

//    function Given_a_active_receiver_We_can_receive_a_signal() {
//      //test schedule command
//      var transmitterId = guid();
//      var receiverId = guid();
//      var signalStrength = 15;
//      var receptionDate = new Date();
//      signalSent(transmitterId, receiverId,signalStrength,receptionDate)
//      .done(function (data) {
//        $("#signalsReceivedResult").append(JSON.stringify(data));
//      });


//    },
//     function Given_a_active_receiver_We_can_start_its_calibration() {
//      //test schedule command
//      var transmitterId = guid();
//      var receiverId = guid();
//      var distance = 2;
//      startCalibration(transmitterId, receiverId,distance)
//      .done(function (data) {
//        $("#startCalibrationResult").append(JSON.stringify(data));
//      });
//    },
//    function Given_a_active_receiver_We_can_stop_its_calibration() {
//      //test schedule command
//      var transmitterId = guid();
//      var receiverId = guid();
//      var distance = 2;
//      stopCalibration(transmitterId, receiverId,distance)
//      .done(function (data) {
//        $("#stopCalibrationResult").append(JSON.stringify(data));
//      });
//    }
//];

//run the tests
//for (var i = 0; i < bearTests.length; i++) bearTests[i]();
//for (var i = 0; i < gameTests.length; i++) gameTests[i]();
//for (var i = 0; i < roomTests.length; i++) roomTests[i]();

//for (var i = 0; i < signalTests.length; i++) signalTests[i]();
