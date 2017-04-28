// tickets sample api
var express = require('express');

var api = express.Router();

api.post('/ticket', function(req, res) {
    console.log('Ticket received: ', req.data);
    res.status(200).end();
});

module.exports = api;
