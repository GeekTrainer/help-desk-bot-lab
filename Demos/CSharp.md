# Sample Demo Code (C#)

## Exercise 2: Ticket Submission Dialog

### Text prompts

In the `StartAsync` method of this dialog, we ask the user to describe his problem and it sends the response to the `MessageReceivedAsync` method in which we print it.

``` csharp
[Serializable]
public class RootDialog : IDialog<object>
{
    public Task StartAsync(IDialogContext context)
    {
        PromptDialog.Text(context, this.MessageReceivedAsync, "First, please briefly describe your problem to me.");

        return Task.CompletedTask;
    }

    public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
    {
        var description = await argument;
        Console.WriteLine(description);
    }
}
```

### Choice, confirm, buttons in cards

In the `StartAsync` method of this dialog, we ask the user to choose from a closed list the severity and it sends the response to the `MessageReceivedAsync` method in which we print it.

``` csharp
[Serializable]
public class RootDialog : IDialog<object>
{
    public async Task StartAsync(IDialogContext context)
    {
        var severities = new string[] { "high", "normal", "low" };
        PromptDialog.Choice(context, this.MessageReceivedAsync, severities, "Which is the severity of this problem?");
    }

    public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
    {
        var severity = await argument;
        Console.WriteLine(severity);
    }
}
```

This code prompts the user for confirmation, expecting a yes/no answer.

``` csharp
[Serializable]
public class RootDialog : IDialog<object>
{
    public async Task StartAsync(IDialogContext context)
    {
        PromptDialog.Confirm(context, this.MessageReceivedAsync, "Are you sure this is correct?");
    }

    public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirmed = await argument;
        Console.WriteLine(confirmed);
    }
}
```

---
WIP from here

## Exercise 3: Luis Dialog

### Scorables

In this sample, we will handle the `help` global message and display a message to the user.

First, create a class implementing the **ScorableBase** base class.

``` csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;

public class HelpScorable : ScorableBase<IActivity, string, double>
{
    protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    protected override double GetScore(IActivity item, string state)
    {
        return 1.0;
    }

    protected override bool HasScore(IActivity item, string state)
    {
        return !string.IsNullOrWhiteSpace(state);
    }

    protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
    {
        ConnectorClient connector = new ConnectorClient(new Uri(item.ServiceUrl));

        var replyActivity = ((Activity)item).CreateReply("I'm the help desk bot and I can help you create a ticket.\nYou can tell me things like _I need to reset my password_ or _I cannot print_.");
        await connector.Conversations.ReplyToActivityAsync(replyActivity);
    }

    protected override async Task<string> PrepareAsync(IActivity activity, CancellationToken token)
    {
        if (activity is IMessageActivity message && !string.IsNullOrWhiteSpace(message.Text))
        {
            if (message.Text.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                return message.Text;
            }
        }
        return null;
    }
}
```

Next, register the new class in **Autofac** container in the `Global.asax` file.

``` csharp
using System.Web.Http;
using Autofac;
using Dialogs;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;

public class WebApiApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        GlobalConfiguration.Configure(WebApiConfig.Register);

        var builder = new ContainerBuilder();

        builder
            .Register(c => new HelpScorable())
            .As<IScorable<IActivity, double>>()
            .InstancePerLifetimeScope();

        builder.Update(Microsoft.Bot.Builder.Dialogs.Conversation.Container);
    }
}
```

More info [here](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-global-handlers) about scorables.

### Create an Adaptive Card

To be able to send AdaptiveCard, you must use the `Microsoft.AdaptiveCards` NuGet package.

``` csharp
var message = context.MakeMessage();
message.Attachments = new List<Attachment>
{
    new Attachment
    {
        ContentType = "application/vnd.microsoft.card.adaptive",
        Content = this.CreateCard()
    }
};

await context.PostAsync(message);
```

``` csharp
using AdaptiveCards;
using System.Collections.Generic;

private AdaptiveCard CreateCard(int ticketId, string category, string severity, string description)
{
    var card = new AdaptiveCard();

    var headerBlock = new TextBlock()
    {
        Text = $"Ticket #1",
        Weight = TextWeight.Bolder,
        Size = TextSize.Large,
        Speak = $"<s>You've created a new Ticket #1</s><s>We will contact you soon.</s>"
    };

    var columnsBlock = new ColumnSet()
    {
        Separation = SeparationStyle.Strong,
        Columns = new List<Column>
        {
            new Column
            {
                Size = "1",
                Items = new List<AdaptiveCards.CardElement>
                {
                    new FactSet
                    {
                        Facts = new List<AdaptiveCards.Fact>
                        {
                            new AdaptiveCards.Fact("Severity:", "High"),
                            new AdaptiveCards.Fact("Category:", "Software"),
                        }
                    }
                }
            },
            new Column
            {
                Size = "auto",
                Items = new List<CardElement>
                {
                    new AdaptiveCards.Image
                    {
                        Url = "https://raw.githubusercontent.com/GeekTrainer/help-desk-bot-lab/develop/assets/botimages/head-smiling-medium.png",
                        Size = ImageSize.Small,
                        HorizontalAlignment = HorizontalAlignment.Right
                    }
                }
            }
        }
    };

    card.Body.Add(headerBlock);
    card.Body.Add(columnsBlock);

    return card;
}
```

## Exercise 7: Handoff to Human

### Middleware

To intercept messages and log them using a _middleware_, you can implement the `IActivityLogger` interface and in the `LogAsync` method do what you need. This method is called whenever the user or the bot send a message.

``` csharp
using System.Threading.Tasks;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;

public class DebugActivityLogger : IActivityLogger
{
    public async Task LogAsync(IActivity activity)
    {
        Debug.WriteLine($"From:{activity.From.Id} - To:{activity.Recipient.Id} - Message:{activity.AsMessageActivity()?.Text}");
    }
}
```

Next, register the new class in **Autofac** container in the `Global.asax` file.

``` csharp
using System.Web.Http;
using Autofac;

public class WebApiApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        GlobalConfiguration.Configure(WebApiConfig.Register);

        var builder = new ContainerBuilder();

        builder
            .RegisterType<DebugActivityLogger>()
            .AsImplementedInterfaces()
            .InstancePerDependency();

        builder.Update(Microsoft.Bot.Builder.Dialogs.Conversation.Container);
    }
}
```
