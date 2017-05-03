/* jshint esversion: 6 */
const express = require('express');
const bodyParser = require('body-parser');
const builder = require('botbuilder');
const request = require('request');
const ticketsApi = require('./ticketsApi');

const app = express();
const listenPort = process.env.port || process.env.PORT || 3978;

app.use(bodyParser.json());

// Setup Express Server
app.listen(listenPort, '::', () => {
    console.log('Server Up');
});

// expose the sample API
app.use('/api', ticketsApi);

// Create connector
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

// Expose connector
app.post('/api/messages', connector.listen());

// Create Chat Bot
var bot = new builder.UniversalBot(connector, [
    (session, args, next) => {
        session.send('Welcome, let\'s complete the ticket details.');
        builder.Prompts.text(session, "Describe your problem");
    },
    (session, result, next) => {
        session.dialogData.description = result.response;

        var choices = ['high', 'normal', 'low'];
        builder.Prompts.choice(session, 'Choose the ticket severity', choices);
    },
    (session, result, next) => {
        session.dialogData.severity = result.response.entity;

        builder.Prompts.text(session, 'Type the ticket description');
    },
    (session, result, next) => {
        session.dialogData.category = result.response;

        var message = 'I\'m going to create ' + session.dialogData.severity + ' severity ticket under category ' + session.dialogData.category +
                        '. The description i will use is: ' + session.dialogData.description + '. Do you want to continue adding this ticket?';

        builder.Prompts.confirm(session, message);
    },
    (session, result, next) => {

        if (result.response) {
            var data = {
                category: session.dialogData.category,
                severity: session.dialogData.severity,
                description: session.dialogData.description,
            }

            request({ method: 'POST', url: 'http://localhost:'  + listenPort + '/api/tickets', json: true, body: data }, (err, response) => {
                if (err || response.body == -1) {
                    session.send('Something went wrong while we was recording your issue. Please try again later.')
                } else {
                    session.send('## Your ticked has been recorded:\n\n - Ticket ID: ' + response.body + '\n\n - Category: ' + session.dialogData.category + '\n\n - Severity: ' + session.dialogData.severity + '\n\n - Description: ' + session.dialogData.description);
                }

                session.endDialog();
            });
        } else {
            session.endDialog('Ok, action cancelled!');
        }
    }
]);
