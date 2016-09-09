namespace handyNews.Domain.Models.SQLiteStorage
{
    internal static class StorageExtensions
    {
        public static Feed ToModel(this SubCatTableRow row)
        {
            return new Feed
            {
                Id = row.Id,
                SortId = row.SortId,
                Title = row.Title,
                PageTitle = row.Title,
                UnreadCount = row.UnreadCount,
                ApproxUnreadCount = row.IsMaxCount
            };
        }

        public static Feed ToModel(this SubItemTableRow row)
        {
            return new Feed
            {
                Id = row.Id,
                SortId = row.SortId,
                Title = row.Title,
                PageTitle = row.Title,
                UnreadCount = row.UnreadCount,
                ApproxUnreadCount = row.IsMaxCount,
                Url = row.Url,
                HtmlUrl = row.HtmlUrl,
                FirstItemMsec = row.FirstItemMsec
            };
        }
    }
}