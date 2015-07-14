using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Inoreader.Annotations;
using Inoreader.Domain.Services;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.ViewModels.Pages
{
	public class SavedItemPageViewModel : ViewModel
	{
		private readonly SavedStreamManager _savedStreamManager;

		private SavedStreamItem _item;

		public SavedStreamItem Item
		{
			get { return _item; }
			set { SetProperty(ref _item, value); }
		}

		public SavedItemPageViewModel([NotNull] SavedStreamManager savedStreamManager)
		{
			if (savedStreamManager == null) throw new ArgumentNullException("savedStreamManager");
			_savedStreamManager = savedStreamManager;
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			var id = (string) navigationParameter;
			Item = _savedStreamManager.Items.First(t => t.Id == id);
		}
	}
}