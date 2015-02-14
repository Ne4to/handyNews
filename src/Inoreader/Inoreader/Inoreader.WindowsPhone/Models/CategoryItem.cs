using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Inoreader.Models
{
	[DataContract]
	public class CategoryItem : TreeItemBase
	{
		[DataMember]
		public List<SubscriptionItem> Subscriptions { get; set; }
	}
}