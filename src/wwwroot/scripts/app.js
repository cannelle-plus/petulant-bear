
Date.prototype.addHours = function (h) {
    this.setTime(this.getTime() + (h * 60 * 60 * 1000));
    return this;
}

function toBEARDATE(MSDate) {
  var bearDate = MSDate.split('T'),
      year = bearDate[0].split('-')[0],
      month = bearDate[0].split('-')[1],
      day = bearDate[0].split('-')[2],
      hour = bearDate[1].split(':')[0],
      min = bearDate[1].split(':')[1];

  return day + '/' + month + '/' + year + ' à ' + hour + 'h' + min;
}

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
            var startDate = (new Date($('#gameDate').val() + ' ' + $('#gameHour').val())).addHours(1)
            var scheduleCmd = createCommand(id, 1, {
                "location": $('#gameLocation').val(),
                "maxPlayers": $('#nbPlayersRequired').val(),
                "name": $('#gameName').val(),
                "startDate": startDate.toJSON()
            });
            loader.show();
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
            loader.show();
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
            loader.show();
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
            loader.show();
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
            loader.show();
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
            loader.show();
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
            loader.show();
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
              loader.show();
              return $.ajax({
                type: "POST",
                url: "api/rooms/postmessage",
                dataType: "json",
                data: JSON.stringify(postMessageCmd)
              })
              .done(function(data){                                
                getDetailRoom(roomId);
                loader.hide();
              })
              .fail(function (err) {
                  $("#postMessageToRoomResult").html(err);
              });
            }
        };

        var getDetailRoom = function (gameRoomId) {
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
                      var msg = [];
                      msg.push("<dl class='chat'>");
                      //msg.push("<TD>" + data.roomId + "</TD>");
                      //msg.push("<TD>" + data.name + "</TD>");
                      for (var i = data.messages.length - 1; i >= ((data.messages.length < 50) ? 0 : data.messages.length - 9); i--) {
                          //msg.push("<TD>" + data.messages[i].bear.bearId + "</TD>");
                          msg.push("<dt style='background-image:url(images/avatar-0" + data.messages[i].bear.bearAvatarId + ".png);'>" + data.messages[i].bear.bearUsername + "</dt>");
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
                  loader.hide();
              })
              .fail(function (err) {
                  $(".roomDetail").html(err);
              });
        };

        var getGame = function (gameId) {
            loader.show();
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
                    msg.push(toBEARDATE(data.startDate));

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
                    var waitingListIndex = 0;
                    for (var i = 0; i < data.players.length; i++) {
                        var cssWaitingList = "";
                        var infos = "";
                        if (data.players[i].isWaitingList) {
                            waitingListIndex++
                            cssWaitingList = "waitingList";
                            infos += " (" + waitingListIndex + ")";
                        }

                        msg.push('<li class="' + cssWaitingList  + '" data-id=" ' + data.players[i].bearId + '">');
                        msg.push(data.players[i].bearUsername);
                        msg.push(infos);
                        //msgPlayers.push("<TD>" + data.players[i].bearAvatarId + "</TD>");
                        //msgPlayers.push("<TD>" + data.players[i].mark + "</TD>");
                        //msgPlayers.push("<TD>" + data.players[i].comment + "</TD>");
                        //msgPlayers.push("<TD><div href='#' class='markBearBtn' data-id='" + data.players[i].bearId + "'>mark</div><div id='markBearResult'></div></TD>");
                        //msgPlayers.push("<TD><div href='#' class='commentBearBtn' data-id='" + data.players[i].bearId + "'>comment</div><div id='commentBearResult'></div></TD>");
                        msg.push('</li>');
                    }                    

                    msg.push('</ul></li><br />');

                    if (data.isJoinable)
                      msg.push('<li class="detailAction infos"><a href="#" class="actionJoin button button-small" data-id="' + data.id + '">Rejoindre</a></li>');
                    else if (data.isAbandonnable)
                      msg.push('<li class="detailAction infos"><a href="#" class="actionAbandonGame button button-small" data-id="' + data.id + '">Quitter</a></li>');
                    
                    msg.push('</ul>');


                    //room of the game
                    msg.push("<a href='#' class='roomDetailButton' data-target='room' data-id='" + data.id + "'><img src='images/chat-icon.png' width='20' height='20' /> Chat</a>")
                    msg.push("<div class='roomDetail' data-target='room'></div>");
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
                  
                loader.hide();

            })
            .fail(function (err) {
                $("#gameDetailxxx").html(err);
            });
        };

        var getGames = function () {
            loader.show();

            $.ajax({
                type: "POST",
                url: "api/games/list",
                dataType: "json",
                data: JSON.stringify({
                    "from": new Date(),
                    "to": new Date().addHours(168)
                })
            })
              .done(function (data) {
                  var msg = [];
                  msg.push('<ul id="gamesListContainer" class="games">');
                  for (var i = 0; i < data.length; i++) {
                      addOwnerAvatar(data[i].ownerId, i);
                      msg.push('<li data-id="' + data[i].id + '">');
                      msg.push('<p class="avatar"><img src="images/avatar.png" alt="" height="25" width="25"></p>');
                      msg.push('<p class="infos"><strong>' + data[i].name + '</strong><span>'+ data[i].location +' <br /> '+ toBEARDATE(data[i].startDate) +'</span></p>');

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
                  
                  loader.hide();

              })
              .fail(function (err) {
                  $("#gamesList").html(err);
              });
        };

        var getBear = function (id) {
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

        var getBears = function () {
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
                      getBear($(e.target).data("id"));
                  });
                  loader.hide();
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

        // Change Profile
        $("#changeBearName").on('submit', function (e) {
            doNothing(e);
            loader.show();
            $.ajax({
                type: "POST",
                url: "/api/bear/changeUserName",
                dataType: "json",
                data: JSON.stringify({
                    bearUsername: $('#profile #bearUsername').val()
                })
            })
              .done(function (data) {
                 
                  loader.hide();
              })
              .fail(function (err) {
                  
              });
        });
        $("#changeBearAvatar").on('submit', function (e) {
            doNothing(e);
            loader.show();
            $.ajax({
                type: "POST",
                url: "/api/bear/changeAvatarId",
                dataType: "json",
                data: JSON.stringify({
                    bearAvatarId: $('#changeBearAvatar input[name=bearAvatarId]:checked').val()
                })
            })
              .done(function (data) {
                 
                  loader.hide();
              })
              .fail(function (err) {
                  
              });
        });


        //Onload
        getGames();

        loader.show();
        $.ajax({
            type: "GET",
            url: "api/bears/current"
          })
          .done(function (data) {
            $('header img').attr('src', 'images/avatar-0' + data.bearAvatarId + '.png');
                    
            //-- Set profile screen
            $('#profile #bearUsername').val(data.bearUsername);
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

        

        $('.logout').on('click', function(e){
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

    });

})(jQuery)