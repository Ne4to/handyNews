using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Inoreader.Annotations;
using Inoreader.Api;
using Inoreader.Api.Exceptions;
using Inoreader.Domain.Models;
using Inoreader.Domain.Models.States;
using Inoreader.Domain.Services;
using Inoreader.Domain.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

namespace Inoreader.ViewModels.Pages
{
	public class StreamPageViewModel : ViewModel
	{
		#region Fields

		private readonly ApiClient _apiClient;
		private readonly INavigationService _navigationService;
		private readonly ITelemetryManager _telemetryManager;
		private readonly TagsManager _tagsManager;
		private readonly SavedStreamManager _savedStreamManager;
		private readonly LocalStorageManager _localStorageManager;
		private readonly NetworkManager _networkManager;
		private readonly CoreDispatcher _dispatcher;
		private readonly bool _showNewestFirst;
		private readonly bool _autoMarkAsRead;
		private string _streamId;
		private bool _isStarsList;

		private string _title;
		private StreamItemCollection _items;
		private bool _isBusy;
		private bool _currentItemRead;
		private bool _currentItemReadEnabled;
		private bool _currentItemStarred;
		private bool _currentItemStarredEnabled;
		private StreamItem _currentItem;
		private bool _isOffline;
		private bool _allArticles;

		private ICommand _itemsScrollCommand;
		private ICommand _selectItemCommand;
		private DelegateCommand _openWebCommand;
		private ICommand _refreshCommand;
		private DelegateCommand _saveCommand;
		private DelegateCommand _shareCommand;
		private ICommand _readItemCommand;
		private ICommand _starItemCommand;
		private ICommand _allArticlesCommand;
		private ICommand _unreadArticlesCommand;
		private ICommand _markAllAsReadCommand;
		private int _preloadItemCount;
		
		#endregion

		#region Properties

		public string Title
		{
			get { return _title; }
			private set { SetProperty(ref _title, value); }
		}

		public StreamItemCollection Items
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

		public bool CurrentItemReadEnabled
		{
			get { return _currentItemReadEnabled; }
			private set { SetProperty(ref _currentItemReadEnabled, value); }
		}

		public bool CurrentItemStarred
		{
			get { return _currentItemStarred; }
			set
			{
				if (SetProperty(ref _currentItemStarred, value))
					OnCurrentItemStarredChanged(value);
			}
		}

		public bool CurrentItemStarredEnabled
		{
			get { return _currentItemStarredEnabled; }
			set { SetProperty(ref _currentItemStarredEnabled, value); }
		}

		public bool IsOffline
		{
			get { return _isOffline; }
			private set { SetProperty(ref _isOffline, value); }
		}

