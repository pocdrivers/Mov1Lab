var Service = require('node-windows').Service;
 
// Create a new service object
var svc = new Service({
  name:'mov1labMiddelMan6',
  description: 'Monitor for the process that sends messages from cobas.',
  script: 'C:\\Program Files (x86)\\Mov1Lab\\messageSender\\app.js'
});
 
// Listen for the 'install' event, which indicates the
// process is available as a service.
svc.on('install',function(){
  svc.start();
});
 
// install the service
svc.install();