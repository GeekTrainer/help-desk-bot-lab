namespace Exercise7.HandOff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Bot.Connector;
    
    public class Provider
    {
        public IList<Conversation> Conversations { get; private set; }

        public Provider()
        {
            Conversations = new List<Conversation>();
        }

        public int Pending()
        {
            return Conversations.Count(c => c.State == ConversationState.WaitingForAgent);
        }

        public Conversation CreateConversation(ConversationReference conversationReference)
        {
            var newConversation = new Conversation {
                User = conversationReference,
                State = ConversationState.ConnectedToBot,
                Timestamp = DateTime.Now
            };
            this.Conversations.Add(newConversation);

            return newConversation;
        }

        public Conversation FindByConversationId(string userConversationId)
        {
            return this.Conversations.Where(c => c.User.Conversation.Id.Equals(userConversationId)).FirstOrDefault();
        }

        public Conversation FindByAgentId(string agentConversationId)
        {
            return this.Conversations.Where(c => c.Agent.Conversation.Id.Equals(agentConversationId)).FirstOrDefault();
        }

        public Conversation PeekConversation(ConversationReference agentReference)
        {
            var conversation = this.Conversations
                                    .Where(c => c.State == ConversationState.WaitingForAgent)
                                    .OrderByDescending(c => c.Timestamp).FirstOrDefault();

            if (conversation != null)
            {
                conversation.State = ConversationState.ConnectedToAgent;
                conversation.Agent = agentReference;
            }

            return conversation;
        }
    }
}