namespace Exercise4.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Exercise4.Services;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;

    public class ShowArticleDetailsScorable : IScorable<IActivity, double>
    {
        private const string TRIGGER = "show details of article ";
        private readonly AzureSearchService searchService = new AzureSearchService();

        public Task DoneAsync(IActivity item, object state, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public double GetScore(IActivity item, object state)
        {
            bool matched = state != null;
            var score = matched ? 1.0 : double.NaN;
            return score;
        }

        public bool HasScore(IActivity item, object state)
        {
            return state != null;
        }

        public async Task PostAsync(IActivity item, object state, CancellationToken token)
        {
            var message = item as IMessageActivity;

            if (state != null && message != null)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                var reply = "Sorry, I could not find that article.";

                var searchResult = await this.searchService.SearchByTitle(state.ToString());
                if (searchResult != null && searchResult.Value.Length != 0)
                {   
                    reply = searchResult.Value[0].Text;                    
                }

                Activity replyActivity = ((Activity)message).CreateReply(reply);
                await connector.Conversations.ReplyToActivityAsync(replyActivity);
            }
        }

        public async Task<object> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item as IMessageActivity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                if (message.Text.StartsWith(TRIGGER, StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text.Substring(TRIGGER.Length);
                }
            }

            return null;
        }
    }
}