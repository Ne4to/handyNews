using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using Inoreader.Api;
using Inoreader.Models;
using Inoreader.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Newtonsoft.Json.Linq;

namespace Inoreader.ViewModels.Pages
{
	public class StreamPageViewModel : ViewModel
	{
		#region Fields

		private readonly ApiClient _apiClient;
		private readonly INavigationService _navigationService;
		private readonly TelemetryClient _telemetryClient;
		private string _steamId;

		private string _title;
		private SteamItemCollection _items;
		private bool _isBusy;
		private bool _currentItemRead;
		private SteamItem _currentItem;

		private ICommand _itemsScrollCommand;
		private ICommand _selectItemCommand;

		#endregion

		#region Properties

		public string Title
		{
			get { return _title; }
			private set { SetProperty(ref _title, value); }
		}

		public SteamItemCollection Items
		{
			get { return _items; }
			private set { SetProperty(ref _items, value); }
		}

		public bool IsBusy
		{
			get { return _isBusy; }
			private set { SetProperty(ref _isBusy, value); }
		}

		public bool CurrentItemRead
		{
			get { return _currentItemRead; }
			set
			{
				if (SetProperty(ref _currentItemRead, value))
					OnCurrentItemReadChanged(value);
			}
		}

		#endregion

		#region Commands

		public ICommand ItemsScrollCommand
		{
			get { return _itemsScrollCommand ?? (_itemsScrollCommand = new DelegateCommand<object>(OnItemsScroll)); }
		}

		public ICommand SelectItemCommand
		{
			get { return _selectItemCommand ?? (_selectItemCommand = new DelegateCommand<object>(OnSelectItem)); }
		}

		#endregion


		public StreamPageViewModel(ApiClient apiClient, INavigationService navigationService, TelemetryClient telemetryClient)
		{
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (navigationService == null) throw new ArgumentNullException("navigationService");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");

			_apiClient = apiClient;
			_navigationService = navigationService;
			_telemetryClient = telemetryClient;
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

			_steamId = (string)navigationParameter;
			LoadData();
		}

		public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
		{
			base.OnNavigatedFrom(viewModelState, suspending);
		}
#if DEBUG
		class AAA
		{
			public List<string> Items { get; set; }

			public AAA()
			{
				Items = new List<string>();
			}
		}
		static AAA testData = new AAA();
#endif
		private async void LoadData()
		{
			IsBusy = true;

			Exception error = null;
			try
			{
				var steamItems = new SteamItemCollection(_apiClient, _steamId, _telemetryClient, b => IsBusy = b);
				Title = await steamItems.InitAsync();

				Items = steamItems;
				_currentItem = Items.FirstOrDefault();
				if (_currentItem != null)
				{
					_currentItem.IsSelected = true;
				}
			}
			catch (Exception ex)
			{
				error = ex;
				_telemetryClient.TrackException(ex);
			}
			finally
			{
				IsBusy = false;
			}

			if (error != null)
			{
				MessageDialog msgbox = new MessageDialog(error.Message, Strings.Resources.ErrorDialogTitle);
				await msgbox.ShowAsync();
			}
#if DEBUG
			testData.Items.AddRange(Items.Select(i => i.Content));
			var tt = JObject.FromObject(testData).ToString();
#endif
		}

		private void OnItemsScroll(object obj)
		{
			var items = (object[])obj;
			var firstItem = (SteamItem)items[0];
			if (firstItem is EmptySpaceSteamItem)
				return;

			if (!firstItem.NeedSetReadExplicitly && firstItem.Unread)
			{
				firstItem.Unread = false;
				MarkAsRead(firstItem.Id, true);
			}

			if (_currentItem != null)
				_currentItem.IsSelected = false;

			_currentItem = firstItem;
			_currentItem.IsSelected = true;
			SetCurrentItemRead(!_currentItem.Unread);
		}

		private void OnSelectItem(object obj)
		{
			var item = obj as SteamItem;
			if (item != null && !(item is EmptySpaceSteamItem))
			{
				if (_currentItem != null)
					_currentItem.IsSelected = false;

				_currentItem = item;
				_currentItem.IsSelected = true;
				SetCurrentItemRead(!_currentItem.Unread);
			}
		}

		private async void MarkAsRead(string id, bool newValue)
		{
			var eventTelemetry = new EventTelemetry(TelemetryEvents.MarkAsRead);
			eventTelemetry.Properties.Add("AsRead", newValue.ToString());
			_telemetryClient.TrackEvent(eventTelemetry);

			try
			{
				if (newValue)
				{
					await _apiClient.AddTagAsync(SpecialTags.MarkItemAsRead, id);
				}
				else
				{
					await _apiClient.RemoveTagAsync(SpecialTags.MarkItemAsRead, id);
				}
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackException(ex);
			}
		}

		private void SetCurrentItemRead(bool newValue)
		{
			_currentItemRead = newValue;
			OnPropertyChanged("CurrentItemRead");
		}

		private void OnCurrentItemReadChanged(bool newValue)
		{
			if (_currentItem == null || _currentItem is EmptySpaceSteamItem)
				return;

			if (newValue)
			{
				_currentItem.Unread = false;
				_currentItem.NeedSetReadExplicitly = false;
				SetCurrentItemRead(true);
				MarkAsRead(_currentItem.Id, true);
			}
			else
			{
				_currentItem.Unread = true;
				_currentItem.NeedSetReadExplicitly = true;
				SetCurrentItemRead(false);
				MarkAsRead(_currentItem.Id, false);
			}
		}
	}
}