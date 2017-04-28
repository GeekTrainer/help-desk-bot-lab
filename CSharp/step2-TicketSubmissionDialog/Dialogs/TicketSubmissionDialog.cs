namespace Step1.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Step1.API;

    [Serializable]
    public class TicketSubmissionDialog : IDialog
    {
        private string category;
        private string severity;
        private string description;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }
        
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            await context.PostAsync("Welcome, let's complete the ticket details.");
            PromptDialog.Text(context, this.CategoryMessageReceivedAsync, "Type the category");
        }

        public async Task CategoryMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.category = await argument;
            await context.PostAsync("Ok, the category is: " + this.category);

            var severities = new string[] { "High", "Normal", "Low" };
            PromptDialog.Choice(context, this.SeverityMessageReceivedAsync, severities, "Choose the severity");
        }

        public async Task SeverityMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.severity = await argument;
            await context.PostAsync("Ok, the category is: " + this.category + " and the severity is: " + this.severity);
            PromptDialog.Text(context, this.DescriptionMessageReceivedAsync, "Type a description");
        }

        public async Task DescriptionMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.description = await argument;

            var api = new ClientAPI();
            var success = await api.PostIssueAsync(this.category, this.severity, this.description);

            if (success)
            {
                await context.PostAsync("Got it. Your ticked has been recorded. Category: " + this.category + ", Severity: " + this.severity + " Description: " + this.description);
            }
            else
            {
                await context.PostAsync("Something went wrong while we was recording your issue. Please try again later.");
            }
            
            context.Done<object>(null);
        }
    }
}