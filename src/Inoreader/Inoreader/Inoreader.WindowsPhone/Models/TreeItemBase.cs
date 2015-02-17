using System.Runtime.Serialization;

namespace Inoreader.Models
{
	[DataContract]
	public abstract class TreeItemBase : BindableBaseEx
	{
		private int _unreadCount;

		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string SortId { get; set; }

		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public int UnreadCount
		{
			get { return _unreadCount; }
			set { SetProperty(ref _unreadCount, value); }
		}
	}
}