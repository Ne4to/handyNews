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
		private readonly LocalCacheManager _localCacheManager;
		private ObservableCollection<LocalStreamItem> _items;

		public ObservableCollection<LocalStreamItem> Items
		{
			get { return _items; }
			set { SetProperty(ref _items, value); }
		}

		public SavedPageViewModel([NotNull] LocalCacheManager localCacheManager)
		{
			_localCacheManager = localCacheManager;
			if (localCacheManager == null) throw new ArgumentNullException("localCacheManager");
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

			Items = new ObservableCollection<LocalStreamItem>(_localCacheManager.Items);
		}
	}
}