

this.PetulantBear = this.PetulantBear || {};

//APP
(function ($, bears, cleaveage, room) {

    var self = this;

    this.schedule = function (id, version) {
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

    this.cancel = function (id, version) {
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


    this.join = function (id, version) {
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

    this.changeName = function (id, version) {
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
    this.changeStartDate = function (id, version) {
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
    this.changeLocation = function (id, version) {
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
    this.changeMaxPlayer = function (id, version) {
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
    this.abandon = function (id, version) {
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

    this.kickPlayer = function (gameId, version, bearId) {
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


    this.getGame = function (gameId, gameVersion) {
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
                msg.push("<div id='cleaveageDetail'></div>")
                msg.push("<div id='roomDetail'></div>");

                msg.push("</p>");
                $("#gameDetail").html(msg.join(''));

                //retrieve the cleaveage associated with this game
                cleaveage.getCleaveages(data.id);
            } else {
                $("#gameDetail").html("no game found");
            }

            $("#RoomBtn").click(function (e) {
                room.getDetailRoom($(e.target).data("id"));
            });


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

    this.getGames = function () {
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
                  self.getGame($(e.target).data("id"), $(e.target).data("version"));
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

              $(".changeName").click(gameAction(self.changeName, "changeNameResult"));
              $(".changeStartDate").click(gameAction(self.changeStartDate, "changeStartDateResult"));
              $(".changeLocation").click(gameAction(self.changeLocation, "changeLocationResult"));
              $(".changeMaxPlayer").click(gameAction(self.changeMaxPlayer, "changeMaxPlayerResult"));

              $(".joinBtn").click(gameAction(self.join, "joinResult"));
              $(".cancelBtn").click(gameAction(self.cancel, "cancelResult"));
              $(".abandonBtn").click(gameAction(self.abandon, "abandonResult"));



          })
          .fail(function (err) {
              $("#gamesList").html(err);
          });
    };



    this.init = function () {
        $("#gamesListBtn").click(self.getGames);
        $("#gameScheduleBtn").click(function () {
            var gameBearId = $("#gameBearId").val();
            var gameId = guid();
            var version = 0;
            self.schedule(gameId, version).done(function (data) {
                $("#scheduleResult").html("received at " + Date.now() + ", " + JSON.stringify(data));
            });
        });
    }







}).call(this.PetulantBear.Games || (this.PetulantBear.Games = {}), jQuery, this.PetulantBear.Bears, this.PetulantBear.Cleavage, this.PetulantBear.Room);

//this.PetulantBear.App = app.call({}, jQuery)



