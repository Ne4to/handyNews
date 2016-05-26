using System;
using SQLite.Net.Attributes;

namespace handyNews.Domain.Models.SQLiteStorage
{
    [Table("STREAM_ITEM")]
    internal class StreamItemTableRow
    {
        [Column("ID")]
        [PrimaryKey]
        [NotNull]
        public string Id { get; set; }

        [Column("STREAM_ID")]
        public string StreamId { get; set; }

        [Column("PUBLISHED")]
        public DateTime Published { get; set; }

        [Column("TITLE")]
        public string Title { get; set; }

        [Column("WEB_URI")]
        public string WebUri { get; set; }

        [Column("CONTENT")]
        public string Content { get; set; }

        [Column("UNREAD")]
        public bool Unread { get; set; }

        [Column("NEED_SET_READ_EXPLICITLY")]
        public bool NeedSetReadExplicitly { get; set; }

        [Column("IS_SELECTED")]
        public bool IsSelected { get; set; }

        [Column("STARRED")]
        public bool Starred { get; set; }

        [Column("SAVED")]
        public bool Saved { get; set; }
    }
}