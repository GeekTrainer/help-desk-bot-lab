namespace Exercise7.HandOff
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs.Internals;

    public class UserRoleResolver
    {
        public async Task<bool> IsAgent(IBotData botData)
        {
            bool isAgent = false;
            var result = botData.ConversationData.TryGetValue("IsAgent", out isAgent);
            return isAgent;
        }
    }
}