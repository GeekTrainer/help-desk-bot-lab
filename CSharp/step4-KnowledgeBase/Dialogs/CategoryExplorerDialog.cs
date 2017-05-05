namespace Step4.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Step4.Model;
    using Step4.Services;    
    using Util;

    [Serializable]
    public class CategoryExplorerDialog : IDialog<object>
    {
        private readonly AzureSearchService searchService = new AzureSearchService();
        private string category = null;

        public CategoryExplorerDialog(string category)
        {
            this.category = category;
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (string.IsNullOrWhiteSpace(this.category))
            {
                FacetResult facetResult = await this.searchService.FetchFacets();
                if (facetResult.Facets.Category.Length != 0)
                {
                    List<string> categories = new List<string>();
                    foreach (Category category in facetResult.Facets.Category)
                    {
                        categories.Add($"{category.Value} ({category.Count})");
                    }

                    PromptDialog.Choice(context, this.AfterMenuSelection, categories, "Which category are you interested in?");
                }
            }
            else
            {
                SearchResult searchResult = await this.searchService.SearchByCategory(this.category);
                CardUtil.ShowSearchResults(context, searchResult, $"No results were found for category: '{this.category}'");

                context.Done<object>(null);
            }
        }

        public virtual async Task AfterMenuSelection(IDialogContext context, IAwaitable<string> result)
        {
            this.category = await result;
            this.category = Regex.Replace(this.category, @"\s\([^)]*\)", string.Empty);

            SearchResult searchResult = await this.searchService.SearchByCategory(this.category);
            CardUtil.ShowSearchResults(context, searchResult, $"No results were found for category: '{this.category}'");

            context.Done<object>(null);
        }
    }
}