namespace Exercise7.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Exercise7.Services;
    using Exercise7.Util;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;

    #pragma warning disable 1998

    public class ElevateMeScorable : IScorable<IActivity, double>
    {
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
                var text = state.ToString();
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                               
                // TODO     
            }
        }

        public async Task<object> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item as IMessageActivity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                if (message.Text.StartsWith("/elevate me", StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text;
                }
            }

            return null;
        }
    }
}