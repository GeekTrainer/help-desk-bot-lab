/* jshint esversion: 6 */
const restify = require('restify');
const builder = require('botbuilder');
const ticketsApi = require('./ticketsApi');

const listenPort = process.env.port || process.env.PORT || 3978;

// Setup Restify Server
const server = restify.createServer();
server.listen(listenPort, () => {
    console.log('%s listening to %s', server.name, server.url);
});

// Setup body parser and sample tickets api
server.use(restify.bodyParser());
server.post('/api/tickets', ticketsApi);

// Create chat connector for communicating with the Bot Framework Service
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

// Listen for messages from users
server.post('/api/messages', connector.listen());

const luisModelUrl = process.env.LUIS_MODEL_URL || 'https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/e55f7b29-8a93-4342-91da-fde51679f526?subscription-key=833c9b1fa49044c9ab07c79a908639a4&timezoneOffset=0&verbose=true&q=';

var bot = new builder.UniversalBot(connector, (session) => {
    session.send(`I'm sorry, I did not understand '${session.message.text}'. I'm the help desk bot and I can help you create a ticket. You can tell me things like _I need to reset my password_ or _I cannot print_.`);
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

        if (!session.dialogData.severity) {
            var choices = ['high', 'normal', 'low'];
            builder.Prompts.choice(session, 'Which is the severity of this problem?', choices);
        } else {
            next();
        }
    },
    (session, result, next) => {
        if (!session.dialogData.severity) {
            session.dialogData.severity = result.response.entity;
        }

        if (!session.dialogData.category) {
            builder.Prompts.text(session, 'Which would be the category for this ticket (software, hardware, network, and so on)?');
        } else {
            next();
        }
    },
    (session, result, next) => {
        if (!session.dialogData.category) {
            session.dialogData.category = result.response;
        }

        var message = `Great! I'm going to create a **${session.dialogData.severity}** severity ticket in the **${session.dialogData.category}** category. ` +
                      `The description I will use is _"${session.dialogData.description}"_. Can you please confirm that this information is correct?`;
                      
        builder.Prompts.confirm(session, message);
    },
    (session, result, next) => {
        if (result.response) {
            var data = {
                category: session.dialogData.category,
                severity: session.dialogData.severity,
                description: session.dialogData.description,
            }

            const client = restify.createJsonClient({ url: `http://localhost:${listenPort}` });

            client.post('/api/tickets', data, (err, request, response, ticketId) => {
                if (err || ticketId == -1) {
                    session.send('Ooops! Something went wrong while I was saving your ticket. Please try again later.')
                } else {
                    session.send(`Awesome! Your ticked has been created with the number ${ticketId}.`);
                }

                session.endDialog();
            });
        } else {
            session.endDialog('Ok. The ticket was not created. You can start again if you want.');
        }
    }
]).triggerAction({
    matches: 'SubmitTicket'
});
