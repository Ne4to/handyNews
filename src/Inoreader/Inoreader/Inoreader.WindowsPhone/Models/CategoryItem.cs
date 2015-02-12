using System.Collections.Generic;

namespace Inoreader.Models
{
	public class CategoryItem : TreeItemBase
	{
		public List<SubscriptionItem> Subscriptions { get; set; }
	}
}