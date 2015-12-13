
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

        var setAvatar = function() {       
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

            var changeUsernameCmd = createCommand(guid(), 1, {
                bearUsername: $('#bearUsername').val()
            });
            return $.ajax({
                type: "POST",
                url: "api/bear/changeUserName",
                dataType: "json",
                data: JSON.stringify(changeUsernameCmd)
            })
              .done(function (data) {                 
                  loader.hide();
              })
              .fail(function (err) {
                  $("#changeUsernameResult").html(err);
              });

        });
        $("#changeBearAvatar").on('submit', function (e) {
            doNothing(e);
            loader.show();

            var changeAvatarIdCmd = createCommand(guid(), 1, {
                bearAvatarId: $('#changeBearAvatar input[name=bearAvatarId]:checked').val()
            });
            return $.ajax({
                type: "POST",
                url: "api/bear/changeAvatarId",
                dataType: "json",
                data: JSON.stringify(changeAvatarIdCmd)
            })
              .done(function (data) {
                  setAvatar();
                  getGames();
              })
              .fail(function (err) {
                  $("#changeAvatarIdResult").html(err);
              });
        });


        //Onload
        getGames();
        setAvatar();        

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


var Notification = window.Notification || window.mozNotification || window.webkitNotification;

Notification.requestPermission(function (permission) {
  //console.log(permission);
});

function notifyClient(title, message) {
  var instance = new Notification(
        title, {
          body: message,
          icon: "data:image/png;base64,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"

        }
      );

  instance.onclick = function () {
    // Something to do
  };
  instance.onerror = function () {
    // Something to do
  };
  instance.onshow = function () {
    // Something to do
  };
  instance.onclose = function () {
    // Something to do
  };

  return false;
}
notifyClient('Ready', 'Let\s go');