/* jshint esversion: 6 */
const express = require('express');
const builder = require('botbuilder');

const app = express();

// Setup Express Server
app.listen(process.env.port || process.env.PORT || 3978, '::', () => {
    console.log('Server Up');
});

// Create connector
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

// Expose connector
app.post('/api/messages', connector.listen());

// Create Chat Bot
var bot = new builder.UniversalBot(connector, [
    function (session, args, next) {
        session.send('You said: ' + session.message.text);
    }
]);
