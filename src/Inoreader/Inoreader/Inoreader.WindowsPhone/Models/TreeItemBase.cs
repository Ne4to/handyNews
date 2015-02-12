namespace Inoreader.Models
{
	public abstract class TreeItemBase
	{
		public string Id { get; set; }
		public string SortId { get; set; }
		public string Title { get; set; }
		public int UnreadCount { get; set; }
	}
}