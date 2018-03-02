var express = require('express');
var router = express.Router();

//require controller modules.
var messageController = require('../controllers/messageController');

// GET request for list of all controller a items.
router.post('/showMessages', messageController.showMessages);

router.get('/showMessages', messageController.showMessagesToLogedUser);

router.get('/getServerIpAndPort', messageController.getServerIpAndPort);

module.exports = router;