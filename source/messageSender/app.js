/** 
 * MessageSender is a middle application in charge of
 * receive messages from cobas and sending them to
 * the mov1lab application to show them to the user.
 * This application its also known to as the
 * "middleman". The application receive messsages
 * through tcp from cobas and sends them through
 * http to mov1lab.
*/

var express = require('express');
var app = express();
/**
 * Used to stablish the connection with the main server through tcp
 */
 var net = require('net');
/**
 * Used to stablish the connection with the client through http
 */
var http = require('http').Server(app);
/** 
 * Used to signal the client that the middleman is alive
*/
var io = require('socket.io')(http,{pingInterval: 1000, pingTimeout:5000});
/**
 * Default port to listen for messages coming from cobas
 */
const Port = 9000;
/** 
 * List of connected clients. Used to have a list of connected ip adresses.
*/
var connectedClients = [];
/** 
 * Stores the last time a heart beat signal was emited from the tool that
 * receives messages from cobas.
*/
var lastTimeHeartBeatWasReceived = null;

/** 
 * Creates the server that will listen for incoming messages.
 * Every time a new message is received, it is sent to the
 * client.
*/
var server = net.createServer(function(socket){

  var remoteAddress = socket.remoteAddress + ':' + socket.remotePort;
  
  connectedClients.push(socket.remoteAddress);
  handleHandshake(remoteAddress);

  /**
   * Event that gets executed everytime a message is received.
   * If the message received is a ping, the datetime is stored
   * in lastTimeHeartBeatWasReceived to be compared later and
   * assume that the application that sends messages is alive;
   * otherwise, we asume is a valid message and it is sent to 
   * the client.
   */
  socket.on('data',function(data){
    console.log('mensaje recibido');
    var mensaje = bin2String(data);
    if(mensaje == 'Ping')
    {
      lastTimeHeartBeatWasReceived = new Date();
      io.emit('hl7ConverterAlive');
      console.log(lastTimeHeartBeatWasReceived);
      console.log('Hl7Converter is alive');
    }
    else
    {
      io.emit('updateTable', mensaje);
      console.log(mensaje);
    }
  });

  /**socket.on('close',function(){
    console.log('close');
    io.emit('disconnect');
  });**/ 
  

  /**
   * Event that gets executed everytime the application that
   * sends messages fails. When this happen the client gets
   * a signal indicating that the server is down.
   */
  socket.on('error',function(e){
    if (e != "websocket: close sent") {
      console.log('An unexpected error occured: ', e);
    }
    console.log('error');
    io.emit('disconnect');
  });

}).listen(Port);

/**
 * Sends a message to the client application with the ip address
 * of the application that sends the messages.
 * @param {string} remoteAddress 
 */
function handleHandshake(remoteAddress){
  io.emit('updateFooterIp',remoteAddress);
}

/**
 * This method gets executed every 10 seconds to check when was
 * the last time that a heartbeat was received. If the interval
 * of time is bigger than 20 seconds, we asume the server is 
 * down.
 */
setInterval(function(){
  var now = new Date();
  var timeSinceLastHeartBeatReceived = now - lastTimeHeartBeatWasReceived
  if(timeSinceLastHeartBeatReceived > 10000)
  {
    console.log('Server down!');
    io.emit('disconnect');
  }
},5000);

/**
 * Transform the byte array representation of the message
 * to a string.
 * @param {array} array 
 */
function bin2String(array) {
  var result = "";
  for (var i = 0; i < array.length; i++) {
    result += String.fromCharCode(array[i]);
  }
  return result;
}

/**
 * Listen for new client connections on port 3006.
 */
http.listen(3006, function(){
  console.log('listening on *:3006');
});