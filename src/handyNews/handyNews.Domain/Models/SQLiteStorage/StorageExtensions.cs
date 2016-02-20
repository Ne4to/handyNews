namespace handyNews.Domain.Models.SQLiteStorage
{
    static class StorageExtensions
    {
        public static CategoryItem ToModel(this SubCatTableRow row)
        {
            return new CategoryItem
            {
                Id = row.Id,
                SortId = row.SortId,
                Title = row.Title,
                PageTitle = row.Title,
                UnreadCount = row.UnreadCount,
                IsMaxUnread = row.IsMaxCount
            };
        }

        public static SubscriptionItem ToModel(this SubItemTableRow row)
        {
            return new SubscriptionItem
            {
                Id = row.Id,
                SortId = row.SortId,
                Title = row.Title,
                PageTitle = row.Title,
                UnreadCount = row.UnreadCount,
                IsMaxUnread = row.IsMaxCount,
                Url = row.Url,
                HtmlUrl = row.HtmlUrl,
                FirstItemMsec = row.FirstItemMsec
            };
        }
    }
}