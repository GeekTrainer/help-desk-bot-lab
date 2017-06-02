namespace HelpDeskBot.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using Newtonsoft.Json;

    public class TextAnalyticsResult
    {
        public TextAnalyticsResultDocument[] Documents { get; set; }
    }

    public class TextAnalyticsResultDocument
    {
        public string Id { get; set; }

        public double Score { get; set; }
    }

    [Serializable]
    public class TextAnalyticsService
    {
        public async Task<double> Sentiment(string text)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://westus.api.cognitive.microsoft.com/");
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", WebConfigurationManager.AppSettings["TextAnalyticsApiKey"]);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                byte[] byteData = Encoding.UTF8.GetBytes("{ \"documents\": " +
                    "[{ \"language\": \"en\", \"id\": \"single\", \"text\":\"" + text + "\"}] }");

                string uri = "/text/analytics/v2.0/sentiment";

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await httpClient.PostAsync(uri, content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<TextAnalyticsResult>(responseString);

                    if (result.Documents.Length == 1)
                    {
                        return result.Documents[0].Score;
                    }

                    return double.NaN;
                }
            }
        }
    }
}