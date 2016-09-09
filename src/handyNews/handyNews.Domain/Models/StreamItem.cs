using System;
using System.Runtime.Serialization;

namespace handyNews.Domain.Models
{
    [DataContract]
    public class StreamItem : BindableBaseEx
    {
        [DataMember] private bool _isSelected;

        [DataMember] private bool _needSetReadExplicitly;

        [DataMember] private bool _saved;

        [DataMember] private bool _starred;

        [DataMember] private bool _unread = true;

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public DateTimeOffset Published { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string WebUri { get; set; }

        [DataMember]
        public string Content { get; set; }

        public bool Unread
        {
            get { return _unread; }
            set { SetProperty(ref _unread, value, nameof(Unread)); }
        }

        public bool NeedSetReadExplicitly
        {
            get { return _needSetReadExplicitly; }
            set { SetProperty(ref _needSetReadExplicitly, value, nameof(NeedSetReadExplicitly)); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value, nameof(IsSelected)); }
        }

        public bool Starred
        {
            get { return _starred; }
            set { SetProperty(ref _starred, value, nameof(Starred)); }
        }

        public bool Saved
        {
            get { return _saved; }
            set { SetProperty(ref _saved, value, nameof(Saved)); }
        }
    }
}