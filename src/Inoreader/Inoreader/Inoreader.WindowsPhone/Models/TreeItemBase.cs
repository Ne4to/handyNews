using System.Runtime.Serialization;

namespace Inoreader.Models
{
	[DataContract]
	public abstract class TreeItemBase
	{
		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string SortId { get; set; }

		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public int UnreadCount { get; set; }
	}
}