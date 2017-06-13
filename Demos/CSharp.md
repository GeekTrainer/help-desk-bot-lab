## Exercise 2: Ticket Submission Dialog

### Text prompts

In the `StartAsync` method of this dialog, we ask the user to describe his problem and send the response to the `MessageReceivedAsync` method in which we print it.

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

In the `StartAsync` method of this dialog, we ask the user to choose from a closed list the severity and send the response to the `MessageReceivedAsync` method in which we print it.

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

### Create an Adaptive Card

## Exercise 7: Handoff to Human

### Middleware