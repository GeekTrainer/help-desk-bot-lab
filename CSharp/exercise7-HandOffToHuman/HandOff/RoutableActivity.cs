namespace Exercise7.HandOff
{
    using Microsoft.Bot.Connector;

    public class RoutableActivity
    {
        public ConversationReference Destination { get; set; }

        public string Text { get; set; }
    }
}
