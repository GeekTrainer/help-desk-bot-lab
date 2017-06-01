namespace HelpDeskBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Services;

    [Serializable]
    public class UserFeedbackRequestDialog : IDialog<object>
    {
        private readonly TextAnalyticsService textAnalyticsService = new TextAnalyticsService();
        
        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Text(context, this.MessageReciveAsync, "How would you rate my help?");
        }

        public async Task MessageReciveAsync(IDialogContext context, IAwaitable<string> result)
        {
            var response = await result;

            double score = await this.textAnalyticsService.Sentiment(response);

            if (score == double.NaN)
            {
                await context.PostAsync("Ooops! Something went wrong while analyzing your answer. An IT representative agent will get in touch with you to follow up soon.");
            }
            else
            {
                if (score < 0.5)
                {
                    await context.PostAsync("I understand that you might be dissatisfied with my assistance. An IT representative agent will get in touch with you soon to help you.");
                }
                else
                {
                    await context.PostAsync("Thanks for sharing your experience.");
                }
            }
            
            context.Done<object>(null);
        }
    }
}