# Scenario 2: Submitting help desk tickets

The goal of this exercise is to allow the user to submit help desk tickets by using the bot.

## Exercise goals

To successfully complete this exercise, your bot must be able to perform the following tasks:

- Inform the user the current capabilities of the bot
- Ask the user for information about the problem
- Create an in-memory API to store ticket information

### Exercise prerequisites

You must have either completed the prior lab, or you can use the starting point provided for either [C#](./CSharp/exercise1-EchoBot) or [Node.js](./Node/exercise1-EchoBot).

### Introducing the bot to the user

Whenever you create a bot you need to ensure the user knows what options are available to them. This is especially important when working in a conversational based interface, where the user tells the bot what she'd like the bot to do.

### Trouble ticket data model

The trouble ticket needs to store the following information:

- Severity
  - High
  - Normal
  - Low
- Category
  - Software
  - Hardware
  - Network
- Description

The order in which the bot collects the information is up to you.

### In-memory API

Using either [Restify](http://restify.com/) for Node.js, or [Web API](https://www.asp.net/web-api) for C#, create a basic HTTP endpoint to store tickets in memory. The endpoint should accept POST calls with the ticket as the body of the message.

For purposes of this exercise, **no database or other eternal datastore** is needed; simply store the data in an array or list. The endpoint should be part of the same web application that hosts your bot.

> **Real world note** When deploying your application, you may decide to separate your endpoint in a separate application. Typically you will be calling existing APIs.

### Resources

- [Getting started with Web API](https://docs.microsoft.com/en-us/aspnet/web-api/overview/getting-started-with-aspnet-web-api/tutorial-your-first-web-api)
- [Routing in Restify](http://restify.com/#common-handlers-serveruse)
- [Prompt users for input in Node.js](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-dialog-prompt)
- [Dialogs in the Bot Builder SDK for .NET](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-dialogs)

### Further challenges

If you wish to continue working on your own, you can try these tasks:

- Send a welcome message to the bot without waiting for the user to send a message first
- Send a typing indicator when the API is being called