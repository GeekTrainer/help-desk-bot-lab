namespace Exercise7.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Exercise7.Services;
    using Exercise7.Util;
    using Microsoft.Bot.Builder.Scorables.Internals;
    using Microsoft.Bot.Connector;

    public class SearchScorable : ScorableBase<IActivity, string, double>
    {
        private const string TRIGGER = "search about ";
        private readonly AzureSearchService searchService = new AzureSearchService();

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

        protected async override Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var searchResult = await this.searchService.Search(state);

            var replyActiviy = ((Activity)item).CreateReply();
            await CardUtil.ShowSearchResults(replyActiviy, searchResult, $"I'm sorry, I did not understand '{state}'.\nType 'help' to know more about me :)");
        }

        protected async override Task<string> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item.AsMessageActivity();
            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                if (message.Text.Trim().StartsWith(TRIGGER, StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text.Substring(TRIGGER.Length);
                }
            }

            return null;
        }
    }
}