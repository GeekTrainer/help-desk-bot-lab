namespace Exercise7.HandOff
{
    using Microsoft.Bot.Connector;

    public interface IUserRoleResolver
    {
        bool IsAgent(IBotState dialog);
    }
}
