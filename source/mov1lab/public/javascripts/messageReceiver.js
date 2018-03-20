const MaxRows = 20;
const DatetimePosition = 1;
const PatientPosition = 2;
const TestPosition = 3;
const InstrumentPosition = 4;
const SerialPosition = 5;
var httpRequest = new XMLHttpRequest();
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
  
      if (dataTable.childElementCount >= MaxRows) {
        dataTable.removeChild(dataTable.lastElementChild);
      }
      
      var tr = document.createElement("tr");

      var tdDatetime = document.createElement("td");
      tr.appendChild(tdDatetime);
      tdDatetime.innerText = data.Datetime;
      tdDatetime.className ="col-md-3 lead";

      var tdPatien = document.createElement("td");
      tr.appendChild(tdPatien);
      tdPatien.innerText = data.Patient;
      tdPatien.className ="col-md-2 lead";

      var tdTest = document.createElement("td");
      tr.appendChild(tdTest);
      tdTest.innerText = data.Test;
      tdTest.className ="col-md-2 lead";

      var tdInstrument = document.createElement("td");
      tr.appendChild(tdInstrument);
      tdInstrument.innerText = data.Instrument;
      tdInstrument.className ="col-md-2 lead";

      var tdSerial = document.createElement("td");
      tr.appendChild(tdSerial);
      tdSerial.innerText = data.Serial;
      tdSerial.className ="col-md-3 lead";

      dataTable.insertBefore(tr,dataTable.firstElementChild);  
  });

function parseHl7message(msg) {
  var splitMsg = msg.split("|");
  return { 
    Datetime: splitMsg[DatetimePosition] == '' ? "-" : splitMsg[DatetimePosition], 
    Patient: hidePatientId(splitMsg[PatientPosition] == '' ? "-" : splitMsg[PatientPosition]), 
    Test: splitMsg[TestPosition] == '' ? "-" : splitMsg[TestPosition], 
    Instrument: splitMsg[InstrumentPosition] == '' ? "-" : splitMsg[InstrumentPosition],
    Serial: splitMsg[SerialPosition] == '' ? "-" : splitMsg[SerialPosition]
  };
};

socket.on('pong',function(){
  lastTimeHeartBeatWasReceived = new Date();
});

socket.on('disconnect',function(){
  setOfflineIcon();
});

socket.on('hl7ConverterAlive',function(){
  setOnlineIcon();
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
  status.className ="reddot";
};

function setOnlineIcon(){
  var status = document.getElementById("status");
  status.className ="greendot";
};