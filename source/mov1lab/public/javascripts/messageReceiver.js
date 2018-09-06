const MaxRows = 20;
const TimePosition = 1;
const DatePosition = 2;
const PatientPosition = 3;
const TestPosition = 4;
const InstrumentPosition = 5;
const SerialPosition = 6;
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
    updateLastConnection();
    var data = parseHl7message(msg);
    var dataTable = document.getElementById("update-table");
  
      if (dataTable.childElementCount >= MaxRows) {
        dataTable.removeChild(dataTable.lastElementChild);
      }
      
      var tr = document.createElement("tr");

      var tdTime = document.createElement("td");
      tr.appendChild(tdTime);
      tdTime.innerText = data.Time;
      tdTime.className ="lead";

      var tdDate = document.createElement("td");
      tr.appendChild(tdDate);
      tdDate.innerText = data.Date;
      tdDate.className ="col-md-2 lead";

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
    Time: splitMsg[TimePosition] == '' ? "-" : splitMsg[TimePosition], 
    Date: splitMsg[DatePosition] == '' ? "-" : splitMsg[DatePosition], 
    Patient: hidePatientId(splitMsg[PatientPosition] == '' ? "-" : splitMsg[PatientPosition]), 
    Test: splitMsg[TestPosition] == '' ? "-" : splitMsg[TestPosition], 
    Instrument: splitMsg[InstrumentPosition] == '' ? "-" : splitMsg[InstrumentPosition],
    Serial: splitMsg[SerialPosition] == '' ? "-" : splitMsg[SerialPosition]
  };
};

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
  var day = new Date().getDay();
  var month = new Date().getMonth();
  var year = new Date().getFullYear();
  var hour = new Date().getHours();
  var minute = new Date().getMinutes();
  var second = new Date().getSeconds();

  document.getElementById("date").innerText = day.toString() + "/" + month.toString() + "/" + year.toString() + " " + hour.toString() + ":" + minute.toString() + ":" + second.toString();
};

setInterval(updateDate,1000);
};

function updateLastConnection(){
  document.getElementById("status").innerText = "Última conexión: " + new Date().toLocaleDateString() + " " + new Date().toLocaleTimeString();
};