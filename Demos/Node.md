# Exercise 2: Ticket Submission Dialog

## Text prompts

In the first step of the waterfall dialog we ask the user to describe his problem. In the second one, we receive what the user has entered and print it.

```javascript
var bot = new builder.UniversalBot(connector, [
(session, args, next) => {
    builder.Prompts.text(session, 'First, please briefly describe your problem to me.');
},
(session, result, next) => {
    console.log(result.response);
});
```

## Choice, confirm, buttons in cards

In the first step of the waterfall dialog we ask the user to choose from a closed list the severity. In the second one, we receive what the user has selected and print it.

``` javascript
var bot = new builder.UniversalBot(connector, [
(session, args, next) => {
    var choices = ['high', 'normal', 'low'];
    builder.Prompts.choice(session, 'Which is the severity of this problem?', choices, { listStyle: builder.ListStyle.button });
},
(session, result, next) => {
    console.log(result.response);
});
```

# Exercise 3: Luis Dialog

## Scorables & triggerAction

This dialog will response when the user send `Help` to the bot. We can put a RegEx in the `matches` property.

``` javascript
bot.dialog('Help',
    (session, args, next) => {
        session.endDialog(`I'm the help desk bot and I can help you create a ticket.\n` +
            `You can tell me things like _I need to reset my password_ or _I cannot print_.`);
    }
).triggerAction({
    matches: 'Help'
});
```

More info [here](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-global-handlers).

## Create an Adaptive Card

This dialog send an adaptive Card to the user. You can also read the content of the card from an external file.

``` javascript
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
                    "text": "Ticket #{ticketId}",
                    "weight": "bolder",
                    "size": "large",
                    "speak": "<s>You've created a new Ticket #1</s><s>We will contact you soon.</s>"
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
                },
                {
                    "type": "TextBlock",
                    "text": "I need to reset my password.",
                    "speak": "",
                    "wrap": true
                }
            ]
        }
    }));
});
```

# Exercise 7: Hang Off to Human

## Middleware (simple logging)

This code snippet may be in a separated file. The `botbuilder` method will log the messages the user sends to the bot and the `usersent` the messages the bot sends to the user.

``` javascript
const builder = require('botbuilder');

function MessageLogger() {
    const middleware = () => { return {
            botbuilder: (session, next) => {
                console.log(session.message.text);
                next();
            },
            usersent: function (event, next) {
                console.log(event.text);
                next();
            }
        };
    };

    return {
        middleware,
    };
}

module.exports = MessageLogger;
```

Here we plug the middleware after we initialize the `UniversalBot`.

``` javascript
bot.use(messageLogger.middleware());
```