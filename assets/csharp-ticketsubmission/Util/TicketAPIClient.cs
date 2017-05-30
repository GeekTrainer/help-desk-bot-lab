namespace HelpDeskBot.Util
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using Exercise2.Model;

    public class TicketAPIClient
    {
        public async Task<int> PostTicketAsync(string category, string severity, string description)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(WebConfigurationManager.AppSettings["TicketsAPIBaseUrl"]);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var ticket = new Ticket
                    {
                        Category = category,
                        Severity = severity,
                        Description = description
                    };

                    var response = await client.PostAsJsonAsync("api/tickets", ticket);
                    return await response.Content.ReadAsAsync<int>();
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}