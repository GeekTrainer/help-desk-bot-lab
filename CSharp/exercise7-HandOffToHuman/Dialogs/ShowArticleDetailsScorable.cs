namespace Exercise7.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Exercise7.Services;
    using Microsoft.Bot.Builder.Scorables.Internals;
    using Microsoft.Bot.Connector;

    public class ShowArticleDetailsScorable : ScorableBase<IActivity, string, double>
    {
        private const string TRIGGER = "show details of article ";
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
            ConnectorClient connector = new ConnectorClient(new Uri(item.ServiceUrl));
            var reply = "Sorry, I could not find that article.";

            var searchResult = await this.searchService.SearchByTitle(state.ToString());
            if (searchResult != null && searchResult.Value.Length != 0)
            {
                reply = searchResult.Value[0].Text;
            }

            var replyActiviy = ((Activity)item).CreateReply(reply);
            await connector.Conversations.ReplyToActivityAsync(replyActiviy);
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