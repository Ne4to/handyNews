using System.Collections.Generic;

namespace handyNews.Domain.Models
{
    public class CategoryItem : SubscriptionItemBase
    {
        public List<SubscriptionItem> Subscriptions { get; set; }
    }
}