Date.prototype.toMSJSON = function () {
    var date = '/Date(' + this.getTime() + ')/'; //CHANGED LINE
    return date;
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
                version: version,
                payLoad: payLoad
            };
        }

        
        var schedule = function (id) {
            var scheduleCmd = createCommand(id, 1, {
                "location": $('#gameLocation').val(),
                "maxPlayers": $('#nbPlayersRequired').val(),
                "name": $('#gameName').val(),
                "startDate": new Date($('#gameDate').val() + ' ' + $('#gameHour').val()).toMSJSON()
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

        //afterGame
        var markBear = function (gameId, bearId,  mark) {
            var markBearCmd = createCommand(gameId, 1, {
                "bearId" : bearId,
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

        var commentBear = function (gameId, bearId,  comment) {
            var commentBearCmd = createCommand(gameId, 1, {
                "bearId" : bearId,
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

        var postMessageToRoom = function (roomId,  message, gameId) {
            var postMessageCmd = createCommand(roomId, 1, {
                "message": message
            });
            if(message !== "") {
              return $.ajax({
                type: "POST",
                url: "api/rooms/postmessage",
                dataType: "json",
                data: JSON.stringify(postMessageCmd)
              })
              .done(function(data){                                
                getDetailRoom(roomId);
              })
              .fail(function (err) {
                  $("#postMessageToRoomResult").html(err);
              });
            }
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
                      msg.push("<dl class='chat'>");
                      //msg.push("<TD>" + data.roomId + "</TD>");
                      //msg.push("<TD>" + data.name + "</TD>");
                      for (var i = data.messages.length - 1; i >= ((data.messages.length < 10) ? 0 : data.messages.length - 9); i--) {

                          //msg.push("<TD>" + data.messages[i].bear.bearId + "</TD>");
                          msg.push("<dt>" + data.messages[i].bear.bearUsername + "</dt>");
                          //msg.push("<TD>" + data.messages[i].bear.socialId + "</TD>");
                          //msg.push("<TD>" + data.messages[i].bear.bearAvatarId + "</TD>");
                          msg.push("<dd>" + data.messages[i].message + "</dd>");
                      };

                      msg.push("</dl>");

                      $(".roomDetail").html(msg.join(''));
                  }
                  else {
                      $(".roomDetail").html("room not found");
                  }
              })
              .fail(function (err) {
                  $(".roomDetail").html(err);
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

                    //msgPlayers.push('');
                    msg.push('<ul class="game-detail"><li data-id="' + data.id + '">');
                    msg.push('<p class="infos">');
                    msg.push('<strong>' + data.name + '</strong>');
                    msg.push(data.location + '<br />');
                    msg.push(data.startDate);

                    var coloredClass = '';
                    if(data.nbPlayers/data.maxPlayers < .6)
                      coloredClass = 'alert';
                    else if(data.nbPlayers/data.maxPlayers < .8)
                      coloredClass = 'warning';
                    else
                      coloredClass = 'success';

                    msg.push('<p class="state ' + coloredClass + '">' + data.nbPlayers + '/' + data.maxPlayers + '</p>');
                    msg.push('</li><li class="clearfix"><ul>');

                    //msg.push("<TD>" + data.startDate + "</TD>");
                    //msg.push("<TD>" + msgPlayers.join('') + "</TD>");
                    for (var i = 0; i < data.players.length; i++) {
                        msg.push('<li data-id=" ' + data.players[i].bearId + '">');
                        msg.push(data.players[i].bearUsername);
                        //msgPlayers.push("<TD>" + data.players[i].bearAvatarId + "</TD>");
                        //msgPlayers.push("<TD>" + data.players[i].mark + "</TD>");
                        //msgPlayers.push("<TD>" + data.players[i].comment + "</TD>");
                        //msgPlayers.push("<TD><div href='#' class='markBearBtn' data-id='" + data.players[i].bearId + "'>mark</div><div id='markBearResult'></div></TD>");
                        //msgPlayers.push("<TD><div href='#' class='commentBearBtn' data-id='" + data.players[i].bearId + "'>comment</div><div id='commentBearResult'></div></TD>");
                        msg.push('</li>');
                    }                    

                    msg.push('</ul></li>');

                    if (data.isJoinable)
                      msg.push('<li class="detailAction infos"><a href="#" class="actionJoin button button-small" data-id="' + data.id + '">Rejoindre</a></li>');
                    else if (data.isAbandonnable)
                      msg.push('<li class="detailAction infos"><a href="#" class="actionAbandonGame button button-small" data-id="' + data.id + '">Quitter</a></li>');
                    
                    msg.push('</ul>');


                    //room of the game
                    msg.push("<a href='#' class='roomDetailButton' data-target='room' data-id='" + data.id + "'><img src='images/chat-icon.png' width='20' height='20' /> Chat</a>")
                    msg.push("<div class='roomDetail'></div>");
                    $("#gameDetailled").html(msg.join(''));
                    $("#chat .infos strong").html(data.name);
                    getDetailRoom(data.id);
                    bearInterval = setInterval(function() {
                      getDetailRoom(data.id);
                    }, 30000);
                } else {
                    $("#gameDetailled").html("no game found");
                }

                $("#chat").on('submit', function (e) {
                    doNothing(e);
                    var roomId = $(e.target).data("id");
                    var message = $("#msgRoom").val();
                    $("#msgRoom").val('');
                    if(message !== '')
                      postMessageToRoom(roomId,  message, data.id);    
                });

                $('body').on('click', '.roomDetailButton', function() {
                  $('#chat').data('id', $(this).data('id'));
                });

                $(".markBearBtn").click(function (e) {
                    var bearId = $(e.target).data("id");
                    var mark = 4;
                    markBear(gameId, bearId, mark);
                });

                $(".commentBearBtn").click(function (e) {
                    var bearId = $(e.target).data("id");
                    var comment = "someComment" + guid();
                    commentBear(gameId, bearId,comment);
                });

                var gameAction = function (f, resultDiv) {
                    return function (e) {
                        doNothing(e);
                        var gameId = $(e.target).data("id");
                        $(e.target).closest('.action').hide();
                        f(gameId).done(function (data) {
                            $("#" + resultDiv).html("received at " + Date.now() + ", " + JSON.stringify(data));
                            getGames();
                            getGame($(e.target).data("id"));
                        });
                    }
                }
                $(".actionJoin").click(gameAction(join, "joinResult"));
                $(".actionCancelGame").click(gameAction(cancel, "cancelResult"));
                $(".actionAbandonGame").click(gameAction(abandon, "abandonResult"));
                  

            })
            .fail(function (err) {
                $("#gameDetailxxx").html(err);
            });
        };

        var getGames = function () {
            $("#gameDetailxxx").html('');
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
                  msg.push('<ul id="gamesListContainer" class="games">');
                  for (var i = 0; i < data.length; i++) {
                      addOwnerAvatar(data[i].ownerId, i);
                      msg.push('<li data-id="' + data[i].id + '">');
                      msg.push('<p class="avatar"><img src="images/avatar.png" alt="" height="25" width="25"></p>');
                      msg.push('<p class="infos"><strong>' + data[i].name + '</strong><span>'+ data[i].location +'</span></p>');

                      var coloredClass = '';
                      if(data[i].nbPlayers/data[i].maxPlayers < .6)
                        coloredClass = 'alert';
                      else if(data[i].nbPlayers/data[i].maxPlayers < .8)
                        coloredClass = 'warning';
                      else
                        coloredClass = 'success';

                      msg.push('<p class="number ' + coloredClass + '"><span class="nbPlayers">'+ data[i].nbPlayers +'</span><span>/</span><span class="maxPlayers">' + data[i].maxPlayers + '</span></p>');
                      msg.push('<p class="more"><a href="#" data-id="' + data[i].id + '">+</a></p>');

                      if(data[i].isCancellable || data[i].isJoinable || data[i].isAbandonnable) {
                        msg.push('<div class="action"><p>Voulez-vous ?</p>');

                        //msg.push("<TD>" + data[i].startDate + "</TD>");
                        //msg.push("<TD>" + data[i].players + "</TD>");


                        if (data[i].isJoinable)
                            msg.push('<button type="button" class="actionJoin" data-id="' + data[i].id + '">Rejoindre</button>');
                        else
                            msg.push('&nbsp;');

                        if (data[i].isAbandonnable)
                            msg.push('<button type="button" class="actionAbandonGame" data-id="' + data[i].id + '">Quitter</button>');
                        else
                            msg.push('&nbsp;');
                        if (data[i].isCancellable)
                            msg.push('<button type="button" class="actionCancelGame" data-id="' + data[i].id + '">Supprimer</button>');
                        else
                            msg.push('&nbsp;');
                        
                        //these should on the bears in games
                        //msg.push("<TD><div href='#' class='markBtn' data-id='" + data[i].id + "'>mark</div><div id='markResult'></div></TD>");
                        //msg.push("<TD><div href='#' class='commentBtn' data-id='" + data[i].id + "'>comment</div><div id='commentResult'></div></TD>");
                        msg.push("</div>");
                      }
                      msg.push("</li>");
                  };
                  msg.push("</ul>");
                  $("#gamesList").html(msg.join(''));

                  $(".games li").click(function (e) {
                    doNothing(e);
                    var _id = (e.target.nodeName === 'LI' || e.target.nodeName === 'li') ? $(e.target).data("id") : $(e.target).closest('li').data("id")
                    getGame(_id);
                    showSection('gameDetail');                    
                    history.pushState(null, null, '#_gameDetail'); 
                  });
                  $(".games .action").click(function (e) {
                    doNothing(e);
                    $(this).hide();
                  });

                  var gameAction = function (f, resultDiv) {
                      return function (e) {
                          doNothing(e);
                          var gameId = $(e.target).data("id");
                          $(e.target).closest('.action').hide();
                          f(gameId).done(function (data) {
                              $("#" + resultDiv).html("received at " + Date.now() + ", " + JSON.stringify(data));
                              getGames();
                          });
                      }
                  }

                  $(".actionJoin").click(gameAction(join, "joinResult"));
                  $(".actionCancelGame").click(gameAction(cancel, "cancelResult"));
                  $(".actionAbandonGame").click(gameAction(abandon, "abandonResult"));
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
                  $('.more a').on('click', function(e){
                    doNothing(e);
                    $(this).closest('li').find('.action').show();
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
                    console.log(data);
                      ownerBear = data;
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

        var addOwnerAvatar = function(id, index) {
          
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
                    $('.games li:eq(' + index + ') img').attr('src', 'images/avatar-0' + data.bearAvatarId + '.png');
                  } else {
                      $("#bearDetail").html("no bear found");
                  }


              })
              .fail(function (err) {
                  $("#bearDetail").html(err);
              });
        }

        $("#bearsListBtn").click(getBears);

        $(".gameForm").on('submit', function (e) {
            doNothing(e);
            var gameBearId = $("#gameBearId").val();
            var gameId = guid();
            $("#gameBearId").val('');
            schedule(gameId, gameBearId).done(function (data) {
                getGames();
                showSection('games');   
                history.pushState(null, null, '#_games'); 
            });
        });


        //Onload
        getGames();

        $.ajax({
            type: "GET",
            url: "api/bears/current"
          })
          .done(function (data) {
            $('header img').attr('src', 'images/avatar-0' + data.bearAvatarId + '.png');
          })
            .fail(function (err) {
        });

        

        $('.logout').on('click', function(e){
          doNothing(e);
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

    });

})(jQuery)