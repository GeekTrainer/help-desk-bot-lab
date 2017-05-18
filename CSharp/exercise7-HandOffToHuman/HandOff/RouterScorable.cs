namespace Exercise7.HandOff
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Builder.Scorables.Internals;
    using Microsoft.Bot.Connector;

#pragma warning disable 1998

    public class RoutableActivity
    {
        public ConversationReference Destination { get; set; }

        public string Text { get; set; }
    }

    public class RouterScorable : ScorableBase<IActivity, RoutableActivity, double>
    {
        private readonly ConversationReference conversationReference;
        private readonly UserRoleResolver userRoleResolver;
        private readonly Provider provider;
        private readonly IBotData botData;

        public RouterScorable(IBotData botData, ConversationReference conversationReference, UserRoleResolver userRoleResolver, Provider provider)
        {
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.conversationReference, nameof(conversationReference), conversationReference);
            SetField.NotNull(out this.userRoleResolver, nameof(userRoleResolver), userRoleResolver);
            SetField.NotNull(out this.provider, nameof(provider), provider);
        }

        protected override async Task<RoutableActivity> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity as Activity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                // determine if the message comes form an agent or user
                if (await this.userRoleResolver.IsAgent(this.botData))
                {
                    return this.PrepareRouteableAgentActivity(message);
                }
                else
                {
                    return this.PrepareRouteableCustomerActivity(message);
                }
            }

            return null;
        }

        protected RoutableActivity PrepareRouteableAgentActivity(Activity activity)
        {
            var conversation = this.provider.FindByAgentId(activity.Conversation.Id);

            if (conversation == null)
            {
                return null;
            }

            return new RoutableActivity { Destination = conversation.User, Text = activity.Text };
        }

        protected RoutableActivity PrepareRouteableCustomerActivity(Activity activity)
        {
            var conversation = this.provider.FindByConversationId(activity.Conversation.Id);
            if (conversation == null)
            {
                conversation = this.provider.CreateConversation(this.conversationReference);
            }

            switch (conversation.State)
            {
                case ConversationState.ConnectedToBot:
                    return null; // continue normal flow
                case ConversationState.WaitingForAgent:
                    return new RoutableActivity { Text = $"Connecting you to the next available human agent...please wait, there are {this.provider.Pending() - 1} users waiting." };
                case ConversationState.ConnectedToAgent:
                    return new RoutableActivity { Destination = conversation.Agent, Text = activity.Text };
            }

            return null;
        }

        protected override bool HasScore(IActivity item, RoutableActivity state)
        {
            return state != null;
        }

        protected override double GetScore(IActivity item, RoutableActivity state)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, RoutableActivity state, CancellationToken token)
        {
            var message = item as IMessageActivity;

            if (message != null && state.Destination == null)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                Activity reply = ((Activity)message).CreateReply(state.Text);

                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else if (state.Destination != null)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(state.Destination.ServiceUrl));
                IMessageActivity newMessage = Activity.CreateMessageActivity();
                newMessage.Type = ActivityTypes.Message;
                newMessage.From = state.Destination.Bot;
                newMessage.Conversation = state.Destination.Conversation;
                newMessage.Recipient = state.Destination.User;
                newMessage.Text = state.Text;
                await connector.Conversations.SendToConversationAsync((Activity)newMessage);
            }
        }

        protected override Task DoneAsync(IActivity item, RoutableActivity state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
