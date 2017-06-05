namespace HelpDeskBot.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using Model;
    using Newtonsoft.Json;

    [Serializable]
    public class AzureSearchService
    {
        private readonly string queryString = $"https://{WebConfigurationManager.AppSettings["AzureSearchAccount"]}.search.windows.net/indexes/{WebConfigurationManager.AppSettings["AzureSearchIndex"]}/docs?api-key={WebConfigurationManager.AppSettings["AzureSearchKey"]}&api-version=2015-02-28&";

        public async Task<SearchResult> Search(string text)
        {
            using (var httpClient = new HttpClient())
            {
                string nameQuey = $"{queryString}search={text}";
                string response = await httpClient.GetStringAsync(nameQuey);
                return JsonConvert.DeserializeObject<SearchResult>(response);
            }
        }

        public async Task<SearchResult> SearchByTitle(string title)
        {
            using (var httpClient = new HttpClient())
            {
                string nameQuey = $"{queryString}$filter=title eq '{title}'";
                string response = await httpClient.GetStringAsync(nameQuey);
                return JsonConvert.DeserializeObject<SearchResult>(response);
            }
        }

        public async Task<FacetResult> FetchFacets()
        {
            using (var httpClient = new HttpClient())
            {
                string facetQuey = $"{queryString}facet=category";
                string response = await httpClient.GetStringAsync(facetQuey);
                return JsonConvert.DeserializeObject<FacetResult>(response);
            }
        }

        public async Task<SearchResult> SearchByCategory(string category)
        {
            using (var httpClient = new HttpClient())
            {
                string nameQuey = $"{queryString}$filter=category eq '{category}'";
                string response = await httpClient.GetStringAsync(nameQuey);
                return JsonConvert.DeserializeObject<SearchResult>(response);
            }
        }
    }
}