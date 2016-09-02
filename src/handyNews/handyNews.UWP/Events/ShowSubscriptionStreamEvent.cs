using System;
using handyNews.Domain.Models;

namespace handyNews.UWP.Events
{
    public class ShowSubscriptionStreamEvent : EventArgs
    {
        public ShowSubscriptionStreamEvent(SubscriptionItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Item = item;
        }

        public SubscriptionItem Item { get; private set; }
    }
}