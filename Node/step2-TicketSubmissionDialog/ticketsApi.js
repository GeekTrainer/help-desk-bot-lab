/* jshint esversion: 6 */
// tickets sample api
var express = require('express');

var api = express.Router();
var tickets = [];
var lastTicketId = 1;

api.post('/ticket', (req, res) => {
    console.log('Ticket received: ', req.body);
    let ticketId = lastTicketId++;
    var ticket = req.body;
    ticket.id = ticketId;
    tickets.push(ticket);

    res.status(200).send(ticketId.toString()).end();
});

module.exports = api;
