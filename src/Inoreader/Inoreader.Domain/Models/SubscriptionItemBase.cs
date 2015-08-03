namespace Inoreader.Domain.Models
{
	public abstract class SubscriptionItemBase
	{
		public string Id { get; set; }
		public string SortId { get; set; }
		public string Title { get; set; }
		public string PageTitle { get; set; }
		public long UnreadCount { get; set; }
	    public bool IsMaxUnread { get; set; }
	    public string IconUrl { get; set; }
	}
}