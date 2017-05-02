/* jshint esversion: 6 */
// tickets sample api
var express = require('express');

var api = express.Router();

api.post('/ticket', (req, res) => {
    console.log('Ticket received: ', req.data);
    res.status(200).end();
});

module.exports = api;
