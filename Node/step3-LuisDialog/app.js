/* jshint esversion: 6 */
const express = require('express');
const bodyParser = require('body-parser');
const builder = require('botbuilder');
const request = require('request');
const sampleApi = require('./ticketsApi');

const app = express();
const listenPort = process.env.port || process.env.PORT || 3978;

app.use(bodyParser.json());

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

const luisModelUrl = process.env.LUIS_MODEL_URL || 'https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/e55f7b29-8a93-4342-91da-fde51679f526?subscription-key=833c9b1fa49044c9ab07c79a908639a4&timezoneOffset=0&verbose=true&q=';

var bot = new builder.UniversalBot(connector, (session) => {
    session.send('Sorry, I did not understand \'%s\'', session.message.text);
});

bot.recognizer(new builder.LuisRecognizer(luisModelUrl));

bot.dialog('SubmitTicket', [
    (session, args, next) => {
        var category = builder.EntityRecognizer.findEntity(args.intent.entities, 'category');
        var severity = builder.EntityRecognizer.findEntity(args.intent.entities, 'severity');

        if (category && category.resolution.values.length > 0) {
            session.dialogData.category = category.resolution.values[0];
        }

        if (severity && severity.resolution.values.length > 0) {
            session.dialogData.severity = severity.resolution.values[0];
        }

        session.dialogData.description = session.message.text;

        if (!session.dialogData.category) {
            builder.Prompts.text(session, "Type the category");
        } else {
            next();
        }
    },
    (session, result, next) => {
        if (!session.dialogData.category) {
            session.dialogData.category = result.response;
            session.send('Ok, the category is: ' + session.dialogData.category);
        }

        if (!session.dialogData.severity) {
            var choices = ['High', 'Normal', 'Low'];
            builder.Prompts.choice(session, 'Choose the severity', choices);
        } else {
            next();
        }
    },
    (session, result, next) => {
        if (!session.dialogData.severity) {
            session.dialogData.severity = result.response.entity;
        }

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

            request({ method: 'POST', url: 'http://localhost:'  + listenPort + '/api/ticket', json: true, body: data }, (err, response) => {
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
]).triggerAction({
    matches: 'SubmitTicket'
});