		public bool AllArticles
		{
			get { return _allArticles; }
			set { SetProperty(ref _allArticles, value); }
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

		public ICommand OpenWebCommand
		{
			get { return _openWebCommand ?? (_openWebCommand = new DelegateCommand(OnOpenWeb, CanOpenWeb)); }
		}

		public ICommand RefreshCommand
		{
			get { return _refreshCommand ?? (_refreshCommand = new DelegateCommand(OnRefresh)); }
		}

		public ICommand SaveCommand
		{
			get { return _saveCommand ?? (_saveCommand = new DelegateCommand(OnSave, CanSave)); }
		}

		public ICommand ShareCommand
		{
			get { return _shareCommand ?? (_shareCommand = new DelegateCommand(OnShare, CanShare)); }
		}

		public ICommand ReadItemCommand
		{
			get { return _readItemCommand ?? (_readItemCommand = new DelegateCommand<object>(OnReadItem)); }
		}

		public ICommand StarItemCommand
		{
			get { return _starItemCommand ?? (_starItemCommand = new DelegateCommand<object>(OnStarItem)); }
		}

		public ICommand AllArticlesCommand
		{
			get { return _allArticlesCommand ?? (_allArticlesCommand = new DelegateCommand(OnAllArticles)); }
		}

		public ICommand UnreadArticlesCommand
		{
			get { return _unreadArticlesCommand ?? (_unreadArticlesCommand = new DelegateCommand(OnUnreadArticles)); }
		}

		public ICommand MarkAllAsReadCommand
		{
			get { return _markAllAsReadCommand ?? (_markAllAsReadCommand = new DelegateCommand(OnMarkAllAsRead)); }
		}

		#endregion

		public StreamPageViewModel([NotNull] ApiClient apiClient,
			[NotNull] INavigationService navigationService,
			[NotNull] ITelemetryManager telemetryManager,
			[NotNull] TagsManager tagsManager,
			[NotNull] AppSettingsService settingsService,
			[NotNull] SavedStreamManager savedStreamManager,
			[NotNull] LocalStorageManager localStorageManager,
			[NotNull] NetworkManager networkManager)
		{
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (navigationService == null) throw new ArgumentNullException("navigationService");
			if (telemetryManager == null) throw new ArgumentNullException("telemetryManager");
			if (tagsManager == null) throw new ArgumentNullException("tagsManager");
			if (settingsService == null) throw new ArgumentNullException("settingsService");
			if (savedStreamManager == null) throw new ArgumentNullException("savedStreamManager");
			if (localStorageManager == null) throw new ArgumentNullException("localStorageManager");
			if (networkManager == null) throw new ArgumentNullException("networkManager");

			_apiClient = apiClient;
			_navigationService = navigationService;
			_telemetryManager = telemetryManager;
			_tagsManager = tagsManager;
			_savedStreamManager = savedStreamManager;
			_localStorageManager = localStorageManager;
			_networkManager = networkManager;

			_showNewestFirst = settingsService.ShowNewestFirst;
			_autoMarkAsRead = settingsService.AutoMarkAsRead;
			_preloadItemCount = settingsService.PreloadItemCount;

			DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
			dataTransferManager.DataRequested += dataTransferManager_DataRequested;

			_dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

			_networkManager.NetworkChanged += _networkManager_NetworkChanged;
		}

		async void _networkManager_NetworkChanged(object sender, NetworkChangedEventArgs e)
		{
			await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsOffline = !e.Connected);
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			// The base implementation uses RestorableStateAttribute and Reflection to save and restore state
			// If you do not use this attribute, do not invoke base impkementation to prevent execution this useless code.

			var navParam = ((string) navigationParameter).FromJson<StreamPageNavigationParameter>(true);

			// backward compatibility (issue #62)
			if (navParam == null)
			{
				_streamId = (string)navigationParameter;
				_isStarsList = _streamId == SpecialTags.Starred;
				Title = _streamId;	
			}
			else
			{
				_streamId = navParam.StreamId;
				_isStarsList = _streamId == SpecialTags.Starred;
				Title = navParam.Title;	
			}
			
			if (!RestoreState(viewModelState))
				LoadData();
		}

		public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
		{
			// The base implementation uses RestorableStateAttribute and Reflection to save and restore state
			// If you do not use this attribute, do not invoke base impkementation to prevent execution this useless code.

			if (!suspending)
			{
				_networkManager.NetworkChanged -= _networkManager_NetworkChanged;
			}

			if (viewModelState != null)
				SaveState(viewModelState);

			DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
			dataTransferManager.DataRequested -= dataTransferManager_DataRequested;
		}

		private void SaveState(Dictionary<string, object> viewModelState)
		{
			viewModelState["Title"] = Title;

			if (Items != null)
				viewModelState["Items"] = Items.GetSate();

			viewModelState["CurrentItemRead"] = CurrentItemRead;
			viewModelState["CurrentItemReadEnabled"] = CurrentItemReadEnabled;
			viewModelState["CurrentItemStarred"] = CurrentItemStarred;
			viewModelState["CurrentItemStarredEnabled"] = CurrentItemStarredEnabled;
		}

