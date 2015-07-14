namespace Inoreader.Domain.Models
{
	public abstract class TreeItemBase : BindableBaseEx
	{
		private long _unreadCount;

		public string Id { get; set; }
		public string SortId { get; set; }
		public string Title { get; set; }
		public string PageTitle { get; set; }
		
		public long UnreadCount
		{
			get { return _unreadCount; }
			set { SetProperty(ref _unreadCount, value); }
		}
	}
}