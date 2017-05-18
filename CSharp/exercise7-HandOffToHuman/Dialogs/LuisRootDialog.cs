namespace Exercise7.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Autofac;
    using Exercise7.Model;
    using Exercise7.Services;
    using Exercise7.Util;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Builder.ConnectorEx;

#pragma warning disable 1998

    [LuisModel("38ffac05-8cc5-493f-b4f6-dda46be5554c", "cbb127d36fc0474c9f9222cf070c44cc")]    
    [Serializable]
    public class LuisRootDialog : LuisDialog<object>
    {
        private readonly AzureSearchService searchService = new AzureSearchService();

        private string category;
        private string severity;
        private string description;

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"I'm sorry, I did not understand '{result.Query}'.\nType 'help' to know more about me :)");
            context.Done<object>(null);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            SearchResult searchResult = await this.searchService.Search(result.Query);
            await context.PostAsync("I'm the help desk bot and I can help you create a ticket or explore the knowledge base.\n" +
                        "You can tell me things like _I need to reset my password_ or _explore hardware articles_.");
            context.Done<object>(null);
        }

        [LuisIntent("HandOffToHuman")]
        public async Task HandOff(IDialogContext context, LuisResult result)
        {
            var conversationReference = context.Activity.ToConversationReference();
            var provider = Conversation.Container.Resolve<HandOff.Provider>();

            if (provider.QueueMe(conversationReference))
            {
                var waitingPeople = provider.Pending() > 1 ? $", there are { provider.Pending() - 1 }" : "";

                await context.PostAsync($"Connecting you to the next available human agent...please wait{waitingPeople}.");
            }
            
            context.Done<object>(null);
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

        [LuisIntent("ExploreCategory")]
        public async Task ExploreCategory(IDialogContext context, LuisResult result)
        {
            EntityRecommendation categoryEntityRecommendation;
            result.TryFindEntity("category", out categoryEntityRecommendation);
            var category = ((Newtonsoft.Json.Linq.JArray)categoryEntityRecommendation?.Resolution["values"])?[0]?.ToString();

            context.Call(new CategoryExplorerDialog(category, result.Query), this.ResumeAndEndDialogAsync);
        }

        private async Task ResumeAndEndDialogAsync(IDialogContext context, IAwaitable<object> argument)
        {
            context.Done<object>(null);
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

        private async Task IssueConfirmedMessageReceivedAsync(IDialogContext context, IAwaitable<bool> argument)
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

                context.Call(new UserFeedbackRequestDialog(), this.ResumeAndEndDialogAsync);
            }
            else
            {
                await context.PostAsync("Ok. The ticket was not created. You can start again if you want.");
                context.Done<object>(null);
            }            
        }       
    }
}