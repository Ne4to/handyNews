using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Navigation;
using Inoreader.Annotations;
using Inoreader.Services;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.ViewModels.Pages
{
	public class SavedPageViewModel : ViewModel
	{
		private readonly SavedStreamManager _savedStreamManager;
		private ObservableCollection<SavedStreamItem> _items;

		public ObservableCollection<SavedStreamItem> Items
		{
			get { return _items; }
			set { SetProperty(ref _items, value); }
		}

		public SavedPageViewModel([NotNull] SavedStreamManager savedStreamManager)
		{
			_savedStreamManager = savedStreamManager;
			if (savedStreamManager == null) throw new ArgumentNullException("savedStreamManager");
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

			Items = new ObservableCollection<SavedStreamItem>(_savedStreamManager.Items);
		}
	}
}