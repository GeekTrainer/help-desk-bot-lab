namespace Step4.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Model;
    using Services;    
    using Util;

    [LuisModel("e55f7b29-8a93-4342-91da-fde51679f526", "986b8131c87246bebc6e8cb2c167fc5b")]
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
            SearchResult searchResult = await this.searchService.Search(result.Query);
            CardUtil.ShowSearchResults(context, searchResult, $"No results were found for '{result.Query}'");

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

            context.Call(new CategoryExplorerDialog(category), this.ResumeAfterCategoryAsync);
        }

        private async Task ResumeAfterCategoryAsync(IDialogContext context, IAwaitable<object> argument)
        {
            context.Done<object>(null);
        }

        private async Task EnsureTicket(IDialogContext context)
        {
            if (this.category == null)
            {
                PromptDialog.Text(context, this.CategoryMessageReceivedAsync, "Type the ticket category");
            }
            else if (this.severity == null)
            {
                var severities = new string[] { "high", "normal", "low" };
                PromptDialog.Choice(context, this.SeverityMessageReceivedAsync, severities, "Choose the ticket severity");
            }
            else
            {
                var message = $"I'm going to create {this.severity} severity ticket under category {this.category}. The description i will use is: {this.description}. Do you want to continue adding this ticket?";
                PromptDialog.Confirm(context, this.IssueConfirmedMessageReceivedAsync, message);
            }
        }

        private async Task CategoryMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.category = await argument;
            await context.PostAsync("Ok, the category is: " + this.category);

            await this.EnsureTicket(context);
        }

        private async Task SeverityMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this.severity = await argument;
            await context.PostAsync("Ok, the category is: " + this.category + " and the severity is: " + this.severity);

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
