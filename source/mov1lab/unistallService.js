var Service = require('node-windows').Service;
 
// Create a new service object
var svc = new Service({
  name:'mov1lab',
  description: 'Monitor for the process that sends messages from cobas.',
  script: 'D:\\Aplicaciones\\mov1lab\\source\\mov1lab\\app.js'
});
 
// Listen for the 'uninstall' event so we know when it is done.
svc.on('uninstall',function(){
  console.log('Uninstall complete.');
  console.log('The service exists: ',svc.exists);
 
});
 
// Uninstall the service.
svc.uninstall();