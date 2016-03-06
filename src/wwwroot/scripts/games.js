

this.PetulantBear = this.PetulantBear || {};

//APP
(function ($, bears, cleaveage, room) {

    var self = this;

    this.schedule = function (id, version) {
        var startDate = (new Date($('#gameDate').val() + ' ' + $('#gameHour').val())).addHours(1)
        var scheduleCmd = createCommand(id, version, {
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

    this.cancel = function (id, version) {
        var cancelCmd = createCommand(id, version, {});
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


    this.join = function (id, version) {
        var joinCmd = createCommand(id, version, {});
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

    this.registerPlayer = function (id, version) {
        var registerPlayerCmd = createCommand(id, version, {
              bearId: $('#bearsToRegistration').val()
        });
        loader.show();
        return $.ajax({
        type: "POST",
                url: "api/games/join",
                dataType: "json",
                data: JSON.stringify(joinCmd)
            })
          .fail(function(err) {
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
                    msg.push('<ul class="game-detail"><li data-id="' + data.id + '" data-version="' + data.version + '">');
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
                        msg.push('</li>');
                    }                    

                    msg.push('</ul></li><br />');

                    if (data.isJoinable)
                        msg.push('<li class="detailAction infos"><a href="#" id="actionDetailJoin" class="actionJoin detail button button-small" data-id="' + data.id + '" data-version="' + data.version + '">Rejoindre</a></li>');
                    else if (data.isAbandonnable)
                        msg.push('<li class="detailAction infos"><a href="#" id="actionDetailAbandonGame" class="actionAbandonGame detail button button-small" data-id="' + data.id + '" data-version="' + data.version + '">Quitter</a></li>');
                    
                    msg.push('</ul>');


                    //room of the game
                    msg.push("<a href='#' class='roomDetailButton' data-target='room' data-id='" + data.id + "' data-version='" + data.roomVersion + "'><img src='images/chat-icon.png' width='20' height='20' /> Chat</a>")
                    msg.push("<div class='roomDetail' data-target='room' data-id='" + data.id + "' data-version='" + data.roomVersion + "'></div>");
                    $("#gameDetailled").html(msg.join(''));
                    $("#chat .infos strong").html(data.name);
                    room.getDetailRoom(data.id);
                } else {
                    $("#gameDetailled").html("no game found");
                }

                $("#chat").on('submit', function (e) {
                    doNothing(e);
                    var roomId = $(e.target).data("id");
                    var roomVersion = $(e.target).data("version");
                    var message = $("#msgRoom").val();
                    $("#msgRoom").val('');
                    if(message !== '')
                        room.postMessageToRoom(roomId,roomVersion,  message, data.id);    
                });

                $('body').on('click', '.roomDetailButton, .roomDetail', function() {
                    $('#chat').data('id', $(this).data('id'));
                });

                var gameAction = function (f, resultDiv) {
                    return function (e) {
                        doNothing(e);
                        var gameId = $(e.target).data("id");
                        var version = $(e.target).data("version");
                        $(e.target).closest('.action').hide();
                        f(gameId, version).done(function (data) {
                            $("#" + resultDiv).html("received at " + Date.now() + ", " + JSON.stringify(data));
                            setTimeout(function () {
                                self.getGames();
                                self.getGame($(e.target).data("id"));
                            },2000);
                        });
                    }
                }
                $("#actionDetailJoin").click(gameAction(self.join, "joinResult"));
                $("#actionDetailCancelGame").click(gameAction(self.cancel, "cancelResult"));
                $("#actionDetailAbandonGame").click(gameAction(self.abandon, "abandonResult"));
                  
                loader.hide();

            })
            .fail(function (err) {
                $("#gameDetailxxx").html(err);
            });
    };

    this.getGames = function () {
        loader.show();
        fromDate = $("#fromDate").val();
        toDate = $("#toDate").val();
        $.ajax({
            type: "POST",
            url: "api/games/list",
            dataType: "json",
            data: JSON.stringify({
                "from": (new Date(fromDate)).toJSON(),
                "to": (new Date(toDate)).toJSON()
            })
        })
              .done(function (data) {
                  var msg = [];
                  msg.push('<ul id="gamesListContainer" class="games">');
                  for (var i = 0; i < data.length; i++) {
                      msg.push('<li data-id="' + data[i].id + '">');
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
                            msg.push('<button type="button"  class="actionListJoin" data-id="' + data[i].id + '" data-version="' + data[i].version + '">Rejoindre</button>');
                        else
                            msg.push('&nbsp;');

                        if (data[i].isAbandonnable)
                            msg.push('<button type="button" class="actionListAbandonGame" data-id="' + data[i].id + '" data-version="' + data[i].version + '">Quitter</button>');
                        else
                            msg.push('&nbsp;');
                        if (data[i].isCancellable)
                            msg.push('<button type="button" class="actionListCancelGame" data-id="' + data[i].id + '" data-version="' + data[i].version + '">Supprimer</button>');
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
                    self.getGame(_id);
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
                          var version = $(e.target).data("version");
                          $(e.target).closest('.action').hide();
                          f(gameId, version).done(function (data) {
                              setTimeout(function () {
                                  $("#" + resultDiv).html("received at " + Date.now() + ", " + JSON.stringify(data));
                                  self.getGames();
                              },2000)
                              
                          });
                      }
                  }

                  $(".actionListJoin").click(gameAction(self.join, "joinResult"));
                  $(".actionListCancelGame").click(gameAction(self.cancel, "cancelResult"));
                  $(".actionListAbandonGame").click(gameAction(self.abandon, "abandonResult"));
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



    this.init = function () {
        $(".gameForm").on('submit', function (e) {
            doNothing(e);
            var gameId = guid();
            var version = 0;            
            self.schedule(gameId,version).done(function (data) {
                setTimeout(function () {
                    self.getGames();
                    showSection('games');
                    history.pushState(null, null, '#_games');
                }, 2000);
            });
        });

        $("#getGames").click(self.getGames);
        $("#fromDate").val(new Date().toHtml5Date());
        $("#toDate").val(new Date().addHours(168).toHtml5Date());
        
        self.getGames();
    }







}).call(this.PetulantBear.Games || (this.PetulantBear.Games = {}), jQuery, this.PetulantBear.Bears, this.PetulantBear.Cleavage, this.PetulantBear.Room);

//this.PetulantBear.App = app.call({}, jQuery)



