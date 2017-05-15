namespace Step3.Dialogs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Step3.Util;

    [LuisModel("c7637a36-6a94-4c15-9943-c25463eb3db6", "cbb127d36fc0474c9f9222cf070c44cc")]
    [Serializable]
    public class TicketSubmissionDialog : LuisDialog<object>
    {        
        private string category;
        private string severity;
        private string description;

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm the help desk bot and I can help you create a ticket.\n" +
                                    "You can tell me things like _I need to reset my password_ or _I cannot print_.");
            await context.PostAsync("First, please briefly describe your problem to me.");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("SubmitTicket")]
        public async Task SubmitTicket(IDialogContext context, LuisResult result)
        {
            EntityRecommendation categoryEntityRecommendation, severityEntityRecommendation;

            result.TryFindEntity("category", out categoryEntityRecommendation);
            result.TryFindEntity("severity", out severityEntityRecommendation);

            this.category = ((Newtonsoft.Json.Linq.JArray)categoryEntityRecommendation?.Resolution["values"])?[0]?.ToString();
            this.severity = ((Newtonsoft.Json.Linq.JArray)severityEntityRecommendation?.Resolution["values"])?[0]?.ToString();
            this.description = result.Query;

            await this.EnsureTicket(context);
        }

        private async Task EnsureTicket(IDialogContext context)
        {
            if (this.severity == null)
            {
                var severities = new string[] { "high", "normal", "low" };
                PromptDialog.Choice(context, this.SeverityMessageReceivedAsync, severities, "Which is the severity of this problem?", null, 3, PromptStyle.AutoText);
            }
            else if (this.category == null)
            {
                PromptDialog.Text(context, this.CategoryMessageReceivedAsync, "Which would be the category for this ticket(software, hardware, network, and so on) ?");
            }
            else
            {
                var text = $"Great!I'm going to create a **{this.severity}** severity ticket in the **{this.category}** category. " +
                       $"The description I will use is _\"{this.description}\"_.Can you please confirm that this information is correct?";

                PromptDialog.Confirm(context, this.IssueConfirmedMessageReceivedAsync, text, null, 3, PromptStyle.AutoText);
            }
        }

        private async Task SeverityMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.severity = await argument;        
            await this.EnsureTicket(context);
        }

        private async Task CategoryMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.category = await argument;
            await this.EnsureTicket(context);
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