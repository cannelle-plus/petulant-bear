//WebSocket
(function ($, room) {

var counter = 0;
var wsUri = "ws://" + window.location.hostname;
if(window.location.port) 
    wsUri += ":" + window.location.port;
wsUri += "/api/websocket";

var output;

function init() {
    testWebSocket();
}

function testWebSocket() {
    websocket = new WebSocket(wsUri);
    websocket.onopen = function (evt) { onOpen(evt) };
    websocket.onclose = function (evt) { onClose(evt) };
    websocket.onmessage = function (evt) { onMessage(evt) };
    websocket.onerror = function (evt) { onError(evt) };
}


function onOpen(evt) {
    writeToScreen("CONNECTED");
    //setInterval(function () {
    //    counter++;
    //    if (counter < 10) doSend("WebSocket rocks");
    //}, 500);
    //setTimeout(function () {
    //    websocket.close();
    //}, 50000);
}

function onClose(evt) {
    writeToScreen("DISCONNECTED");
}

function onMessage(evt) {
    room.handle(evt);
    writeToScreen(evt.data);
}

function onError(evt) {
    writeToScreen(evt.data);
}

function doSend(message) {
    writeToScreen("SENT: " + message);
    websocket.send(message);
}

function writeToScreen(message) {
    console.log(message);
}

window.addEventListener("load", init, false);


}).call(this.PetulantBear.WebSosket || (this.PetulantBear.WebSosket = {}), jQuery, this.PetulantBear.Room);

