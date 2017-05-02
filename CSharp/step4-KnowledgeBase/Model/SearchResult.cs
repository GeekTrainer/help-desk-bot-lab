namespace Step4.Model
{
    using Newtonsoft.Json;

    public class SearchResult
    {
        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }

        public Value[] Value { get; set; }
    }    
}