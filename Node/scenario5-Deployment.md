# Deployment Steps

## 1 - Publish LUIS model
1. Sign in on [https://luis.ai](https://luis.ai)
2. Click on _My Apps_ then on _Import App_
3. Click on _browse_ and select the **&lt;project&gt;/data/luis\_model.json** file.
4. Click on _Import,_ a new LUIS App is created.
5. Open the new created LUIS App.
6. Click on _Publish App_
    1. Select &quot;BootstrapKey&quot; on the _Endpoint Key_ dropdown.
    2. Click on _Train_ button.
    3. Click on _Publish_ button.
7. Copy the **Endpoint URL** for future usage. We will call this value as **LUIS model URL.**

## 2 - Create the Bot in the bot directory
1. Sign in on [https://dev.botframework.com](https://dev.botframework.com)
2. Click on _Register a Bot_
    1. Complete the Name. Handle &amp; Description fields
    2. Leave the _Messaging endpoint_ blank for now (it will be edited later)
    3. Click on _Create Microsoft App ID and password  _(a new window opens)
        1. Copy _App Name_ for future usage. We will call it as **Bot App Name**.
        2. Copy _App ID_ for future usage. We will call it as **Bot App ID**.
        3. Click on _Generate an app password to continue  _(a popup opens)
        4. Copy the given password for future usage. Notice that this is the only time when it will be displayed. Store it securely. We will call it as **Bot Password**.
        5. Click Ok, then click on _Finish and go back to Bot Framework_ button.
3. Back on bot creation from, click on _&quot;I Agree to the terms of Use … &quot;_ checkbox.
4. Click on Register.

## 3 - Deploy on azure
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
        1. Back on the _DocumentDB account_ click on _Settings\Keys_
        2. Click on _Read-write Keys_
        3. Copy to the clipboard the _Primary Connection String_
        4. [Download](https://www.microsoft.com/en-us/download/details.aspx?id=46436) and unzip the [Database migration tool](https://docs.microsoft.com/en-us/azure/documentdb/documentdb-import-data)
            1. Open the _dtui.exe_ file from the extracted folder.
            2. Click on Source Infromation.
            3. Set _Import from_ to be &quot;JSON file(s)&quot;
            4. Click on _Add Files and select_ **&lt;project&gt;/data/**** knowledge\_base.json** file
            5. Click on _Next (or click on Target Information)_
            6. Paste from the clipboard the previously copied _DocumentDB Primary Connection String_  and append at the end the following &quot;_database=_newdbname_&quot;_ (without quotes) which is the database name required to access import the data.
            7. Enter &quot;faq&quot; on Collection field.
            8. Click on &quot;Verify&quot; Button, a success message should appear.
            10. Click on &quot;Next&quot; Button twice.
            11. Click on &quot;Import&quot; Button.
            12. All records should be imported successfully
    4. Verify the imported data
        1. Back on the _DocumentDB account_ click on _Collections\Data Explorer_
        2. Browse to _newdbname\faq_ and verify that documents were on there
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
3. App Service
    1. Provision the App Service account
        1. From the Azure portal dashboard, Click on + (new) button.
        2. Click on _Web + Mobile_ -a new _blade_ opens-
        3. Click on _Web App_ -a new _blade_ opens-
            1. Enter a name for your App
            2. Choose an existing Resource Group (or create a new one)
            3. (optional) Choose a location for the account.
            4. Choose a _Pricing Tier_
            5. (optional) Pin to dashboard for easy access.
            6. Click on _Create_
        4. Wait for the new azure web app deployment is completed
    2. Provision App Settings
        1. Specific App Settings for Node.Js version
            1. Open the previously created _App Service account Add the following app settings:_
                1. AZURE\_SEARCH\_ACCOUNT -&gt; Use the **Azure Search account name**
                2. AZURE\_SEARCH\_INDEX -&gt; Use the **Azure Search index name**
                3. AZURE\_SEARCH\_KEY-&gt; Use the **Azure Search key**
                4. MICROSOFT\_APP\_ID -&gt; Use the **Bot App ID**
                5. MICROSOFT\_APP\_PASSWORD -&gt; Use the **Bot Password**
                6. LUIS\_MODEL\_URL -&gt; Use the **LUIS model URL**
        2. Specific App Settings for C# version
            1. TBD
    3. Provision deployment credentials
        1. Open the previously created _App Service account_
        2. Click on Deployment\Deployment credentials _-a new blade opens-_
            1. Choose a username for deployment, set it on _deployment username_ field.
            2. Set a password and its verification.
            3. Click on _Save_ button
        3. Click on Deployment\Deployment options _-a new blade opens-_
            1. Click on _Setup -a new blade opens-_
                1. Click on _Choose Source_ and select _Local Git Repository_
            2. Click on _OK_ button
        4. Back on the _App Service account c_lick on Overview _-a new blade opens-_
            1. Copy the **Git Clone URL** _ _for future use.
    4. Deploy a Node.js written bot
        1. These instructions assume that you already have created your Node project which incidentally uses git as source control software.
        2. Do: `git remote add azure <git clone url>` (where **Git Clone URL** is the value obtained in previous steps)
        3. Do: `git push azure master` (assuming you want to deploy master branch)
        4. Browse to the site and verify that it is up and running.
    5. Deploy a C# written bot
        1. Open the Step4.sln solution via VS2015
        2. Right click on the project, click on _Publish.._
        3. Select &quot;Microsoft Azure Web Apps&quot; as _publish target_
        4. **TBD**
    6. Copy the **Endpoint URL** for future usage (e.g. [https://host.azurewebsite.net/api/message](https://host.azurewebsite.net/api/message)) We will call this value as **Bot Service URL.**

## 4 - Publish the Bot in the bot directory
1. Once the App Service was created, up and running, go back to the [https://dev.botframework.com](https://dev.botframework.com) dashboard to edit the bot configuration.
2. Edit your bot configuration  _Messaging endpoint_ field with the **Bot Service URL** previously obtained.
3. Click on _Save changes_ button
4. Click on _Publish_ button
5. Complete the _Publisher from_ and click on _Submit for review_.
6. TBD