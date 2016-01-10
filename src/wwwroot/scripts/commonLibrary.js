

this.PetulantBear = this.PetulantBear || {};
//COMMON
Date.prototype.addHours = function (h) {
    this.setTime(this.getTime() + (h * 60 * 60 * 1000));
    return this;
};

var guid = function () {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0,
          v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
};

var createCommand = function (id, version, payLoad) {
    return {
        id: id,
        idCommand: guid(),
        version: version,
        payLoad: payLoad
    };
};

var toBEARDATE = function(MSDate) {
  var bearDate = MSDate.split('T'),
      year = bearDate[0].split('-')[0],
      month = bearDate[0].split('-')[1],
      day = bearDate[0].split('-')[2],
      hour = bearDate[1].split(':')[0],
      min = bearDate[1].split(':')[1];

  return day + '/' + month + '/' + year + ' à ' + hour + 'h' + min;
}