		private bool RestoreState(Dictionary<string, object> viewModelState)
		{
			if (viewModelState == null)
				return false;
			
			object stateValue;
			if (viewModelState.TryGetValue("Title", out stateValue))
				Title = (string)stateValue;

			_currentItemRead = viewModelState.GetValue<bool>("CurrentItemRead");
			OnPropertyChanged("CurrentItemRead");

			CurrentItemReadEnabled = viewModelState.GetValue<bool>("CurrentItemReadEnabled");

			_currentItemStarred = viewModelState.GetValue<bool>("CurrentItemStarred");
			OnPropertyChanged("CurrentItemStarred");

			CurrentItemStarredEnabled = viewModelState.GetValue<bool>("CurrentItemStarredEnabled");

			var itemsState = viewModelState.GetValue<StreamItemCollectionState>("Items");
			if (itemsState == null)
				return false;

			_currentItem = itemsState.Items.FirstOrDefault(i => i.IsSelected);
			Items = new StreamItemCollection(itemsState, _apiClient, _telemetryManager, b => IsBusy = b, _preloadItemCount);
			Items.LoadMoreItemsError += (sender, args) => IsOffline = true;

			return true;
		}

		private async void LoadData()
		{
			IsBusy = true;

			Exception error = null;
			try
			{
				IsOffline = false;
				await LoadDataInternalAsync();
			}
			catch (AuthenticationApiException)
			{
				_apiClient.ClearSession();
				_navigationService.Navigate(PageTokens.SignIn, null);
				return;
			}
			catch (Exception ex)
			{
				error = ex;
				_telemetryManager.TrackError(ex);
			}
			finally
			{
				IsBusy = false;
			}

			if (error == null) return;

			IsOffline = true;

			IsBusy = true;
			var cacheData = await _localStorageManager.LoadStreamCollectionAsync(_streamId);
			IsBusy = false;

			if (cacheData != null)
			{
				var items = new StreamItemCollection(cacheData, _apiClient, _telemetryManager, b => IsBusy = b, _preloadItemCount);
				_currentItem = items.FirstOrDefault(i => i.IsSelected);
				_currentItemRead = _currentItem != null && !_currentItem.Unread;
				CurrentItemReadEnabled = _currentItem != null;
				_currentItemStarred = _currentItem != null && _currentItem.Starred;
				CurrentItemStarredEnabled = _currentItem != null;

				Items = items;
			}

			MessageDialog msgbox = new MessageDialog(error.Message, Strings.Resources.ErrorDialogTitle);
			await msgbox.ShowAsync();
		}

		private async Task LoadDataInternalAsync()
		{
			await _localStorageManager.ClearTempFilesAsync();

			var streamItems = new StreamItemCollection(_apiClient, _streamId, _showNewestFirst, _telemetryManager, AllArticles, b => IsBusy = b, _preloadItemCount);
			streamItems.LoadMoreItemsError += (sender, args) => IsOffline = true;
			await streamItems.InitAsync();
			
			Items = streamItems;
			_currentItem = Items.FirstOrDefault();

			if (_currentItem != null)
			{
				_currentItem.IsSelected = true;
				SetCurrentItemRead(!_currentItem.Unread);
				SetCurrentItemStarred(_currentItem.Starred);
			}
			else
			{
				SetCurrentItemRead(false);
				SetCurrentItemStarred(false);
			}

			CurrentItemReadEnabled = _currentItem != null && !(_currentItem is EmptySpaceStreamItem) && !(_currentItem is HeaderSpaceStreamItem);
			CurrentItemStarredEnabled = _currentItem != null && !(_currentItem is EmptySpaceStreamItem) && !(_currentItem is HeaderSpaceStreamItem);
			RaiseOpenWebCommandCanExecuteChanged();
			RaiseShareCommandCanExecuteChanged();
			RaiseSaveCommandCanExecuteChanged();

			if (Items != null)
			{
				var streamItemCollectionState = Items.GetSate();
				//await _cacheManager.SaveStreamAsync(streamItemCollectionState);
				await _localStorageManager.SaveStreamCollectionAsync(streamItemCollectionState);
			}
		}

		private void RaiseOpenWebCommandCanExecuteChanged()
		{
			if (_openWebCommand != null)
				_openWebCommand.RaiseCanExecuteChanged();
		}

		private void RaiseShareCommandCanExecuteChanged()
		{
			if (_shareCommand != null)
				_shareCommand.RaiseCanExecuteChanged();
		}

		private void RaiseSaveCommandCanExecuteChanged()
		{
			if (_saveCommand != null)
				_saveCommand.RaiseCanExecuteChanged();
		}

