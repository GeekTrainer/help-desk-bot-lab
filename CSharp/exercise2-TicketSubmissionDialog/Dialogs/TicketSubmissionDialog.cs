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
            await context.PostAsync("Hi! I’m the help desk bot and I can help you create a ticket.");
            PromptDialog.Text(context, this.DescriptionMessageReceivedAsync, "First, please briefly describe your problem to me.");
        }

        public async Task DescriptionMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.description = await argument;
            var severities = new string[] { "high", "normal", "low" };
            PromptDialog.Choice(context, this.SeverityMessageReceivedAsync, severities, "Which is the severity of this problem?", null, 3, PromptStyle.AutoText);
        }

        public async Task SeverityMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.severity = await argument;
            PromptDialog.Text(context, this.CategoryMessageReceivedAsync, "Which would be the category for this ticket(software, hardware, network, and so on) ?");
        }

        public async Task CategoryMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.category = await argument;
            var text = $"Great!I'm going to create a **{this.severity}** severity ticket in the **{this.category}** category. " +
                       $"The description I will use is _\"{this.description}\"_.Can you please confirm that this information is correct?";

            PromptDialog.Confirm(context, this.IssueConfirmedMessageReceivedAsync, text, null, 3, PromptStyle.AutoText);
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
                    await context.PostAsync($"Awesome! Your ticked has been created with the number {ticketId}.");
                }
                else
                {
                    await context.PostAsync("Ooops! Something went wrong while I was saving your ticket. Please try again later.");
                }
            }
            else
            {
                await context.PostAsync("Ok. The ticket was not created. You can start again if you want.");                
            }
            context.Done<object>(null);
        }
    }
}