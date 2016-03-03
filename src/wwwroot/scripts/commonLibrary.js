this.PetulantBear = this.PetulantBear || {};
//COMMON
Date.prototype.addHours = function (h) {
    this.setTime(this.getTime() + (h * 60 * 60 * 1000));
    return this;
};

Date.prototype.toHtml5Date = function () {
    var year = this.getFullYear();
    var m = this.getMonth() + 1;
    var month = m > 9 ? m : "0" + m;
    var day = this.getDate() > 9 ? this.getDate() : "0" + this.getDate();
    return year + "-" + month + "-" + day;
};

var toHtml5Date = function (d) {
    
}

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

var bearInterval = 0;

doNothing = function(event){
  event.preventDefault();
  event.stopPropagation();
}

// Loader
var loader = {
  show: function() {
        $('.loading').show();
  },
  hide: function() {
        $('.loading').hide();
  }
}