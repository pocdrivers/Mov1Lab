const MaxRows = 20;
const DatetimePosition = 1;
const PatientPosition = 2;
const TestPosition = 1;
const InstrumentPosition = 2;
const SerialPosition = 3;
var httpRequest = new XMLHttpRequest();
var dateTime;
var patient;
var lastTimeHeartBeatWasReceived;

httpRequest.onload = function(){
  var server = httpRequest.responseText.split("|");
  var socket = io('http://' + server[0] + ':' + server[1]);

  startMessageReceive(socket);
}
httpRequest.open("GET", "/messages/getServerIpAndPort");
httpRequest.send();

function startMessageReceive(socket){
  socket.on('updateTable', function (msg) {
    setOnlineIcon();
    var data = parseHl7message(msg);
    var dataTable = document.getElementById("update-table");
  
    data.forEach(element => {

      if (dataTable.childElementCount >= MaxRows) {
        dataTable.removeChild(dataTable.lastElementChild);
      }
      
      var tr = document.createElement("tr");

      var tdDatetime = document.createElement("td");
      tr.appendChild(tdDatetime);
      tdDatetime.innerText = element.Datetime;
      tdDatetime.className ="col-md-3";

      var tdPatien = document.createElement("td");
      tr.appendChild(tdPatien);
      tdPatien.innerText = element.Patient;
      tdPatien.className ="col-md-2";

      var tdTest = document.createElement("td");
      tr.appendChild(tdTest);
      tdTest.innerText = element.Test;
      tdTest.className ="col-md-2";

      var tdInstrument = document.createElement("td");
      tr.appendChild(tdInstrument);
      tdInstrument.innerText = element.Instrument;
      tdInstrument.className ="col-md-2";

      var tdSerial = document.createElement("td");
      tr.appendChild(tdSerial);
      tdSerial.innerText = element.Serial;
      tdSerial.className ="col-md-3";

      dataTable.insertBefore(tr,dataTable.firstElementChild);    
    });
});

function parseHl7message(msg) {
  var splitMsg = msg.split("|");
  dateTime = splitMsg[DatetimePosition];
  patient = hidePatientId(splitMsg[PatientPosition]);

  var returnValue = [];

  var i=0;
  while(splitMsg.indexOf("NEWTEST", i) > 0){
    returnValue.push({ 
      Datetime: dateTime == '' ? "-" : dateTime, 
      Patient: patient == '' ? "-" : patient, 
      Test: splitMsg[splitMsg.indexOf("NEWTEST", i)+TestPosition] == '' ? "-" : splitMsg[splitMsg.indexOf("NEWTEST", i)+TestPosition], 
      Instrument: splitMsg[splitMsg.indexOf("NEWTEST", i)+InstrumentPosition] == '' ? "-" : splitMsg[splitMsg.indexOf("NEWTEST", i)+InstrumentPosition], 
      Serial: splitMsg[splitMsg.indexOf("NEWTEST", i)+SerialPosition] == '' ? "-" : splitMsg[splitMsg.indexOf("NEWTEST", i)+SerialPosition] });

    i = splitMsg.indexOf("NEWTEST", i) + 1;
  }

  return returnValue;
};

socket.on('pong',function(){
  lastTimeHeartBeatWasReceived = new Date();
});


socket.on('disconnect',function(){
  setOfflineIcon();
});


socket.on('updateFooterIp',function(masterServerIp){
  document.getElementById("masterServerIp").innerText = "IP: " + masterServerIp.replace("::ffff:",'');
  masterServerConnected = true;
});

function hidePatientId(patientId){
  var stringSize = patientId.length;
  var stringRest = stringSize - 3;
  var res = patientId.substring(stringRest,stringSize);
  
  for (i = 0; i < stringRest; i++) { 
    res = "*" + res;
  }
  
  return res;
};

function updateDate(){
  document.getElementById("date").innerText = new Date().toLocaleDateString() + " " + new Date().toLocaleTimeString();
};

setInterval(updateDate,1000);
};

setInterval(function(){
  var now = new Date();
  var timeSinceLastHeartBeatReceived = now - lastTimeHeartBeatWasReceived
  if(timeSinceLastHeartBeatReceived > 10000)
  {
    console.log('Middleman down!');
  }
},10000);

function setOfflineIcon(){
  var status = document.getElementById("status");
  status.src = "../images/offline.png";
};

function setOnlineIcon(){
  var status = document.getElementById("status");
  status.src = "../images/online.png";
};