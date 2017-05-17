# Exercise 3: Making the bot with Natural Language Processing (NLP) with LUIS

In this exercise you will learn how to add natural language understanding abilities to the bot to enhance the user experience when creating a help desk ticket. Throughout this lab you will use LUIS (Language Understanding Intelligent Service), which is part of the Azure Cognitive Services offering. LUIS is designed to enable developers to build smart applications that can understand human language and accordingly react to user requests.

One of the key problems in human-computer interactions is the ability of the computer to understand what a person wants. LUIS is designed to enable developers to build smart applications that can understand human language and accordingly react to user requests. With LUIS, a developer can quickly deploy an HTTP endpoint that will take the sentences sent to it and interpret them in terms of their intents (the intentions they convey) and entities (key information relevant to the intent).

## Exercise goals

To successfully complete this exercise, your bot must be able to perform the following actions:

- Allow the user to type a full sentence describing his problem. The system should be able to detect:
  - When the user is submitting a trouble ticket
  - The severity (if provided)
  - The category (if provided)
- Update the bot to use the LUIS model

## Prerequisites

You must have either completed the prior lab, or you can use the starting point provided for either [C#](./CSharp/exercise2-TicketSubmissionDialog) or [Node.js](./Node/exercise2-TicketSubmissionDialog).

## Resources

- [Entities in LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/LUIS/luis-concept-entity-types)
- [Enable language understanding with LUIS in .NET](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-luis-dialogs)
- [Recognize user intent in Node.js](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-recognize-intent)