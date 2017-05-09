# Exercise 4: Implementing a Help Desk Knowledge Base with Azure Search and DocumentDB (Node.js)

## Introduction

In this exercise you will learn how to help users navigate a knowledge base by using the bot.

[Azure Search](https://azure.microsoft.com/en-us/services/search/) is a service that offers most of the needed pieces of functionality for Search capabilities, including keyword search, built-in linguistics, custom scoring, faceted navigation and more. Azure Search can also index content from various sources (Azure SQL DB, DocumentDB, Blob Storage, Table Storage), supports "push" indexing for other sources of data, and can crack open PDFs, Office documents and other formats containing unstructured data. The content catalog goes into an Azure Search index, which we can then query from dialogs.

Inside [this folder](./exercise4-KnowledgeBase) you will find a solution with the code that results from completing the steps in this hands-on lab. You can use this solutions as guidance if you need additional help as you work through this hands-on lab. Remember that for using it, you first need to run `npm install`.

## Prerequisites

The following software is required for completing this hands-on lab:

* [Latest Node.js with NPM](https://nodejs.org/en/download/)
* A code editor like [Visual Studio Code](https://code.visualstudio.com/download) or Visual Studio 2017 Community, Professional, or Enterprise
* An Azure Subscription - you can signup for a free trial [here](https://azureinfo.microsoft.com/us-freetrial.html?cr_cc=200744395&wt.mc_id=usdx_evan_events_reg_dev_0_iottour_0_0)
* Creating an account in the LUIS Portal [here](https://www.luis.ai)
* The Bot Framework Emulator - download it from [here](https://emulator.botframework.com/)

## Task 1: Create a Document DB Service and Upload the Knowledge Base

You can learn more about Document DB Concepts here: https://docs.microsoft.com/en-us/azure/documentdb/documentdb-resources
// TODO: rename the `faq` collection to `knowledge-base`

1. Sign in on [https://portal.azure.com](https://portal.azure.com)

2. Setup DocumentDB
    1. Provision a new DocumentDB database.
        1. From the Azure portal dashboard, Click on + (new) button.
        2. Click on _Databases_ -a new _blade_ opens-
        3. Click on _NoSQL (DocumentDB)_ -a new _blade_ opens-
            1. Enter the _account ID._ (i.e. the database identifier)
            2. Leave _NoSQL API_ as _DocumentDB_
            3. Choose an existing Resource Group (or create a new one)
            4. (optional) Choose a location for the account.
            5. (optional) Pin to dashboard for easy access.
            6. Click on _Create_
        4. Wait for the new database deployment is completed
    2. Provision a new DocumentDB collection.
        1. Open the previously created _DocumentDB account_
        2. Click on _Overview_
        3. Click on + Add Collection -a new _blade_ opens-
            1. Enter &quot;faq&quot; on _Collection Id_
            2. Select 10GB on _Storage Capacity_
            3. _Set 400 on Throughput Capacity_
            4. Leave _Partition key_ empty
            5. Enter &quot;newdbname&quot; on _Database_
            6. Click on _OK_
    3. Import data from Json
        // TODO: Use the Document Explorer to upload the 17 files to DocumentDB
        
    4. Verify the imported data
        1. Back on the _DocumentDB account_ click on _Collections\Data Explorer_
        2. Browse to _newdbname\faq_ and verify that documents were on there

## Task 2: Create the Azure Search Service

1. Log into the Azure Portal with your Azure Subscription credentials.

2. Azure Search
    1. Provision an Azure Search account
        1. From the Azure portal dashboard, Click on + (new) button.
        2. Click on _Web + Mobile_ -a new _blade_ opens-
        3. Click on _Azure Search_ -a new _blade_ opens-
            1. Enter some _service name_ (i.e. the azure search identifier)
            2. Choose an existing Resource Group (or create a new one)
            3. (optional) Choose a location for the account.
            4. (optional) Pin to dashboard for easy access.
            5. Choose a _Pricing Tier_
            6. Click on _Create_
        4. Wait for the new azure search deployment is completed
    2. Import data from DocumentDB
        1. Open the previously created _Azure Search account_
        2. Click on _Overview_
        3. Click on _Import Data -a new blade opens-_
            1. Click on &quot;Connect to your data&quot; _-a new blade opens-_
                1. Click on _DocumentDB -a new blade opens-_
                    1. Enter &quot;faq-datasource&quot; on Name field
                    2. Click on _Select an Account -a new blade opens-_
                        1. Choose the DocumentDB created on step 3.b
                    3. Select &quot;newdbname&quot; on the _Database_ dropdown
                    4. Select &quot;faq&quot; on the _Collection_ dropdown
                    5. Click _OK_ button
            2. (the Customize target index _blade_ opens)
                1. Enter &quot;faq-index&quot; as the index name.
                2. Make sure to complete the matrix as follow:
                3. ![scenario5-faq=index-facets-matrix](./images/scenario5-faq-index-facets-matrix.png)
                1. Click OK
            3. (the Import Your data _blade_ opens)
                1. Enter &quot;faq-indexer&quot; as indexer name
                2. Leave &quot;Once&quot; as _Schedule_ strategy
                3. Click OK.
            4. Back on the Import data salde, click OK. (This should create the Azure Search Index)
    3. Once the index is created:
        1. Copy the **Azure Search account name** for future usage.
        2. Copy the **Azure Search index name** for future usage. (in this example was &#39;faq-index&#39;)
        3. Back on the _Azure Search account_ click on _Settings\Keys - a new blade opens -_
            1. Click on _Manage query keys - a new blade opens -_
            2. Copy the **Azure Search key** for future usage.  (identified by &#39;&lt;empty&gt;&#39; name)

## Task 3: Create Azure Search API Client

1. Create the azureSearchApiClient.js and paste the code.
1. Add the require and client in app.js.

    ```
    const azureSearchQuery = azureSearch({
        searchName: process.env.AZURE_SEARCH_ACCOUNT || 'bot-framework-training',
        indexName: process.env.AZURE_SEARCH_INDEX || 'faq-index',
        searchKey: process.env.AZURE_SEARCH_KEY || '0690536062B90F1BE86342AB8B7A5281'
    });
    ```

## Task 4: Update the LUIS Model to Include the ExploreKnowledgeBase Intent

// We make the user add the intent and add a couple of Utterances.

## Task 5: Update the Bot to Display the Articles Categories Available in the Search Index

1. Add the `ExploreKnowledgeBase`handler to retrieve the Facets
    
    // ESTE CODIGO ESTA TESTEADO
    ```
    bot.dialog('ExploreKnowledgeBase', [
        (session, args) => {
            var category = builder.EntityRecognizer.findEntity(args.intent.entities, 'category');
            if (!category) {
                return session.endDialog('Try typing something like _explore hardware_.');
            }
            // search by category
            azureSearchQuery('$filter=' + encodeURIComponent(`category eq '${category.entity}'`), (error, result) => {
                if (error) {
                    session.endDialog('Ooops! Something went wrong while contacting Azure Search. Please try again later.');
                } else {
                    var msg = `These are some articles I\'ve found in the knowledge base for the _'${category.entity}'_ category:`;
                    result.value.forEach((article) => {
                        msg += `\n * ${article.title}`;
                    });
                    session.endDialog(msg);
                }
            });
        }
    ]).triggerAction({
        matches: 'ExploreKnowledgeBase'
    });
    ```

1. Run the bot and show how the articles are retrieved.

## Task 6: Update the Bot to Display the Article

1. Update the default dialog to perform a search.

    ```
    var bot = new builder.UniversalBot(connector, (session) => {
        session.sendTyping();
        azureSearchQuery(`search=${encodeURIComponent(session.message.text)}`, (err, result) => {
            if (err) {
                session.send('Ooops! Something went wrong on my side, please try again later.');
                return;
            }
            session.replaceDialog('ShowKBResults', { result, originalText: session.message.text });
        });
    });
    ```

1. Add the full code of the ExploreKnowledgeBase Dialog.

1. Add the DetailsOf dialog.

1. Add the ShowKBResults dialog.

1. Test the bot.
