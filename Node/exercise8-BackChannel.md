# Exercise 8: Send and receive events through Back Channel (Node.js)

## Introduction

The [open source Web Chat Control](https://github.com/Microsoft/BotFramework-WebChat) communicates with bots by using the [Direct Line API](https://docs.botframework.com/en-us/restapi/directline3/#navtitle), which allows `activities` to be sent back and forth between client and bot. The most common type of activity is `message`, but there are other types as well. For example, the activity type `typing` indicates that a user is typing or that the bot is working to compile a response.

You can use the backchannel mechanism to exchange information between client and bot without presenting it to the user by setting the activity type to `event`. The web chat control will automatically ignore any activities where `type="event"`.

Essentially the backchannel allows client and server to exchange any data needed, from requesting the client's time zone to reading a GPS location or what the user is doing on a web page. The bot can even guide the user by automatically filling out parts of a form and so on. The backchannel closes the gap between client JavaScript and bots.

In this exercise, the bot and web page will use the backchannel mechanism to exchange information that is invisible to the user. The bot will request that the web page show the knowledge base search results, and the web page will request that the bot show the detail of an article.

Inside [this folder](./exercise8-BackChannel) you will find a solution with the code that results from completing the steps in this exercise. You can use this solutions as guidance if you need additional help as you work through this exercise. Remember that for using it, you first need to run `npm install` and complete the placeholders of the **Web Chat Channel Secrets**.

If you are not following all the exercises maybe you don't have already register your bot. To know how to register your bot, please refer to the [Exercise 5](./exercise5-Deployment.md).

## Prerequisites

The following software is required for completing this exercise:

* [Latest Node.js with NPM](https://nodejs.org/en/download/)
* A code editor like [Visual Studio Code](https://code.visualstudio.com/download) (preferred), or Visual Studio 2017 Community or higher
* The [Bot Framework Emulator](https://emulator.botframework.com) (make sure it's configured with the `en-US` Locale)
* Download [ngrok](https://ngrok.com/)

## Task 1: Add a New Site to your Bot's Web Chat Channel

1. Sign in to the [Bot Framework Portal](https://dev.botframework.com).

1. Click the **My bots** button and next click on your bot for editing it.

1. Click on the **Edit** (![exercise8-edit](./images/exercise8-edit.png)) link for the _Web Chat_ channel. In the opened window, click **Add new site**. Enter a _site name_ (for example, _Help Desk Ticket Search_), Site name is for your reference and you can change it anytime.

    ![exercise8-addnewsite](./images/exercise8-addnewsite.png)

1. Click **Done** and you may see a page as follow. Notice you have two **Secret Keys**. Safe for late use one of them. Click the **Done** button at page bottom.

    ![exercise8-webchatsecrets](./images/exercise8-webchatsecrets.png)

## Task 2: Add HTML Page with an Embedded Web Chat

In this task you will add a HTML page to your server which contains the web chat control and the code to send/receive `event` messages to/from your bot.

1. Copy the folder `web-ui` from the [assets/exercise8](../assets/exercise8-BackChannel/) folder to the app root folder. Inside you will find a _index.html_ file which contains the markup, style and code for the web page. The JavaScript code may be break down in these sections:

    * Imports the latest `botchat.js` version from a CDN, so you don't need to worry about update or host it.

    * The web page creates a **DirectLine** object with the **Web Channel Secret**:

        ``` javascript
        var botConnection = new BotChat.DirectLine({
            secret: '{YourWebChannelSecret}'
        });
        ```

    * It shares this when creating the Web Chat instance:

        ``` javascript
        BotChat.App({
            botConnection: botConnection,
            user: { id: 'WebChatUser' },
            bot: { id: '{YourBotID}' },
            locale: 'en-us',
        }, document.getElementById("bot"));
        ```

    * The client JavaScript listens for a specific event (`searchResults`) from the bot:

        ``` javascript
        botConnection.activity$
            .filter(function (activity) {
                return activity.type === `event` && activity.name === 'searchResults';
            })
            .subscribe(function (activity) {
                updateSearchResults(activity.value)
            });
        ```

    * The `updateSearchResults` method parse the message sent from the bot and build the list of articles in the page and add a click event to the title element that post to the bot an event that tells to show the details of the article inside the **Web Chat Control**.

        ``` javascript
        ...
        botConnection
            .postActivity({
                type: `event`,
                value: this.textContent.trim(),
                from: { id: 'user' },
                name: 'showDetailsOf'
            });
        ...
        ```

1. Open the **index.html** file and replace the `{YourWebChannelSecret}` placeholder with the _Web Chat Secret_ you've obtained from the last task and the `{YourBotID}` placeholder with the _Bot Id_ (if you don't remember this, you can obtain from the _Setting_ tab from your **Bot Framework portal**, under the _Bot handle_ field.)

## Task 3: Update your Bot to send and receive `event` messages

In this task, you will add the ability to send and receive `event` messages to your bot.

1. Add the following `require` statement in the require section.

    ``` javascript
    const path = require('path');
    ```

1. In the **app.js**, before the `var bot = new builder.UniversalBot(...);`, add the following code which tells _Restify_ to serve the `web-ui/index.html` file as the default web page.

    ``` javascript
    server.get(/\/?.*/, restify.serveStatic({
        directory: path.join(__dirname, 'web-ui'),
        default: 'index.html'
    }));
    ```

1. In the first step on the waterfall for the **SubmitTicket** dialog, add just after where you store the message in the `session.dialogData.description` the code below to send a event with the search result to the web page.

    ``` javascript
    azureSearchQuery(`search=${encodeURIComponent(session.message.text)}`, (err, result) => {
        if (err || !result.value) return;
        var event = createEvent('searchResults', result.value, session.message.address);
        session.send(event);
    });
    ```

1. Add the `createEvent` as follows. This method build a message with `event` as type and eventName for the case you wish to implement others `event` messages.

    ``` javascript
    const createEvent = (eventName, value, address) => {
        var msg = new builder.Message().address(address);
        msg.data.type = `event`;
        msg.data.name = eventName;
        msg.data.value = value;
        return msg;
    };
    ```

1. Add the following event listener registration which will be called when the user clicks in an article's title. This method will search for article's titles in the **Knowledge Base** with the title requested and then send the result article to user in the Web Chat. Click [here](https://docs.botframework.com/en-us/node/builder/chat-reference/classes/_botbuilder_d_.universalbot.html#on) for more information about the `on` event listener.

    ``` javascript
    bot.on(`event`, function (event) {
        var msg = new builder.Message().address(event.address);
        msg.data.textLocale = 'en-us';
        if (event.name === 'showDetailsOf') {
            azureSearchQuery('$filter=' + encodeURIComponent(`title eq '${event.value}'`), (error, result) => {
                if (error || !result.value[0]) {
                    msg.data.text = 'Sorry, I could not find that article.';
                } else {
                    msg.data.text = result.value[0].text;
                }
                bot.send(msg);
            });
        }
    });
    ```

## Task 4: Test the Bot from the Web Page

1. Run the app from a console (`nodemon app.js`).

1. Open a new console window where you've downloaded _ngrok_ and type `ngrok http 3978`. Notice that `3978` is the port number where your bot is running. Change if you are using another port number. Next, save for later use the Forwarding **https** URL.

    ![exercise8-ngrok](./images/exercise8-ngrok.png)

1. Sign in to the [Bot Framework Portal](https://dev.botframework.com).

1. Click the **My bots** button and next click on your bot for editing it. Click on the **Settings** tab and update the _Messaging endpoint_ URL (remember to keep the `/api/messages`). Click in the **Save changes** button.

1. In a Web Browser, navigate to your bot URL (http://localhost:3978/ as usual). On the Web Chat Control, type `I need to reset my password, this is urgent`. You should see the ticket confirmation message as usual in the Web Chat Control. Notice the article list in the right is populated based on the description you entered.

    ![exercise8-webchatarticles](./images/exercise8-webchat-articles.png)

1. Click on the title of any of the articles on the right and next you should see the details of the article displayed in the bot Web Chat Control.

    ![exercise8-webchat-articlesdetail](./images/exercise8-webchat-articlesdetail.png)
