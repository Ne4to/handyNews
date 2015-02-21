using System;
using System.Runtime.Serialization;
using Inoreader.Services;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.Models
{
	[DataContract]
	public class StreamItem : BindableBaseEx
	{
		private string _shortPlainText;

		#region Fields

		[DataMember]
		private bool _unread = true;

		[DataMember]
		private bool _needSetReadExplicitly;

		[DataMember]
		private bool _isSelected;

		[DataMember]
		private bool _starred;

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

		public string ShortPlainText
		{
			get { return _shortPlainText ?? (_shortPlainText = HtmlParser.GetPlainText(Content, 200)); }
		}

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

		#endregion

	}
}