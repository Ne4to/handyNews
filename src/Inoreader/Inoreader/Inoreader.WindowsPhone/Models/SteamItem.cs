using System;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.Models
{
	public class SteamItem : BindableBase
	{
		#region Fields

		private bool _unread = true;
		private bool _needSetReadExplicitly;
		private bool _isSelected;

		#endregion
		
		#region Properties
		
		public string Id { get; set; }
		public DateTimeOffset Published { get; set; }
		public string Title { get; set; }
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

		#endregion

	}
}