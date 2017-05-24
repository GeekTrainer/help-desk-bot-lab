namespace Exercise3.Util
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using Exercise3.Model;

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

                    var issue = new Ticket
                    {
                        Category = category,
                        Severity = severity,
                        Description = description
                    };

                    var response = await client.PostAsJsonAsync("api/tickets", issue);
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