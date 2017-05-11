# Exercise 6: Determine the Mood Behind a User's Message (Node.js)

## Introduction

...

Inside [this folder](./exercise6-MoodDetection) you will find a solution with the code that results from completing the steps in this exercise. You can use this solutions as guidance if you need additional help as you work through this exercise. Remember that for using it, you first need to run `npm install`.

## Prerequisites

The following software is required for completing this exercise:

* [Latest Node.js with NPM](https://nodejs.org/en/download/)
* A code editor like [Visual Studio Code](https://code.visualstudio.com/download) or Visual Studio 2017 Community, Professional, or Enterprise
* An Azure Subscription - you can sign up for a free trial [here](https://azureinfo.microsoft.com/us-freetrial.html?cr_cc=200744395&wt.mc_id=usdx_evan_events_reg_dev_0_iottour_0_0)
* The Bot Framework Emulator - download it from [here](https://emulator.botframework.com/)

## Task 1: Create Text Analytics API Key

1. Browse [here](https://azure.microsoft.com/en-us/try/cognitive-services/), select the **Language** tab. Find the Text Anlytics API and click **Create**. Login with the account of your Azure Subscription. You should be taken to a page like the following one with an evaluation key with 5000 free requests.

    ![exercise6-text-analytics-keys](./images/exercise6-text-analytics-keys.png)

1. Copy one of the keys.

## Task 1: Modify the Bot to Ask for Feedback and Determine the Mood

1. Open the **app.js** file you've obtained from exercise 4. Alternatively, you can open the file from the [exercise4-KnowledgeBase](./exercise4-KnowledgeBase) folder.

// TODO: add the `textAnalyticsApiClient.js` file and code
// TODO: add the `UserFeedbackRequest` Dialog and the `session.replaceDialog('UserFeedbackRequest');` in the submit ticket dialog

## Task 3: Test the Bot from the Emulator

