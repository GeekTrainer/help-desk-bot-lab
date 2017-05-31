namespace Exercise7.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Autofac;
    using Exercise7.Services;
    using Microsoft.Bot.Builder.ConnectorEx;
    using Microsoft.Bot.Builder.Dialogs;

    [Serializable]
    public class UserFeedbackRequestDialog : IDialog<object>
    {
        private readonly TextAnalyticsService textAnalyticsService = new TextAnalyticsService();
        
        public Task StartAsync(IDialogContext context)
        {
            PromptDialog.Text(context, this.MessageReciveAsync, "How would you rate my help?");

            return Task.CompletedTask;
        }

        public async Task MessageReciveAsync(IDialogContext context, IAwaitable<string> result)
        {
            var response = await result;

            double score = await this.textAnalyticsService.Sentiment(response);

            if (score == double.NaN)
            {
                await context.PostAsync("Ooops! Something went wrong while analying your answer. An IT representative agent will get in touch with you to follow up soon.");
            }
            else
            {
                if (score < 0.5)
                {
                    var text = "Do you want me to escalate this with an IT representative?";
                    PromptDialog.Confirm(context, this.EscalateWithHumanAgent, text, promptStyle: PromptStyle.AutoText);
                }
                else
                {
                    await context.PostAsync("Thanks for sharing your experience.");
                    context.Done<object>(null);
                }
            }
        }

        private async Task EscalateWithHumanAgent(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirmed = await argument;

            if (confirmed)
            {
                var conversationReference = context.Activity.ToConversationReference();
                var provider = Conversation.Container.Resolve<HandOff.Provider>();

                if (provider.QueueMe(conversationReference))
                {
                    var waitingPeople = provider.Pending() > 1 ? $", there are { provider.Pending() - 1 }" : string.Empty;

                    await context.PostAsync($"Connecting you to the next available human agent...please wait{waitingPeople}.");
                }
            }

            context.Done<object>(null);
        }
    }
}