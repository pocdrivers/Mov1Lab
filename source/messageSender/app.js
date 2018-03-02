var express = require('express');
var app = express();
//used to stablish the connection with the main server through tcp
var net = require('net');
//used to stablish the connection with the client through http
var http = require('http').Server(app);
var io = require('socket.io')(http,{pingInterval: 1000, pingTimeout:5000});

const Port = 9000;

var connectedClients = [];
var lastTimeHeartBeatWasReceived = null;

//Create the server object
var server = net.createServer(function(socket){

  /*socket.name = socket.remoteAddress + ":" + socket.remotePort;
  console.log('remote ' + socket.name);
  console.log('local ' + socket.localAddress + ":" + socket.localPort);*/

  var remoteAddress = socket.remoteAddress + ':' + socket.remotePort;
  
  //if(!connectedClients.includes(socket.remoteAddress))
  //{
    connectedClients.push(socket.remoteAddress);
    handleHandshake(remoteAddress);
  //}

  socket.on('data',function(data){
    console.log('mensaje recibido');
    var mensaje = bin2String(data);
    if(mensaje == 'Ping')
    {
      lastTimeHeartBeatWasReceived = new Date();
      console.log(lastTimeHeartBeatWasReceived);
      console.log('Client is alive');
    }
    else
    {
      io.emit('updateTable', mensaje);
      //socket.write("llego");
      console.log(mensaje);
    }
  });

  socket.on('close',function(){
    console.log('cerrando');
    //io.emit('disconnect');
  });

  socket.on('error',function(e){
    if (e != "websocket: close sent") {
      console.log('An unexpected error occured: ', e);
    }
    console.log('error');
    io.emit('disconnect');
  });

}).listen(Port);

function handleHandshake(remoteAddress){
  io.emit('updateFooterIp',remoteAddress);
}

setInterval(function(){
  var now = new Date();
  var timeSinceLastHeartBeatReceived = now - lastTimeHeartBeatWasReceived
  if(timeSinceLastHeartBeatReceived > 10000)
  {
    console.log('Server down!');
    io.emit('disconnect');
  }
},10000);

function bin2String(array) {
  var result = "";
  for (var i = 0; i < array.length; i++) {
    result += String.fromCharCode(array[i]);
  }
  return result;
}


http.listen(3006, function(){
  console.log('listening on *:3006');
});

//An make it listen to port 9000
//server.listen(Port,Address);

//Binding the handleConnection function to the connection event. 
//The server emits this event every time there is a new TCP connection made to the server, passing in the socket object.
//server.on('connection', handleConnection);

//Sockets objects can emit several events
// data - every time data arrives from the connected peer
// close - once the connection closes
// error - if an error happens 
/*function handleConnection(conn) {  

  var remoteAddress = conn.remoteAddress + ':' + conn.remotePort;
  console.log('new client connection from %s', remoteAddress);

  conn.on('data', onConnData);
  conn.on('close', onConnClose);
  conn.on('error', onConnError);

  function onConnData(d) {
    var mensaje = bin2String(d);
    sendMsg(mensaje, remoteAddress);
    conn.write(d);
    io.emit('mainServerOnline');
  }

  function onConnClose() {
    //console.log('connection from %s closed', remoteAddress);
    console.log('Main Server Offline', remoteAddress);
    io.emit('disconnect');
  }

  function onConnError(err) {
    console.log('Connection %s error: %s', remoteAddress, err.message);
    io.emit('disconnect');
  }
}*/

/*function sendMsg(mensaje, masterServerIp){
  console.log(mensaje);
    io.emit('updateTable', mensaje);
    io.emit('updateFooterIp', masterServerIp);
    console.log("io emit done successfully");
};*/


