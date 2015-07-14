using System.Collections.Generic;

namespace Inoreader.Domain.Models
{	
	public class CategoryItem : TreeItemBase
	{
		public List<SubscriptionItem> Subscriptions { get; set; }
	}
}