using System;
using handyNews.Domain.Models;

namespace handyNews.UWP.Events
{
    public class ShowSubscriptionStreamEvent : EventArgs
    {
        public Feed Item { get; private set; }

        public ShowSubscriptionStreamEvent(Feed item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Item = item;
        }
    }
}