		private void OnItemsScroll(object obj)
		{
			var items = (object[])obj;
			var firstItem = (StreamItem)items[0];
			if (firstItem is EmptySpaceStreamItem || firstItem is HeaderSpaceStreamItem)
			{
				CurrentItemReadEnabled = false;
				CurrentItemStarredEnabled = false;
				return;
			}

			if (!firstItem.NeedSetReadExplicitly && firstItem.Unread && _autoMarkAsRead && !_isStarsList)
			{
				firstItem.Unread = false;
				MarkAsRead(firstItem.Id, true);
			}

			if (_currentItem != null)
				_currentItem.IsSelected = false;

			_currentItem = firstItem;
			_currentItem.IsSelected = true;
			SetCurrentItemRead(!_currentItem.Unread);
			SetCurrentItemStarred(_currentItem.Starred);

			//CurrentItemReadEnabled = _currentItem != null;
			CurrentItemStarredEnabled = _currentItem != null;
			RaiseOpenWebCommandCanExecuteChanged();
			RaiseShareCommandCanExecuteChanged();
			RaiseSaveCommandCanExecuteChanged();
		}

		private void OnSelectItem(object obj)
		{
			var item = obj as StreamItem;
			if (item != null && !(item is EmptySpaceStreamItem) && !(item is HeaderSpaceStreamItem))
			{
				if (_currentItem != null)
					_currentItem.IsSelected = false;

				_currentItem = item;
				_currentItem.IsSelected = true;

				if (!item.NeedSetReadExplicitly && item.Unread && !_isStarsList)
				{
					item.Unread = false;
					MarkAsRead(item.Id, true);
				}

				SetCurrentItemRead(!_currentItem.Unread);
				SetCurrentItemStarred(_currentItem.Starred);

				CurrentItemReadEnabled = _currentItem != null;
				CurrentItemStarredEnabled = _currentItem != null;
				RaiseOpenWebCommandCanExecuteChanged();
				RaiseShareCommandCanExecuteChanged();
				RaiseSaveCommandCanExecuteChanged();
			}
			else
			{
				CurrentItemReadEnabled = false;
				CurrentItemStarredEnabled = false;
			}
		}

		private bool CanOpenWeb()
		{
			Uri tmp;
			return _currentItem != null && !String.IsNullOrEmpty(_currentItem.WebUri) && Uri.TryCreate(_currentItem.WebUri, UriKind.Absolute, out tmp);
		}

		private async void OnOpenWeb()
		{
			if (_currentItem == null || String.IsNullOrEmpty(_currentItem.WebUri))
				return;

			var uri = new Uri(_currentItem.WebUri);
			_telemetryManager.TrackEvent(TelemetryEvents.OpenItemInWeb);
			await Launcher.LaunchUriAsync(uri);
		}

		private void OnRefresh()
		{
			_telemetryManager.TrackEvent(TelemetryEvents.ManualRefreshStream);
			LoadData();
		}

		private async void OnSave()
		{
			try
			{
				if (_currentItem.Saved)
				{
					_currentItem.Saved = false;
					await _savedStreamManager.DeleteAsync(_currentItem.Id);
				}
				else
				{
					_currentItem.Saved = true;
					await _savedStreamManager.AddAsync(_currentItem);
				}
			}
			catch (Exception e)
			{
				_telemetryManager.TrackError(e);
			}
		}

		private bool CanSave()
		{
			return _currentItem != null && !(_currentItem is EmptySpaceStreamItem) && !(_currentItem is HeaderSpaceStreamItem);
		}

		private void OnShare()
		{
			DataTransferManager.ShowShareUI();
		}

		private bool CanShare()
		{
			return _currentItem != null && !(_currentItem is EmptySpaceStreamItem) && !(_currentItem is HeaderSpaceStreamItem);
		}

		private void OnReadItem(object o)
		{
			var item = o as StreamItem;
			if (item == null || item is EmptySpaceStreamItem || item is HeaderSpaceStreamItem)
				return;

			item.NeedSetReadExplicitly = !item.Unread;
			item.Unread = !item.Unread;
			MarkAsRead(item.Id, !item.Unread);

			if (item == _currentItem)
			{
				SetCurrentItemRead(!item.Unread);
			}
		}

