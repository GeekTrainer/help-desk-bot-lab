/* jshint esversion: 6 */
require('dotenv').config();
const restify = require('restify');
const builder = require('botbuilder');

// Setup Restify Server
var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, () => {
    console.log('%s listening to %s', server.name, server.url);
});

// Create chat connector for communicating with the Bot Framework Service
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

// Listen for messages from users
server.post('/api/messages', connector.listen());


const Middleware = () => { 
    return {
        botbuilder: (session, next) => {
            console.log(`Middleware logging: ${session.message.text}`);
            next();
        },
        usersent: function (event, next) {
            console.log(`Middleware logging: ${event.text}`);
            next();
        }
    };
};


// Receive messages from the user and respond by echoing each message back (prefixed with 'You said:')
var bot = new builder.UniversalBot(connector, [
    (session, args, next) => {
        session.send(new builder.Message(session).addAttachment({
            contentType: "application/vnd.microsoft.card.adaptive",
            content: {
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "type": "AdaptiveCard",
                "version": "1.0",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "Ticket #1",
                        "weight": "bolder",
                        "size": "large",
                    },
                    {
                        "type": "ColumnSet",
                        "columns": [
                            {
                                "type": "Column",
                                "size": "1",
                                "items": [
                                    {
                                        "type": "FactSet",
                                        "facts": [
                                            {
                                                "title": "Severity:",
                                                "value": "High"
                                            },
                                            {
                                                "title": "Category:",
                                                "value": "Software"
                                            }
                                        ]
                                    }
                                ]
                            },
                            {
                                "type": "Column",
                                "size": "auto",
                                "items": [
                                    {
                                        "type": "Image",
                                        "url": "https://raw.githubusercontent.com/GeekTrainer/help-desk-bot-lab/develop/assets/botimages/head-smiling-medium.png",
                                        "horizontalAlignment": "right"
                                    }
                                ]
                            }
                        ],
                        "separation": "strong"
                    }
                ]
            }
        }));
    }
]);