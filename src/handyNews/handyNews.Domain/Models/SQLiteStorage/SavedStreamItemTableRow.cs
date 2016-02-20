using System;
using SQLite.Net.Attributes;

namespace handyNews.Domain.Models.SQLiteStorage
{
    [Table("SAVED_STREAM_ITEM")]
    class SavedStreamItemTableRow
    {
        [Column("ID")]
        [PrimaryKey]
        [NotNull]
        public string Id { get; set; }

        [Column("TITLE")]
        public string Title { get; set; }

        [Column("PUBLISHED")]
        public DateTime Published { get; set; }

        [Column("WEBURI")]
        public string WebUri { get; set; }

        [Column("SHORT_CONTENT")]
        public string ShortContent { get; set; }

        [Column("CONTENT")]
        public string Content { get; set; }

        [Column("IMAGE_FOLDER")]
        public string ImageFolder { get; set; }
    }
}