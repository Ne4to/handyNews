using SQLite.Net.Attributes;

namespace handyNews.Domain.Models.SQLiteStorage
{
    [Table("SUB_ITEM")]
    class SubItemTableRow
    {
        [Column("ID")]
        [PrimaryKey]
        [NotNull]
        public string Id { get; set; }

        [Column("SORT_ID")]
        public string SortId { get; set; }

        [Column("TITLE")]
        public string Title { get; set; }

        [Column("UNREAD_COUNT")]
        public long UnreadCount { get; set; }

        [Column("URL")]
        public string Url { get; set; }

        [Column("HTML_URL")]
        public string HtmlUrl { get; set; }

        [Column("ICON_URL")]
        public string IconUrl { get; set; }

        [Column("FIRST_ITEM_MSEC")]
        public long FirstItemMsec { get; set; }

        [Column("IS_MAX_COUNT")]
        [NotNull]
        [Default(value: false)]
        public bool IsMaxCount { get; set; }
    }
}