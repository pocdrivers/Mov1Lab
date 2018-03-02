var fs = require('fs');

exports.showMessages = function(req, res) {
  req.checkBody('username', 'Username is required').notEmpty();
  req.checkBody('password', 'Password is required').notEmpty();

  var errors = req.validationErrors();

  if(errors){
    res.render('login',{errors:errors});
  }
  else {
    var jsonData = fs.readFileSync('config.json', 'utf8');
    var username = JSON.parse(jsonData).user.trim();
    var password = JSON.parse(jsonData).password.trim();
    try {
      if ( username == req.body.username && password == req.body.password) 
      {
        res.render('hl7messages', {user:true});
      }
      else 
      {
        var errorMessages = [{msg: "Invalid username or password."}];
        res.render('login',{errors:errorMessages});
      }
    } catch (e) {
      console.log('Error: ', e.stack);
    }
  }
};

exports.showMessagesToLogedUser = function(req, res){
    var errorMessages = [{msg: "You have to login into the application."}];
    res.render('login',{errors:errorMessages});
};

exports.getServerIpAndPort = function(req, res){
  var jsonData = fs.readFileSync('config.json', 'utf8');
  var serverIp = JSON.parse(jsonData).serverIp.trim();
  var serverPort = JSON.parse(jsonData).serverPort.trim();
  res.send(serverIp+"|"+serverPort);
};