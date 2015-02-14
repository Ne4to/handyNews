using System.Runtime.Serialization;

namespace Inoreader.Models
{
	[DataContract]
	public class SubscriptionItem : TreeItemBase
	{
		[DataMember]
		public string Url { get; set; }

		[DataMember]
		public string HtmlUrl { get; set; }

		[DataMember]
		public string IconUrl { get; set; }

		[DataMember]
		public long FirstItemMsec { get; set; }
	}
}