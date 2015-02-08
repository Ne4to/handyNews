using Newtonsoft.Json;

namespace Inoreader.Api.Models
{
	public class UnreadCountResponse
	{
		[JsonProperty(PropertyName = "max")]
		public int Max { get; set; }

		[JsonProperty(PropertyName = "unreadcounts")]
		public Unreadcount[] UnreadCounts { get; set; }
	}

	public class Unreadcount
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "count")]
		public int Count { get; set; }

		[JsonProperty(PropertyName = "newestItemTimestampUsec")]
		public string NewestItemTimestampUsec { get; set; }
	}

}