using SQLite.Net.Attributes;

namespace handyNews.Domain.Models.SQLiteStorage
{
    [Table("STREAM_COLLECTION")]
    internal class StreamCollectionTableRow
    {
        [Column("STREAM_ID")]
        [PrimaryKey]
        [NotNull]
        public string StreamId { get; set; }

        [Column("CONTINUATION")]
        public string Continuation { get; set; }

        [Column("SHOW_NEWEST_FIRST")]
        public bool ShowNewestFirst { get; set; }

        [Column("STREAM_TIMESTAMP")]
        public int StreamTimestamp { get; set; }

        [Column("FAULT")]
        public bool Fault { get; set; }
    }
}