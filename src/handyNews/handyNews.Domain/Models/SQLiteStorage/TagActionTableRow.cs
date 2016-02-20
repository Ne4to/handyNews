using SQLite.Net.Attributes;

namespace handyNews.Domain.Models.SQLiteStorage
{
    [Table("TAG_ACTION")]
    class TagActionTableRow
    {
        [Column("ID")]
        [PrimaryKey]
        [NotNull]
        [AutoIncrement]
        public long Id { get; set; }

        [Column("ITEM_ID")]
        public string ItemId { get; set; }

        [Column("TAG")]
        public string Tag { get; set; }

        [Column("ACTION_KIND")]
        public int ActionKind { get; set; }
    }
}