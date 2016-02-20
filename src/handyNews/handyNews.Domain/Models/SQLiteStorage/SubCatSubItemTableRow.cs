using SQLite.Net.Attributes;

namespace handyNews.Domain.Models.SQLiteStorage
{
    [Table("SUB_CAT_SUB_ITEM")]
    class SubCatSubItemTableRow
    {
        [Column("CAT_ID")]
        public string CatId { get; set; }

        [Column("ITEM_ID")]
        public string ItemId { get; set; }
    }
}