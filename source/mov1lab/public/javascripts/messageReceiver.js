const MaxRows = 20;
const DatePosition = 1;
const TimePosition = 2;
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
  socket.on('close', function(){
    console.log('Close received!');
    document.getElementById("status").src = "/images/offline.png";
    console.log('Offline icon set!');
  })

  socket.on('updateTable', function (msg) {
    updateLastConnection();
    var data = parseHl7message(msg);
    var dataTable = document.getElementById("update-table");
  
      if (dataTable.childElementCount >= MaxRows) {
        dataTable.removeChild(dataTable.lastElementChild);
      }
      
      var tr = document.createElement("tr");

      var tdDate = document.createElement("td");
      tr.appendChild(tdDate);
      tdDate.innerText = data.Date;
      tdDate.className ="lead";
      tdDate.style ="padding-right: 22px;";

      var tdTime = document.createElement("td");
      tr.appendChild(tdTime);
      tdTime.innerText = data.Time;
      tdTime.className ="col-md-2 lead";

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
    Date: splitMsg[DatePosition] == '' ? "-" : splitMsg[DatePosition], 
    Time: splitMsg[TimePosition] == '' ? "-" : splitMsg[TimePosition], 
    Patient: hidePatientId(splitMsg[PatientPosition] == '' ? "-" : splitMsg[PatientPosition]), 
    Test: splitMsg[TestPosition] == '' ? "-" : splitMsg[TestPosition], 
    Instrument: splitMsg[InstrumentPosition] == '' ? "-" : splitMsg[InstrumentPosition],
    Serial: splitMsg[SerialPosition] == '' ? "-" : splitMsg[SerialPosition]
  };
};

socket.on('updateFooterIp',function(masterServerIp){
  document.getElementById("masterServerIp").innerText = "IP: " + masterServerIp.replace("::ffff:",'');
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
  var day = "0" + new Date().getUTCDate().toString();
  var month = "0" + (parseInt(new Date().getUTCMonth().toString())+1);
  var year = new Date().getFullYear().toString();
  var hour = "0" + new Date().getHours().toString();
  var minute = "0" + new Date().getMinutes().toString();
  var second = "0" + new Date().getSeconds().toString();

  document.getElementById("date").innerText = day.substring(day.length -2, day.length) + "/" + 
                                              month.substring(month.length -2, month.length) + "/" + 
                                              year.toString() + " " +
                                              hour.substring(hour.length -2, hour.length) + ":" +
                                              minute.substring(minute.length -2, minute.length) + ":" +
                                              second.substring(second.length -2, second.length);
};

setInterval(updateDate,1000);
};

function updateLastConnection(){
  document.getElementById("status").src = "/images/online.png"
};

// socket.on('disconnect',function(time){
//   console.log('Disconnect received!' + time);
//   document.getElementById("status").src = "/images/offline.png"
//   console.log('Offline icon set!' + time);
// });