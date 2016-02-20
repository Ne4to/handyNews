using SQLite.Net.Attributes;

namespace handyNews.Domain.Models
{
	public abstract class SubscriptionItemBase
	{
        [Column("ID")]
        public string Id { get; set; }

        [Column("SORT_ID")]
        public string SortId { get; set; }

        [Column("TITLE")]
        public string Title { get; set; }
		public string PageTitle { get; set; }

        [Column("UNREAD_COUNT")]
        public long UnreadCount { get; set; }

        [Column("IS_MAX_COUNT")]
	    public bool IsMaxUnread { get; set; }
	    public string IconUrl { get; set; }
	}
}