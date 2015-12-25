

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
