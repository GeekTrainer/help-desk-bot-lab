/* jshint esversion: 6 */
const restify = require('restify');
const builder = require('botbuilder');
const ticketsApi = require('./ticketsApi');
const azureSearch = require('./azureSearchApiClient');

const azureSearchQuery = azureSearch({
    searchName: process.env.AZURE_SEARCH_ACCOUNT,
    indexName: process.env.AZURE_SEARCH_INDEX,
    searchKey: process.env.AZURE_SEARCH_KEY
});

const listenPort = process.env.port || process.env.PORT || 3978;

// Setup Restify Server
const server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, () => {
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

        var message = `Great! I'm going to create a ${session.dialogData.severity} severity ticket in the "${session.dialogData.category}" category. ` +
                      `The description I will use is "${session.dialogData.description}". Can you confirm that this information is correct?`;

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
                    session.send('Something went wrong while I was saving your ticket. Please try again later.')
                } else {
                    session.send(`Awesome! Your ticked has been created with the number ${ticketId}.`);
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

bot.dialog('ExploreCategory', [
    (session, args) => {
        var category = builder.EntityRecognizer.findEntity(args.intent.entities, 'category');

        if (!category) {
            // retrieve facets
            azureSearchQuery('facet=category', (error, result) => {
                if (error) {
                    session.endDialog('Sorry, something went wrong while contacting Azure Search. Try again later.');
                } else {
                    var choices = result['@search.facets'].category.map(item=> `${item.value} (${item.count})`);
                    builder.Prompts.choice(session, 'Which category are you interested in?', choices);
                }
            });
        } else {
            // search by category
            azureSearchQuery('$filter=' + encodeURIComponent(`category eq '${category.entity}'`), (error, result) => {
                if (error) {
                    session.endDialog('Sorry, something went wrong while contacting Azure Search. Try again later.');
                } else {
                    session.replaceDialog('/showFaqResults', { result, originalText: session.message.text });
                }
            });
        }
    },
    (session, args) => {
        var category = args.response.entity.replace(/\s\([^)]*\)/,'');
        // search by category
        azureSearchQuery('$filter=' + encodeURIComponent(`category eq '${category}'`), (error, result) => {
            if (error) {
                session.endDialog('Sorry, something went wrong while contacting Azure Search. Try again later.');
            } else {
                session.replaceDialog('/showFaqResults', { result, originalText: category });
            }
        });
    }
]).triggerAction({
    matches: 'ExploreCategory'
});

bot.dialog('DetailsOf', [
    (session, args) => {
        var title = session.message.text.substring('show details of article '.length);
        azureSearchQuery('$filter=' + encodeURIComponent(`title eq '${title}'`), (error, result) => {
            if (error) {
                session.endDialog('Sorry, the article was not found');
            } else {
                session.endDialog(result.value[0].text);
            }
        });
    }
]).triggerAction({
    matches: /^show details of article (.*)/
});

bot.dialog('/showFaqResults', [
    (session, args) => {
        if (args.result.value.length > 0) {
            var msg = new builder.Message(session).attachmentLayout(builder.AttachmentLayout.carousel);
            args.result.value.forEach((faq, i) => {
                msg.addAttachment(
                    new builder.HeroCard(session)
                        .title(faq.title)
                        .subtitle(`Category: ${faq.category} | Search Score: ${faq['@search.score']}`)
                        .text(faq.text.substring(0, Math.min(faq.text.length, 50) + '...'))
                        .buttons([{ title: 'More details', value: `show details of article ${faq.title}`, type: 'postBack' }])
                );
            });
            session.endDialog(msg);
        } else {
            session.endDialog(`No results were found for '${args.originalText}'`);
        }
    }
]);
