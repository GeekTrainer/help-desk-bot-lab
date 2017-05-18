namespace Exercise7.Util
{
    using Exercise7.HandOff;
    using Microsoft.Bot.Connector;
    
    public class SampleUserRole : IUserRoleResolver
    {
        public bool IsAgent(IBotState state)
        {
            // TODO
            return false;
        }
    }
}