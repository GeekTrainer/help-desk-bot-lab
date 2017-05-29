namespace Exercise7.HandOff
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using System.Threading;

    public static class AgentExtensions
    {
        private const string IS_AGENT = "isAgent";
        public static bool IsAgent(this IBotData botData)
        {
            bool isAgent = false;
            botData.ConversationData.TryGetValue(IS_AGENT, out isAgent);
            return isAgent;
        }

        public static Task SetAgent(this IBotData botData, bool value, CancellationToken token)
        {
            botData.ConversationData.SetValue(IS_AGENT, value);
            return botData.FlushAsync(token);
        }
    }
}