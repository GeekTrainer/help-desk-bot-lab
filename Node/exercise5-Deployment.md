# Exercise 5: Deploying Your Bot to the Cloud (Node.js)

// In the end, this lab should show how to connect from the emulator with authentication enabled.

## 1 - Create the Bot in the bot directory

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
        1. Open the exercise4.sln solution via VS2015
        2. Right click on the project, click on _Publish.._
        3. Select &quot;Microsoft Azure Web Apps&quot; as _publish target_
        4. **TBD**
    6. Copy the **Endpoint URL** for future usage (e.g. [https://host.azurewebsite.net/api/message](https://host.azurewebsite.net/api/message)) We will call this value as **Bot Service URL.**

// TODO: Add a note about publishing de bot in the directory. Do not publish.