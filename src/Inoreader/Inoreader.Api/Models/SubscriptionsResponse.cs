using Newtonsoft.Json;

namespace Inoreader.Api.Models
{
	public class SubscriptionsResponse
	{
		[JsonProperty(PropertyName = "subscriptions")]
		public Subscription[] Subscriptions { get; set; }
	}
	
	public class Subscription
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "categories")]
		public Category[] Categories { get; set; }

		[JsonProperty(PropertyName = "sortid")]
		public string SortId { get; set; }

		[JsonProperty(PropertyName = "firstitemmsec")]
		public long FirstItemMsec { get; set; }

		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }

		[JsonProperty(PropertyName = "htmlUrl")]
		public string HtmlUrl { get; set; }

		[JsonProperty(PropertyName = "iconUrl")]
		public string IconUrl { get; set; }
	}

	public class Category
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "label")]
		public string Label { get; set; }
	}
}