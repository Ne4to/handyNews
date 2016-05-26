using SQLite.Net.Attributes;

namespace handyNews.Domain.Models.SQLiteStorage
{
    [Table("SUB_CAT")]
    internal class SubCatTableRow
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

        [Column("IS_MAX_COUNT")]
        [NotNull]
        [Default(value: false)]
        public bool IsMaxCount { get; set; }
    }
}