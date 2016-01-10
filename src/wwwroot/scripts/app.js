(function ($, bears,games, cleaveage) {

    $(function () {

      bears.init().done(games.init);

        var guid = function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0,
                  v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }

        

        /*var markBear = function (gameId, version, bearId,  mark) {
            var markBearCmd = createCommand(gameId, version, {
                "bearId" : bearId,
                "mark": mark
            });
            loader.show();
            return $.ajax({
                type: "POST",
                url: "api/finishedGames/markBear",
                dataType: "json",
                data: JSON.stringify(markBearCmd)
            })
              .fail(function (err) {
                  $("#markBearResult").html(err);
              });
        }

        var commentBear = function (gameId,version, bearId,  comment) {
            var commentBearCmd = createCommand(gameId, version, {
                "bearId" : bearId,
                "comment": comment
            });
            loader.show();
            return $.ajax({
                type: "POST",
                url: "api/finishedGames/commentBear",
                dataType: "json",
                data: JSON.stringify(commentBearCmd)
            })
              .fail(function (err) {
                  $("#commentBearResult").html(err);
              });
        }*/

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

        var startCalibration = function (transmitterId,version,  receiverId, distance) {
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
        

    });

}).call(this.PetulantBear.App || (this.PetulantBear.App = {}), jQuery, this.PetulantBear.Bears, this.PetulantBear.Games, this.PetulantBear.Cleavage);


/*
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
notifyClient('Ready', 'Let\s go');*/