		private void OnStarItem(object o)
		{
			var item = o as StreamItem;
			if (item == null || item is EmptySpaceStreamItem || item is HeaderSpaceStreamItem)
				return;

			item.Starred = !item.Starred;
			MarkAsStarred(item.Id, item.Starred);

			if (item == _currentItem)
			{
				SetCurrentItemStarred(item.Starred);
			}
		}

		private void OnAllArticles()
		{
			AllArticles = true;
			OnRefresh();
		}

		private void OnUnreadArticles()
		{
			AllArticles = false;
			OnRefresh();
		}

		private async void OnMarkAllAsRead()
		{
			if (Items == null)
				return;

			var dlg = new MessageDialog(Strings.Resources.MarkAllAsReadDialogContent);
			dlg.Commands.Add(new UICommand(Strings.Resources.DialogCommandYes) { Id = 1 });
			dlg.Commands.Add(new UICommand(Strings.Resources.DialogCommandNo) { Id = 0 });
			var x = await dlg.ShowAsync();

			if ((int)x.Id != 1)
				return;

			Exception error = null;
			try
			{
				_telemetryManager.TrackEvent(TelemetryEvents.MarkAllAsRead);
				await _apiClient.MarkAllAsReadAsync(Items.StreamId, Items.StreamTimestamp);

				foreach (var item in Items)
				{
					item.Unread = false;
				}

				SetCurrentItemRead(true);
			}
			catch (Exception ex)
			{
				error = ex;
				_telemetryManager.TrackError(ex);
			}

			if (error == null) return;

			MessageDialog msgbox = new MessageDialog(error.Message, Strings.Resources.ErrorDialogTitle);
			await msgbox.ShowAsync();
		}

		private void MarkAsRead(string id, bool newValue)
		{
			if (newValue)
			{
				_tagsManager.MarkAsRead(id);
			}
			else
			{
				_tagsManager.MarkAsUnreadTagAction(id);
			}
		}

		private void MarkAsStarred(string id, bool newValue)
		{
			if (newValue)
			{
				_tagsManager.AddToStarred(id);
			}
			else
			{
				_tagsManager.RemoveFromStarred(id);
			}
		}

		private void SetCurrentItemRead(bool newValue)
		{
			_currentItemRead = newValue;
			OnPropertyChanged("CurrentItemRead");
		}

		private void SetCurrentItemStarred(bool newValue)
		{
			_currentItemStarred = newValue;
			OnPropertyChanged("CurrentItemStarred");
		}

		private void OnCurrentItemReadChanged(bool newValue)
		{
			if (_currentItem == null || _currentItem is EmptySpaceStreamItem || _currentItem is HeaderSpaceStreamItem)
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

		private void OnCurrentItemStarredChanged(bool newValue)
		{
			if (_currentItem == null || _currentItem is EmptySpaceStreamItem || _currentItem is HeaderSpaceStreamItem)
				return;

			if (newValue)
			{
				_currentItem.Starred = true;
				SetCurrentItemStarred(true);
				MarkAsStarred(_currentItem.Id, true);
			}
			else
			{
				_currentItem.Starred = false;
				SetCurrentItemStarred(false);
				MarkAsStarred(_currentItem.Id, false);
			}
		}

		void dataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
		{
			if (_currentItem == null || _currentItem is EmptySpaceStreamItem || _currentItem is HeaderSpaceStreamItem)
			{
				args.Request.FailWithDisplayText(Strings.Resources.ErrorShareMessage);
				return;
			}

			var dataPackage = args.Request.Data;
			dataPackage.Properties.Title = _currentItem.Title;
			if (!String.IsNullOrEmpty(_currentItem.WebUri))
				dataPackage.SetWebLink(new Uri(_currentItem.WebUri));

			dataPackage.SetHtmlFormat(_currentItem.Content);
		}
	}

	public class StreamPageNavigationParameter
	{
		public string Title { get; set; }
		public string StreamId { get; set; }
	}
}