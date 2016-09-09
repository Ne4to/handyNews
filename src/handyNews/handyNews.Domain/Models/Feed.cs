using System.Collections.Generic;

namespace handyNews.Domain.Models
{
    public class Feed
    {
        public string Id { get; set; }
        public string SortId { get; set; }
        public string Title { get; set; }
        public string PageTitle { get; set; }
        public long UnreadCount { get; set; }
        public bool ApproxUnreadCount { get; set; }
        public string IconUrl { get; set; }


        public string Url { get; set; }
        public string HtmlUrl { get; set; }
        public long FirstItemMsec { get; set; }
        public IReadOnlyCollection<Feed> Children { get; set; }
    }
}