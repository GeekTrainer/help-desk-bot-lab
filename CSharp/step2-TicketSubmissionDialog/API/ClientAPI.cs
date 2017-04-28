namespace Step1.API
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class ClientAPI
    {
        public async Task<bool> PostIssueAsync(string category, string severity, string description)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:3979/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var issue = new IssueDTO
                {
                    Category = category,
                    Severity = severity,
                    Description = description
                };

                await client.PostAsJsonAsync("api/issues", issue);
            }

            return true;
        }
    }
}