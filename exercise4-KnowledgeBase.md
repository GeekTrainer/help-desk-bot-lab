# Exercise 4: Implementing a Help Desk Knowledge Base with Azure Search and Cosmos DB (C#)

Your bots can also help the user navigate large amounts of content and create a data-driven exploration experience for users. In this exercise you will learn how to add search functionality to the bot to help users explore a knowledge base. To do this, you will connect the Bot to the Azure Search service that will index KB articles stored in an Azure Cosmos DB.

[Azure Cosmos DB](https://azure.microsoft.com/en-us/services/cosmos-db/) is Microsoft's globally distributed, multi-model database service for mission-critical applications. It supports different data models. In this exercise you will use its Document DB API, that will allow you to store knowledge base articles as JSON documents.

[Azure Search](https://azure.microsoft.com/en-us/services/search/) is a fully managed cloud search service that provides a rich search experience to custom applications. Azure Search can also index content from various sources (Azure SQL DB, Cosmos DB, Blob Storage, Table Storage), supports "push" indexing for other sources of data, and can open PDFs, Office documents and other formats containing unstructured data. The content catalog goes into an Azure Search index, which you can then query from bot dialogs.

## Goals

To successfully complete this exercise, your bot must be able to perform the following actions:

* Respond to an utterance like _explore knowledge base_ listing the different categories of articles (software, hardware, networking, security) and asking the user to choose one. If a category is typed, list some articles under that category.
* If the user types 'explore hardware' do not ask for the category and list the articles under the hardware category (use LUIS language understanding for this).
* Every article shown by the bot should have a "More Details" button that displays the article content
* If the user types 'show me the article Turn off OneDrive in windows 10' the bot should look an article with the "Turn off OneDrive in windows 10" in Azure Search.
* If the user types 'search onedrive' the bot should search the knowledge base with the OneDrive keyword.

Here is a sample interaction with the bot:

![exercise4-emulator-showkbresults](./CSharp/images/exercise4-emulator-showkbresults.png)

## Prerequisites

* You must have either completed the prior exercise, or you can use the starting point provided for either [C#](./CSharp/exercise3-LuisDialog) or [Node.js](./Node/exercise2-LuisDialog).
* An account in the [LUIS Portal](https://www.luis.ai)
* An [Azure](https://azureinfo.microsoft.com/us-freetrial.html?cr_cc=200744395&wt.mc_id=usdx_evan_events_reg_dev_0_iottour_0_0) subscription

## Create and Configure the Azure Services

For sample articles for your Knowledge Base, use the files in the [assets/kb](./assets/kb) folder.

## Update the LUIS Model to Include the ExploreKnowledgeBase Intent

You need to add a new intent to your LUIS Model to handle the intent to search and explore the knowledge base. Some sample utterances can be:
    * explore knowledge base
    * explore hardware articles
    * find me articles about hardware

## Update the Bot to call the Azure Search API

You should add code to query Azure Search using its API. The URL you should use should look something like:

```
https://helpdeskbotsearch.search.windows.net/indexes/knowledge-base-index/docs?api-key=...&api-version=2015-02-28&{query_placeholder}
```
Where the `{query_placeholder}` can be something like:
* $filter='category eq hardware'
* search=OneDrive
* facet=category

For more information see these articles:
    * [Query your Azure Search index](https://docs.microsoft.com/en-us/azure/search/search-query-overview)
    * [OData Expression Syntax for Azure Search](https://docs.microsoft.com/en-us/rest/api/searchservice/odata-expression-syntax-for-azure-search)

## Update the Bot to Display Categories and Articles

