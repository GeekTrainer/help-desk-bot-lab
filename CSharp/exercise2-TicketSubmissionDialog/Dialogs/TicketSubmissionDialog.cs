namespace Step2.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Step2.Util;    

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
            await context.PostAsync("Welcome, let's complete ticket details:");
            PromptDialog.Text(context, this.CategoryMessageReceivedAsync, "Type the ticket category");
        }

        public async Task CategoryMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.category = await argument;
            await context.PostAsync("Ok, the category is: " + this.category);

            var severities = new string[] { "high", "normal", "low" };
            PromptDialog.Choice(context, this.SeverityMessageReceivedAsync, severities, "Choose the ticket severity");
        }

        public async Task SeverityMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.severity = await argument;
            await context.PostAsync("Ok, the category is: " + this.category + " and the severity is: " + this.severity);
            PromptDialog.Text(context, this.DescriptionMessageReceivedAsync, "Please add additional details");
        }

        public async Task DescriptionMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.description = await argument;
            var message = $"I'm going to create {this.severity} severity ticket under category {this.category}. The description i will use is: {this.description}. Do you want to continue adding this ticket?";
            PromptDialog.Confirm(context, this.IssueConfirmedMessageReceivedAsync, message);
        }

        public async Task IssueConfirmedMessageReceivedAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirmed = await argument;

            if (confirmed)
            {
                var api = new TicketAPIClient();
                var ticketId = await api.PostTicketAsync(this.category, this.severity, this.description);

                if (ticketId != -1)
                {
                    await context.PostAsync("## Your ticked has been recorded:\n\n - Ticket ID: " + ticketId + "\n\n - Category: " + this.category + "\n\n - Severity: " + this.severity + "\n\n - Description: " + this.description);
                }
                else
                {
                    await context.PostAsync("Something went wrong while we was recording your ticket. Please try again later.");
                }

                context.Done<object>(null);
            }
            else
            {
                await context.PostAsync("Ok, action cancelled!");
                context.Done<object>(null);
            }
        }
    }
}