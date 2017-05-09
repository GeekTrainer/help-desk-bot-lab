namespace Exercise6.Util
{
    using System;
    using System.Collections.Generic;
    using Exercise6.Model;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;    

    public static class CardUtil
    {
        public static async void ShowSearchResults(IDialogContext context, SearchResult searchResult, string notResultsMessage)
        {
            if (searchResult.Value.Length != 0)
            {
                Activity reply = ((Activity)context.Activity).CreateReply();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                foreach (SearchResultHit item in searchResult.Value)
                {
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction button = new CardAction()
                    {
                        Value = $"show details of article {item.Title}",
                        Type = "postBack",
                        Title = "More details"
                    };
                    cardButtons.Add(button);

                    HeroCard card = new HeroCard()
                    {
                        Title = item.Title,
                        Subtitle = $"Category: {item.Category} | Search Score: {item.SearchScore}",
                        Text = item.Text.Substring(0, 50) + "...",
                        Buttons = cardButtons
                    };
                    reply.Attachments.Add(card.ToAttachment());
                }

                ConnectorClient connector = new ConnectorClient(new Uri(reply.ServiceUrl));
                await connector.Conversations.SendToConversationAsync(reply);
            }
            else
            {
                await context.PostAsync(notResultsMessage);
            }            
        }
    }
}