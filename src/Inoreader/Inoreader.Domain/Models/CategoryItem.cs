using System.Collections.Generic;

namespace Inoreader.Domain.Models
{	
	public class CategoryItem : SubscriptionItemBase
	{
		public List<SubscriptionItem> Subscriptions { get; set; }
	}
}