namespace Exercise7.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Exercise7.HandOff;
    using Exercise7.Services;
    using Exercise7.Util;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Builder.Dialogs.Internals;

#pragma warning disable 1998

    public class AgentLoginScorable : IScorable<IActivity, double>
    {
        private readonly AzureSearchService searchService = new AzureSearchService();
        private readonly Provider provider;
        private readonly IBotData botData;

        public AgentLoginScorable(IBotData botData, Provider provider)
        {
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.provider, nameof(provider), provider);
        }

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

        public async Task PostAsync(IActivity activity, object state, CancellationToken token)
        {
            var message = activity as IMessageActivity;

            if (state != null && message != null)
            {
                this.botData.ConversationData.SetValue("IsAgent", true);
                await this.botData.FlushAsync(token);

                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                var welcome = $"Welcome back human agent, there are {this.provider.Pending()} waiting users in the queue.\n\nType _agent help_ for more details.";
                Activity reply = ((Activity)message).CreateReply(welcome);

                await connector.Conversations.ReplyToActivityAsync(reply, token);
            }
        }

        public async Task<object> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item as IMessageActivity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                if (message.Text.StartsWith("/agent login", StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text;
                }
            }

            return null;
        }
    }
}