using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml.Navigation;
using Inoreader.Annotations;
using Inoreader.Domain.Services;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

namespace Inoreader.ViewModels.Pages
{
	public class SavedPageViewModel : ViewModel
	{
		private readonly SavedStreamManager _savedStreamManager;
		private readonly INavigationService _navigationService;

		private ObservableCollection<SavedStreamItem> _items;

		private ICommand _selectItemCommand;

		public ObservableCollection<SavedStreamItem> Items
		{
			get { return _items; }
			set { SetProperty(ref _items, value); }
		}

		public ICommand SelectItemCommand
		{
			get { return _selectItemCommand ?? (_selectItemCommand = new DelegateCommand<object>(OnSelectItem)); }
		}

		public SavedPageViewModel([NotNull] SavedStreamManager savedStreamManager, INavigationService navigationService)
		{
			if (savedStreamManager == null) throw new ArgumentNullException("savedStreamManager");
			if (navigationService == null) throw new ArgumentNullException("navigationService");
			
			_savedStreamManager = savedStreamManager;
			_navigationService = navigationService;
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

			Items = new ObservableCollection<SavedStreamItem>(_savedStreamManager.Items);
		}

		private void OnSelectItem(object parameter)
		{
			var item = parameter as SavedStreamItem;
			if (item == null)
				return;

			_navigationService.Navigate(PageTokens.SavedItem, item.Id);
		}
	}
}