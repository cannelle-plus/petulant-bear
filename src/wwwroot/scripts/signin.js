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
        
        var guid = function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                  v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }

        var signin = function (bearId, bearUsername, bearAvatarId) {
            var signinCmd = createCommand(bearId, 1, {
                "bearAvatarId": bearAvatarId,
                "bearUsername": bearUsername
            });            
            return $.ajax({
                type: "POST",
                url: "api/bears/signin",
                dataType: "json",
                data: JSON.stringify(signinCmd)
            });
        }

        $("#signIn form").on('submit', function (e) {
            doNothing(e);
            var bearId = guid(); //fake bear id used only to provide an id to the commmand
            var bearUsername = $('#bearUsername').val();
            var bearAvatarId = $('input[name=bearAvatarId]:checked').val();
            signin(bearId, bearUsername, bearAvatarId).done(function (data) {
               document.location.replace(data.msg);
            }).fail(function (err) {
                $("#signinResult").html(err);
            });
        });     


    });


    $('.avatars input').on('click', function(){
      $('.avatars li').removeClass('checked');
      $(this).closest('li').addClass('checked');
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
