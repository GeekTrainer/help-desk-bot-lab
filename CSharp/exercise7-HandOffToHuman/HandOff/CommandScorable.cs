namespace Exercise7.HandOff
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Builder.Scorables.Internals;
    using Microsoft.Bot.Connector;

    public enum ConversationState
    {
        ConnectedToBot,
        WaitingForAgent,
        ConnectedToAgent
    }

    public class Conversation
    {
        public DateTime Timestamp { get; set; }

        public ConversationReference User { get; set; }

        public ConversationReference Agent { get; set; }

        public ConversationState State { get; set; }
    }

    public class CommandScorable : ScorableBase<IActivity, string, double>
    {
        private readonly ConversationReference conversationReference;
        private readonly UserRoleResolver userRoleResolver;
        private readonly Provider provider;
        private readonly IBotData botData;

        public CommandScorable(IBotData botData, ConversationReference conversationReference, UserRoleResolver userRoleResolver, Provider provider)
        {
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.conversationReference, nameof(conversationReference), conversationReference);
            SetField.NotNull(out this.userRoleResolver, nameof(userRoleResolver), userRoleResolver);
            SetField.NotNull(out this.provider, nameof(provider), provider);
        }

        protected override async Task<string> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity as IMessageActivity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                // determine if the message comes form an agent or user
                if (await this.userRoleResolver.IsAgent(this.botData))
                {
                    return await this.PrepareAgentCommand(message);
                }                
            }

            return null;
        }

        protected async Task<string> PrepareAgentCommand(IMessageActivity message)
        {
            var conversation = this.provider.FindByAgentId(message.Conversation.Id);

            if (Regex.IsMatch(message.Text, @"^(?i)agent help"))
            {
                return this.GetAgentCommandOptions();
            }

            if (conversation == null)
            {
                if (Regex.IsMatch(message.Text, @"^(?i)connect"))
                {
                    var targetConversation = this.provider.PeekConversation(this.conversationReference);

                    if (targetConversation != null)
                    {
                        // notifty the user
                        var hello = "You are now talking to a human agent.";
                        ConnectorClient connector = new ConnectorClient(new Uri(targetConversation.User.ServiceUrl));
                        IMessageActivity newMessage = Activity.CreateMessageActivity();
                        newMessage.Type = ActivityTypes.Message;
                        newMessage.From = targetConversation.User.Bot;
                        newMessage.Conversation = targetConversation.User.Conversation;
                        newMessage.Recipient = targetConversation.User.User;
                        newMessage.Text = hello;
                        await connector.Conversations.SendToConversationAsync((Activity)newMessage);
                        
                        // notify the agent
                        return "You are now connected to the next user that requested human help.\nType *resume* to connect the user back to the bot.";
                    }
                    else
                    {
                        return "No users waiting in queue.";
                    }
                }
            }
            else
            {
                if (Regex.IsMatch(message.Text, @"^(?i)resume"))
                {
                    // disconnect the user from the agent
                    var targetConversation = this.provider.FindByAgentId(message.Conversation.Id);
                    targetConversation.State = ConversationState.ConnectedToBot;
                    targetConversation.Agent = null;

                    // notifty the user
                    var goodbye = "You are now talking to the bot again.";
                    ConnectorClient connector = new ConnectorClient(new Uri(targetConversation.User.ServiceUrl));
                    IMessageActivity newMessage = Activity.CreateMessageActivity();
                    newMessage.Type = ActivityTypes.Message;
                    newMessage.From = targetConversation.User.Bot;
                    newMessage.Conversation = targetConversation.User.Conversation;
                    newMessage.Recipient = targetConversation.User.User;
                    newMessage.Text = goodbye;
                    await connector.Conversations.SendToConversationAsync((Activity)newMessage);

                    // notify the agent
                    return $"Disconnected. There are {this.provider.Pending()} people waiting.";
                }
            }               

            return null;
        }
        
        protected override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var message = item as IMessageActivity;

            if (message != null)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                Activity reply = ((Activity)message).CreateReply(state);

                await connector.Conversations.ReplyToActivityAsync(reply);
            }
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected string GetAgentCommandOptions()
        {
            return "### Human Agent Help, please type:\n" +
                    " - *connect* to connect to the user who has been waiting the longest.\n" +
                    " - *agent help* at any time to see these options again.\n";
        }
    }
}
