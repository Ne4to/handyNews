using Newtonsoft.Json;

namespace handyNews.API.Models
{
    public class TagsResponse
    {
        [JsonProperty(PropertyName = "tags")]
        public TagResponse[] Tags { get; set; }
    }

    public class TagResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "sortid")]
        public string SortId { get; set; }
    }
}