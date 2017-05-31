# Exercise 8: [Title] (C#)

## Introduction

[intro about back channel, events and ways to interact with bots - it should match Node version]

Inside [this folder](./exercise8-BackChannel) you will find a solution with the code that results from completing the steps in this exercise. You can use this solution as guidance if you need additional help as you work through this exercise. Remember that for using it, you first need to build it by using Visual Studio and complete the placeholders of the LUIS Model and Azure Search Index name and key in Web.config.

## Prerequisites

The following software is required for completing this exercise:

* [Visual Studio 2017 Community](https://www.visualstudio.com/downloads/) or higher
* An [Azure](https://azureinfo.microsoft.com/us-freetrial.html?cr_cc=200744395&wt.mc_id=usdx_evan_events_reg_dev_0_iottour_0_0) subscription
* An account in the [LUIS Portal](https://www.luis.ai)
* The [Bot Framework Emulator](https://emulator.botframework.com/)

## Task 1: [Enable BackChannel]

1. [steps to get the secret backchannel key - it should match Node version]

1. Open the app you've obtained from the previous exercise. Alternatively, you can use the app from the [exercise7-HandOffToHuman](./exercise7-HandOffToHuman) folder.
    > **NOTE:** If you use the solution provided remember to replace:
    > * the **`LuisModel`** attribute in `RootDialog.cs` with your modelID and SubscriptionKey
    > * the **{TextAnalyticsApiKey}** in `Web.config` with your Text Analytics Key (as explained in exercise 6)
    > * the **{AzureSearchAccount}**, **{AzureSearchIndex}** and **{AzureSearchKey}** in `Web.config` with your search account, index name and key (as explained in exercise 4)

1. Replace the `default.htm` with [this template](../assets/csharp-backchannel/default.htm).

1. Bellow the [`botchat.js` script element](../assets/csharp-backchannel/default.htm#L52) add a new script element with the following code boilerplate.

    ```html
    <script>
        var botConnection = new BotChat.DirectLine({
            secret: 'DIRECTLINE_SECRET'
        });
        var resPanel = document.getElementById('results');

        BotChat.App({
            botConnection: botConnection,
            user: { id: 'WebChatUser' },
            bot: { id: 'BOT_ID' },
            locale: 'en-us',
        }, document.getElementById("bot"));

    </script>
    ```
    > **NOTE:** Replace the placeholders with yours keys:
    > * the **`DIRECTLINE_SECRET`** with your secret key from web-chat
    > * the **`BOT_ID`** with the ID from your bot

1. In the same script add the code to catch the `searchResults` incoming events.

    ```javascript
    botConnection.activity$
        .filter(function (activity) {
            return activity.type === 'event' && activity.name === 'searchResults';
        })
        .subscribe(function (activity) {
            updateSearchResults(activity.value)
        });

    function updateSearchResults(results) {
        console.log(results);
        resPanel.innerHTML = ''; // clear
        results.forEach(function (result) {
            resPanel.appendChild(createSearchResult(result));
        });
    }

    function createSearchResult(result) {
        var el = document.createElement('div');
        el.innerHTML = '<h3>' + result.Title + '</h3>' +
            '<p>' + result.Text.substring(0, 140) + '...</p>';

        return el;
    }
    ```

1. Now, in `RootDialog`, add a `SendSearchToBackchannel` method to create and send the `searchResults` events.

    ```CSharp
    private async Task SendSearchToBackchannel(IDialogContext context, IMessageActivity activity, string textSearch)
    {
        var searchService = new AzureSearchService();
        var searchResult = await searchService.Search(textSearch);
        if (searchResult != null && searchResult.Value.Length != 0)
        {
            var reply = ((Activity)activity).CreateReply();

            reply.Type = ActivityTypes.Event;
            reply.Name = "searchResults";
            reply.Value = searchResult.Value;
            await context.PostAsync(reply);
        }
    }
    ```

1. Update the `SubmitTicket` method in `RootDialog` to call the new `SendSearchToBackchannel` method when the bot receive the ticket's description.

    ```CSharp
    [LuisIntent("SubmitTicket")]
    public async Task SubmitTicket(IDialogContext context, IAwaitable<IMessageActivity> activityWaiter, LuisResult result)
    {
        ...
        await this.EnsureTicket(context);

        await this.SendSearchToBackchannel(context, activity, this.description);
    }
    ```

## Task 2: Test [Event to BackChannel - it should match Node version]

1. [use the web chat]

1. [type _my computer is not working_]

1. [see the results]

## Task 3: [Update to sent messages to user]

1. In `default.htm` replace the `#results h3` style with the following css

    ```css
    #results h3 {
        margin-top: 0;
        margin-bottom: 0;
        cursor: pointer;
    }
    ```

1. Inside the `createSearchResult` function add a new event click to send a `showDetailsOf` event to the bot when the user clicks on the Title of any article.

    ```javascript
    el.getElementsByTagName('h3')[0]
        .addEventListener('click', function () {
            botConnection
                .postActivity({
                    type: 'event',
                    value: this.textContent.trim(),
                    from: { id: 'user' },
                    name: 'showDetailsOf'
                })
                .subscribe(function (id) {
                    console.log('event sent', id);
                });
        });
    ```

1. In the `MessagesController` add the following code to handle the `showDetailsOf` events.

    ```CSharp
    public class MessagesController : ApiController
    {
        private readonly AzureSearchService searchService = new AzureSearchService();

    ...
    ```

    ```CSharp
    private async Task HandleEventMessage(Activity message)
    {
        if (string.Equals(message.Name, "showDetailsOf", StringComparison.InvariantCultureIgnoreCase))
        {
            var searchResult = await this.searchService.SearchByTitle(message.Value.ToString());
            string reply = "Sorry, I could not find that article.";

            if (searchResult != null && searchResult.Value.Length != 0)
            {
                reply = searchResult.Value[0].Text;
            }

            // return our reply to the user
            Activity replyActivity = message.CreateReply(reply);

            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync(replyActivity);
        }
    }
    ```

1. Update the `Post` of the controller to send the `event` activities to the corresponding method.

    ```CSharp
    public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
    {
        if (activity.Type == ActivityTypes.Message)
        {
            await Conversation.SendAsync(activity, () => new RootDialog());
        }
        else if (activity.Type == ActivityTypes.Event)
        {
            await this.HandleEventMessage(activity);
        }
        else
        {
            this.HandleSystemMessage(activity);
        }

        var response = Request.CreateResponse(HttpStatusCode.OK);
        return response;
    }
    ```

## Task 4: Test [BackChannel Event to conversation - it should match Node version]

1. [follow the same workflow from the last test]

1. [click in any title]

1. [see the magic]

## Further Challenges

[TBD]