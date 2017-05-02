/* jshint esversion: 6 */
const express = require('express');
const builder = require('botbuilder');
const request = require('request');
const sampleApi = require('./api');
const azureSearch = require('./searchApi');

const app = express();
const listenPort = process.env.port || process.env.PORT || 3978;

const azureSearchQuery = azureSearch({
    searchName: process.env.AZURE_SEARCH_ACCOUNT || '',
    indexName: process.env.AZURE_SEARCH_INDEX || '',
    searchKey: process.env.AZURE_SEARCH_KEY || ''
});

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
    session.sendTyping();
    azureSearchQuery('search=' + session.message.text, (err, result) => {
        if (err) {
            session.send('Sorry, something went wrong on our side, please try again latter.');
            return;
        }
        session.replaceDialog('/showFaqResults', { result, originalText: session.message.text });
    });
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
            session.send('Ok, the category is: ' + session.dialogData.category + ' and the severity is: ' + session.dialogData.severity);
        }

        var data = {
            category: session.dialogData.category,
            severity: session.dialogData.severity,
            description: session.dialogData.description,
        }

        request.post('http://localhost:'  + listenPort + '/api/ticket', data, (err, response) => {
            if (err) {
                session.send('Something went wrong while we was recording your issue. Please try again later.')
            } else {
                session.send('Got it. Your ticked has been recorded. Category: ' + session.dialogData.category + ', Severity ' + session.dialogData.severity + ', Description: ' + session.dialogData.description);
            }

            session.endDialog();
        });
    }
]).triggerAction({
    matches: 'SubmitTicket'
});

bot.dialog('DetailsOf', [
    (session, args) => {
        var title = session.message.text.substring('details of'.length + 1);
        azureSearchQuery('$filter=' + encodeURIComponent('title eq \'' + title + '\''), (error, result) => {
            if (error) {
                session.endDialog('Sorry, the article was not found');
            } else {
                session.endDialog(result.value[0].text);
            }
        });
    }
]).triggerAction({
    matches: /^details of (.*)/
});

bot.dialog('/showFaqResults', [
    (session, args) => {
        if (args.result.value.length > 0) {
            var msg = new builder.Message(session).attachmentLayout(builder.AttachmentLayout.carousel);
            args.result.value.forEach((faq, i) => {
                msg.addAttachment(
                    new builder.HeroCard(session)
                        .title(faq.title)
                        .subtitle('Category: ' + faq.category + ' | Search Score: ' + faq['@search.score'])
                        .text(faq.text.substring(0, 50) + '...')
                        .buttons([{ title: 'More details', value: "details of " + faq.title, type: 'postBack' }])
                );
            });
            session.endDialog(msg);
        } else {
            session.endDialog('No results were found for "' + args.originalText + '"');
        }
    }
]);
