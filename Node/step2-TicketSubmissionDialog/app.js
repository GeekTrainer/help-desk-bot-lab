/* jshint esversion: 6 */
const express = require('express');
const builder = require('botbuilder');
const request = require('request');
const sampleApi = require('./api');

const app = express();
const listenPort = process.env.port || process.env.PORT || 3978;

// Setup Express Server
app.listen(listenPort, '::', () => {
    console.log('Server Up');
});

// expose the sample API
app.use('/api', sampleApi);

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
        session.send('Welcome, let\'s complete the ticket details.');
        builder.Prompts.text(session, "Type the category");
    },
    function (session, result, next) {
        session.dialogData.category = result.response;
        session.send('Ok, the category is: ' + session.dialogData.category);

        var choices = ['High', 'Normal', 'Low'];
        builder.Prompts.choice(session, 'Choose the severity', choices);
    },
    function (session, result, next) {
        session.dialogData.severity = result.response.entity;
        session.send('Ok, the category is: ' + session.dialogData.category + ' and the severity is: ' + session.dialogData.severity);
        builder.Prompts.text(session, 'Type a description');
    },
    function (session, result, next) {
        session.dialogData.description = result.response;

        var data = {
            category: session.dialogData.category,
            severity: session.dialogData.severity,
            description: session.dialogData.description,
        }

        request.post('http://localhost:'  + listenPort + '/api/ticket', data, function(err, response) {
            if (err) {
                session.send('Something went wrong while we was recording your issue. Please try again later.')
            } else {
                session.send('Got it. Your ticked has been recorded. Category: ' + session.dialogData.category + ', Severity ' + session.dialogData.severity + ', Description: ' + session.dialogData.description);
            }

            session.endDialog();
        });
    }
]);
