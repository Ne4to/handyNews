using System;
using System.Runtime.Serialization;

namespace Inoreader.Domain.Models
{
	[DataContract]
	public class StreamItem : BindableBaseEx
	{
		#region Fields

		[DataMember]
		private bool _unread = true;

		[DataMember]
		private bool _needSetReadExplicitly;

		[DataMember]
		private bool _isSelected;

		[DataMember]
		private bool _starred;

		[DataMember]
		private bool _saved;

		#endregion

		#region Properties

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
			set { SetProperty(ref _unread, value); }
		}

		public bool NeedSetReadExplicitly
		{
			get { return _needSetReadExplicitly; }
			set { SetProperty(ref _needSetReadExplicitly, value); }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set { SetProperty(ref _isSelected, value); }
		}

		public bool Starred
		{
			get { return _starred; }
			set { SetProperty(ref _starred, value); }
		}

		public bool Saved
		{
			get { return _saved; }
			set { SetProperty(ref _saved, value); }
		}

		#endregion

	}
}