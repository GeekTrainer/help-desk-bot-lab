namespace Step4.Model
{
    using Newtonsoft.Json;

    public class Value
    {
        [JsonProperty("@search.score")]
        public float SearchScore { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public string Text { get; set; }
    }    
}