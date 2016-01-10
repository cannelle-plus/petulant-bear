// JavaScript source code

this.PetulantBear = this.PetulantBear || {};

(function ($, bears) {



    //let propose = "/api/cleaveage/propose"
    //let joinTeam = "/api/cleaveage/joinTeam"
    //let leaveTeam = "/api/cleaveage/leaveTeam"
    //let kickPlayerFromTeam = "/api/cleaveage/kickPlayerFromTeam"
    //let switchPlayer = "/api/cleaveage/switchPlayer"
    //let changeName = "/api/cleaveage/changeName"
    //let list = "/api/cleaveage/list"
    //let detail = "/api/cleaveage/detail"
    var self = this;

    var getClassBtnAction = function (name) {
        return name + "Btn";
    }

    var createTeamActionLink = function (predicate, name, cleaveage, team) {
        if (predicate)
            return "<div href='#' class='" + getClassBtnAction(name) + "' data-id='" + cleaveage.cleaveageId + "' data-version='" + cleaveage.version + "' data-teamid='" + team.teamId + "'>" + name + "</div><div id='" + name + "Result'></div>"
        else
            return "";
    };

    var createTeamPlayerActionLink = function (predicate, name, cleaveage, team, player) {
        if (predicate)
            return "<div href='#' class='" + getClassBtnAction(name) + "' data-id='" + cleaveage.cleaveageId + "' data-version='" + cleaveage.version + "' data-teamid='" + team.teamId + "'  data-bearid='" + player.bearId + "'>" + name + "</div><div id='" + name + "Result'></div>"
        else
            return "";
    };

    var subscribeToTeamActionLink = function (name) {

        var f = function (id, version, teamId) {
            var actionCmd = createCommand(id, version, {
                "teamId": teamId
            });
            return $.ajax({
                type: "POST",
                url: "api/cleaveage/" + name + "Team",
                dataType: "json",
                data: JSON.stringify(actionCmd)
            })
              .fail(function (err) {
                  $("#" + name + "Result").html(err);
              });
        }

        return function () {
            $("." + getClassBtnAction(name)).click(function (e) {
                var cleaveageId = $(e.target).data("id");
                var version = $(e.target).data("version");
                var teamid = $(e.target).data("teamid");
                f(cleaveageId, version, teamid);
            });
        }
    };

    var subscribeTokickPlayerFromTeamLink = function (cleaveageId) {
        var name = "kickPlayerFromTeam";

        var f = function (id, version, teamId, playerId) {
            var actionCmd = createCommand(id, version, {
                "teamId": teamId,
                "playerId": playerId
            });
            return $.ajax({
                type: "POST",
                url: "api/cleaveage/" + name,
                dataType: "json",
                data: JSON.stringify(actionCmd)
            })
              .fail(function (err) {
                  $("#" + name + "Result").html(err);
              });
        }

        return function () {
            $("." + getClassBtnAction(name) + "[data-id='" + cleaveageId + "']").click(function (e) {
                var cleaveageId = $(e.target).data("id");
                var version = $(e.target).data("version");
                var teamid = $(e.target).data("teamid");
                var playerId = $(e.target).data("bearid");
                f(cleaveageId, version, teamid, playerId);
            });
        }


    };

    var subscribeToSwitchPlayerLink = function (cleaveageId, teamIdA, teamIdB) {
        var name = "switchPlayer";

        var f = function (id, version, fromTeamId, playerId) {
            toTeamId = (fromTeamId == teamIdA) ? teamIdB : teamIdA;
            var actionCmd = createCommand(id,  version, {
                "fromTeamId": fromTeamId,
                "toTeamId": toTeamId,
                "playerId": playerId
            });
            return $.ajax({
                type: "POST",
                url: "api/cleaveage/" + name,
                dataType: "json",
                data: JSON.stringify(actionCmd)
            })
              .fail(function (err) {
                  $("#" + name + "Result").html(err);
              });
        }

        return function () {
            $("." + getClassBtnAction(name) + "[data-id='" + cleaveageId + "']").click(function (e) {
                var cleaveageId = $(e.target).data("id");
                var version = $(e.target).data("version");
                var teamid = $(e.target).data("teamid");
                var playerId = $(e.target).data("bearid");
                f(cleaveageId, version, teamid, playerId);
            });
        }


    };

    var subscribeTochangeNameLink = function (newName) {
        var name = "changeName";
        var f = function (id, version, teamId) {
            var actionCmd = createCommand(id, version, {
                "teamId": teamId,
                "nameTeam": newName
            });
            return $.ajax({
                type: "POST",
                url: "api/cleaveage/" + name + "Team",
                dataType: "json",
                data: JSON.stringify(actionCmd)
            })
              .fail(function (err) {
                  $("#" + name + "Result").html(err);
              });
        }

        return function () {
            $("." + getClassBtnAction(name)).click(function (e) {
                var cleaveageId = $(e.target).data("id");
                var version = $(e.target).data("version");
                var teamid = $(e.target).data("teamid");
                f(cleaveageId, version, teamid);
            });
        }
    };

    this.proposeCleaveage = function (id, version, gameId, nameTeamA, nameTeamB) {

        //{
        //    gameId: Guid;
        //    nameTeamA: string;
        //    nameTeamB: string;
        //}

        var proposeCmd = createCommand(id, version, {
            "gameId": gameId,
            "nameTeamA": nameTeamA,
            "nameTeamB": nameTeamB
        });
        return $.ajax({
            type: "POST",
            url: "api/cleaveage/propose",
            dataType: "json",
            data: JSON.stringify(proposeCmd)
        })
          .fail(function (err) {
              $("#proposeCleavageResult").html(err);
          });
    }

    this.getCleaveages = function (gameRoomId) {
        $.ajax({
            type: "POST",
            url: "/api/cleaveage/list",
            dataType: "json",
            data: JSON.stringify({
                gameId: gameRoomId
            })
        })
          .done(function (data) {
              var currentBear = bears.getcurrentBear();
              var msg = [];
              var subscriptions = [];

              msg.push("<a href='#' id='proposeCleaveBtn' data-gameId='" + gameRoomId + "'>propose cleaveage</a></div><div id='proposeCleavageResult'>");
              if (data !== null && data !== undefined && data.length) {

                  msg.push("<TABLE  border=\"1\">");
                  msg.push("<TR>");
                  msg.push("<TH>cleaveageId</TH>");
                  msg.push("<TH>version</TH>");
                  msg.push("<TH>gameId</TH>");
                  msg.push("<TH>isOpenned</TH>");
                  msg.push("<TH>team A</TH>");
                  msg.push("<TH>team B</TH>");
                  msg.push("</TR>");
                  for (var c = 0; c < data.length; c++) {
                      var cleaveage = data[c];

                      var isPartOfTeam = function (bearId) {
                          return function (player) {
                              return player.bearId === bearId;
                          }
                      }
                      var isCurrentBearPartofTeamA = cleaveage.teamA.players.some(isPartOfTeam(currentBear.bearId));
                      var isCurrentBearPartofTeamB = cleaveage.teamB.players.some(isPartOfTeam(currentBear.bearId));

                      msg.push("<TR>");
                      msg.push("<TD>" + cleaveage.cleaveageId + "</TD>");
                      msg.push("<TD>" + cleaveage.version + "</TD>");
                      msg.push("<TD>" + cleaveage.gameId + "</TD>");
                      msg.push("<TD>" + cleaveage.isOpenned + "</TD>");
                      //team A
                      msg.push("<TD>");
                      msg.push("<TABLE  border=\"1\">");
                      msg.push("<TR>");
                      msg.push("<TH>teamId</TH>");
                      msg.push("<TH>name</TH>");
                      msg.push("<TH colspan=3>actions</TH>");
                      msg.push("<TH>players</TH>");
                      msg.push("</TR>");
                      msg.push("<TR>");
                      msg.push("<TD>" + cleaveage.teamA.teamId + "</TD>");
                      msg.push("<TD>" + cleaveage.teamA.name + "</TD>");
                      msg.push("<TD>" + createTeamActionLink(true, "changeName", cleaveage, cleaveage.teamA) + "</TD>");
                      msg.push("<TD>" + createTeamActionLink(!isCurrentBearPartofTeamA, "join", cleaveage, cleaveage.teamA) + "</TD>");
                      msg.push("<TD>" + createTeamActionLink(isCurrentBearPartofTeamA, "leave", cleaveage, cleaveage.teamA) + "</TD>");
                      msg.push("<TD>");
                      msg.push("<TABLE  border=\"1\">");
                      msg.push("<TR>");
                      msg.push("<TH>bearId</TH>");
                      msg.push("<TH>bearUsername</TH>");
                      msg.push("<TH>bearAvatarId</TH>");
                      msg.push("<TH colspan=2>actions</TH>");
                      msg.push("</TR>");
                      for (var i = 0; i < cleaveage.teamA.players.length; i++) {

                          var player = cleaveage.teamA.players[i];
                          msg.push("<TR>");
                          msg.push("<TD>" + player.bearId + "</TD>");
                          msg.push("<TD>" + player.bearUsername + "</TD>");
                          msg.push("<TD>" + player.bearAvatarId + "</TD>");
                          msg.push("<TD>" + createTeamPlayerActionLink(true, "kickPlayerFromTeam", cleaveage, cleaveage.teamA, player) + "</TD>");
                          msg.push("<TD>" + createTeamPlayerActionLink(true, "switchPlayer", cleaveage, cleaveage.teamA, player) + "</TD>");
                          msg.push("</TR>");
                      };
                      msg.push("</TABLE>"); //end of players
                      msg.push("</TD>");
                      msg.push("</TR>");
                      msg.push("</TABLE>"); //end of team A
                      msg.push("</TD>");
                      //team B
                      msg.push("<TD>");
                      msg.push("<TABLE  border=\"1\">");
                      msg.push("<TR>");
                      msg.push("<TH>teamId</TH>");
                      msg.push("<TH>name</TH>");
                      msg.push("<TH colspan=3>actions</TH>");
                      msg.push("<TH>players</TH>");
                      msg.push("</TR>");
                      msg.push("<TR>");
                      msg.push("<TD>" + cleaveage.teamB.teamId + "</TD>");
                      msg.push("<TD>" + cleaveage.teamB.name + "</TD>");
                      msg.push("<TD>" + createTeamActionLink(true, "changeName", cleaveage, cleaveage.teamB) + "</TD>");
                      msg.push("<TD>" + createTeamActionLink(!isCurrentBearPartofTeamB, "join", cleaveage, cleaveage.teamB) + "</TD>");
                      msg.push("<TD>" + createTeamActionLink(isCurrentBearPartofTeamB, "leave", cleaveage, cleaveage.teamB) + "</TD>");
                      msg.push("<TD>");
                      msg.push("<TABLE  border=\"1\">");
                      msg.push("<TR>");
                      msg.push("<TH>bearId</TH>");
                      msg.push("<TH>bearUsername</TH>");
                      msg.push("<TH>bearAvatarId</TH>");
                      msg.push("<TH colspan=2>actions</TH>");
                      msg.push("</TR>");
                      for (var i = 0; i < cleaveage.teamB.players.length; i++) {
                          var player = cleaveage.teamB.players[i];
                          msg.push("<TR>");
                          msg.push("<TD>" + player.bearId + "</TD>");
                          msg.push("<TD>" + player.bearUsername + "</TD>");
                          msg.push("<TD>" + player.bearAvatarId + "</TD>");
                          msg.push("<TD>" + createTeamPlayerActionLink(true, "kickPlayerFromTeam", cleaveage, cleaveage.teamA, player) + "</TD>");
                          msg.push("<TD>" + createTeamPlayerActionLink(true, "switchPlayer", cleaveage, cleaveage.teamA, player) + "</TD>");
                          msg.push("</TR>");
                      };
                      msg.push("</TABLE>"); //end of players
                      msg.push("</TD>");
                      msg.push("</TR>");
                      msg.push("</TABLE>"); //end of team B
                      msg.push("</TD>");
                      msg.push("</TR>");

                      // one subscription by cleaveage to get the context of every call, could be replaced aby a genreic solution once a local db is used
                      subscriptions.push(subscribeTokickPlayerFromTeamLink(cleaveage.cleaveageId));
                      subscriptions.push(subscribeToSwitchPlayerLink(cleaveage.cleaveageId, cleaveage.teamA.teamId, cleaveage.teamB.teamId));
                  }

                  msg.push("</TABLE>");

                  subscriptions.push(subscribeToTeamActionLink("join"));
                  subscriptions.push(subscribeToTeamActionLink("leave"));
                  subscriptions.push(subscribeTochangeNameLink("newName " + guid()));

              }
              else {
                  msg.push("no cleaveage found");
              }
              $("#cleaveageDetail").html(msg.join(''));

              //Now that the html is aprt of the dom, we can subscribe to every link
              //we could have also targeted a higher container instead
              for (var i = 0; i < subscriptions.length; i++) subscriptions[i]();

              $("#proposeCleaveBtn").click(function (e) {
                  var id = guid();
                  var version = 0; //new aggregate
                  var gameId = $(e.target).data("gameid"); //the game this cleaveage is related to
                  var nameTeamA = "team A -" + guid();
                  var nameTeamB = "team B -" + guid();
                  self.proposeCleaveage(id, version, gameId, nameTeamA, nameTeamB);
              });


          })
          .fail(function (err) {
              $("#roomDetail").html(err);
          });
    };

}).call(this.PetulantBear.Cleavage || (this.PetulantBear.Cleavage = {}), jQuery, this.PetulantBear.Bears);