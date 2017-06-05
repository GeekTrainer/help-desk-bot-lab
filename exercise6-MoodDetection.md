# Exercise 6: Determine the Sentiments Behind a User's Message

The interaction between users and bots is mostly free-form, so bots need to understand language naturally and contextually. In this exercise you will learn how to detect the user's sentiments and mood using the Azure Text Analytics API.

With [Text Analytics APIs](https://azure.microsoft.com/en-us/services/cognitive-services/text-analytics/), part of the Azure Cognitive Services offering, you can detect sentiment, key phrases, topics, and language from your text. The API returns a numeric score between 0 and 1. Scores close to 1 indicate positive sentiment and scores close to 0 indicate negative sentiment. Sentiment score is generated using classification techniques.

## Goals

To successfully complete this exercise, your bot must be able to perform the following actions:

* Ask the user for feedback when complete the ticket submission.
* Send to analyze the user feedback to detect positive or negative sentiments to Text Analytics Service.
* Evaluate the score returned and send different message for positive and negative sentiment.

## Prerequisites

* You must have either completed the prior exercise, or you can use the starting point provided for either [C#](./CSharp/exercise4-KnowledgeBase) or [Node.js](./Node/exercise4-KnowledgeBase)
* An [Azure](https://azureinfo.microsoft.com/us-freetrial.html?cr_cc=200744395&wt.mc_id=usdx_evan_events_reg_dev_0_iottour_0_0) subscription

## Create the Text Analytics Service

You need to create a Text Analytics Key from the Language tab in the [Cognitive Services Portal](https://azure.microsoft.com/en-us/try/cognitive-services/). Next,
you could create a Service Client utility which ease the communication from the Bot to the **Text Analytics Service** using the given Key.

## Modify the Bot to Ask for Feedback and Analyze the User's Sentiments

You can create a new Dialog to ask for user experience feedback an call it when the ticket submission creation is complete. Then, call the **Text Analytics Service** using the Client created and response to user with different message based on the result. If the feedback is positive (score >= 0.5) the bot can thanks user to share her feedback. Otherwise (score < 5), you can tell the user that an IT responsible will contact soon. Note that in the next exercise (7) you will learn how to hand-off the conversation to a human so he can assist the user.

## Further Challenges

* You can add Speech Recognition to the bot by using another Microsoft Cognitive Services. You can try the [Bing Speech API](https://azure.microsoft.com/en-us/services/cognitive-services/speech/).