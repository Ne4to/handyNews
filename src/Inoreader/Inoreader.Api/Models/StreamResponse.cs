namespace Inoreader.Api.Models
{
	public class StreamResponse
	{
		public string direction { get; set; }
		public string id { get; set; }
		public string title { get; set; }
		public string description { get; set; }
		public Self self { get; set; }
		public int updated { get; set; }
		public string updatedUsec { get; set; }
		public Item[] items { get; set; }
		public string continuation { get; set; }
	}

	public class Self
	{
		public string href { get; set; }
	}

	public class Item
	{
		public string crawlTimeMsec { get; set; }
		public string timestampUsec { get; set; }
		public string id { get; set; }
		public string[] categories { get; set; }
		public string title { get; set; }
		public int published { get; set; }
		public int updated { get; set; }
		public Canonical[] canonical { get; set; }
		public Alternate[] alternate { get; set; }
		public Summary summary { get; set; }
		public string author { get; set; }
		public object[] likingUsers { get; set; }
		public object[] comments { get; set; }
		public int commentsNum { get; set; }
		public object[] annotations { get; set; }
		public Origin origin { get; set; }
	}

	public class Summary
	{
		public string direction { get; set; }
		public string content { get; set; }
	}

	public class Origin
	{
		public string streamId { get; set; }
		public string title { get; set; }
		public string htmlUrl { get; set; }
	}

	public class Canonical
	{
		public string href { get; set; }
	}

	public class Alternate
	{
		public string href { get; set; }
		public string type { get; set; }
	}

}