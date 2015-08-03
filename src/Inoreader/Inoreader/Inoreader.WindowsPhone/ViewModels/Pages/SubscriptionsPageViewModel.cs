using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Inoreader.Annotations;
using Inoreader.Api;
using Inoreader.Api.Exceptions;
using Inoreader.Domain.Models;
using Inoreader.Domain.Services;
using Inoreader.Domain.Services.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

namespace Inoreader.ViewModels.Pages
{
	public class SubscriptionsPageViewModel : ViewModel, INavigateBackwards
	{
		#region Fields

		private readonly INavigationService _navigationService;
		private readonly IStreamManager _streamManager;
		private readonly ISettingsManager _settingsService;
		private readonly ITelemetryManager _telemetryManager;
		private readonly TileManager _tileManager;
		private readonly LocalStorageManager _localStorageManager;
		private readonly ISubscriptionsManager _subscriptionsManager;
		private readonly NetworkManager _networkManager;
	    private readonly ISignInManager _signInManager;
	    private readonly CoreDispatcher _dispatcher;

		private bool _isBusy;
		private bool _isOffline;
		private List<SubscriptionItemBase> _treeItems;
		private List<SubscriptionItemBase> _rootItems;
		private bool _isRoot = true;
		private string _categoryId;
		private string _subscriptionsHeader = Strings.Resources.SubscriptionsSectionHeader;

		private ICommand _settingsPageCommand;
		private ICommand _aboutPageCommand;
		private ICommand _signOutCommand;
		private ICommand _starsCommand;
		private ICommand _savedCommand;
		private ICommand _itemClickCommand;
		private ICommand _refreshCommand;
		private ICommand _markAllAsReadCommand;
		
		#endregion

		#region Properties

		public string SubscriptionsHeader
		{
			get { return _subscriptionsHeader; }
			private set { SetProperty(ref _subscriptionsHeader, value); }
		}

		public List<SubscriptionItemBase> TreeItems
		{
			get { return _treeItems; }
			private set { SetProperty(ref _treeItems, value); }
		}

		public bool IsBusy
		{
			get { return _isBusy; }
			private set { SetProperty(ref _isBusy, value); }
		}

		public bool IsOffline
		{
			get { return _isOffline; }
			private set { SetProperty(ref _isOffline, value); }
		}

		#endregion

		#region Commands

		public ICommand SettingsPageCommand
		{
			get { return _settingsPageCommand ?? (_settingsPageCommand = new DelegateCommand(OnSettingsPage)); }
		}

		public ICommand AboutPageCommand
		{
			get { return _aboutPageCommand ?? (_aboutPageCommand = new DelegateCommand(OnAboutPage)); }
		}

		public ICommand SignOutCommand
		{
			get { return _signOutCommand ?? (_signOutCommand = new DelegateCommand(OnSignOut)); }
		}

		public ICommand StarsCommand
		{
			get { return _starsCommand ?? (_starsCommand = new DelegateCommand(OnStars)); }
		}

		public ICommand SavedCommand
		{
			get { return _savedCommand ?? (_savedCommand = new DelegateCommand(OnSaved)); }
		}

		public ICommand ItemClickCommand
		{
			get { return _itemClickCommand ?? (_itemClickCommand = new DelegateCommand<object>(OnItemClick)); }
		}

		public ICommand RefreshCommand
		{
			get { return _refreshCommand ?? (_refreshCommand = new DelegateCommand(OnRefresh)); }
		}

		public ICommand MarkAllAsReadCommand
		{
			get { return _markAllAsReadCommand ?? (_markAllAsReadCommand = new DelegateCommand(OnMarkAllAsRead)); }
		}

		#endregion
		
		public SubscriptionsPageViewModel([NotNull] INavigationService navigationService,
			[NotNull] IStreamManager streamManager,
			[NotNull] ISettingsManager settingsService,
			[NotNull] ITelemetryManager telemetryManager,
			[NotNull] TileManager tileManager,
			[NotNull] LocalStorageManager localStorageManager,
			[NotNull] ISubscriptionsManager subscriptionsManager,
			[NotNull] NetworkManager networkManager,
            [NotNull] ISignInManager signInManager)
		{
			if (navigationService == null) throw new ArgumentNullException(nameof(navigationService));
			if (streamManager == null) throw new ArgumentNullException(nameof(streamManager));
			if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
			if (telemetryManager == null) throw new ArgumentNullException(nameof(telemetryManager));
			if (tileManager == null) throw new ArgumentNullException(nameof(tileManager));
			if (localStorageManager == null) throw new ArgumentNullException(nameof(localStorageManager));
			if (subscriptionsManager == null) throw new ArgumentNullException(nameof(subscriptionsManager));
			if (networkManager == null) throw new ArgumentNullException(nameof(networkManager));
		    if (signInManager == null) throw new ArgumentNullException(nameof(signInManager));

		    _navigationService = navigationService;
			_streamManager = streamManager;
			_settingsService = settingsService;
			_telemetryManager = telemetryManager;
			_tileManager = tileManager;
			_localStorageManager = localStorageManager;
			_subscriptionsManager = subscriptionsManager;
			_networkManager = networkManager;
		    _signInManager = signInManager;

		    _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

			Application.Current.Resuming += Application_Resuming;
			_networkManager.NetworkChanged += _networkManager_NetworkChanged;
		}

		async void _networkManager_NetworkChanged(object sender, NetworkChangedEventArgs e)
		{
			await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsOffline = !e.Connected);
		}

		void Application_Resuming(object sender, object e)
		{
			LoadSubscriptions();
		}

		private async void LoadSubscriptions()
		{
			List<SubscriptionItemBase> subscriptionItems = null;

			IsBusy = true;

			Exception error = null;
			try
			{
				IsOffline = false;

				subscriptionItems = await _subscriptionsManager.LoadSubscriptionsAsync();

				var readAllItem = subscriptionItems.FirstOrDefault(t => t.Id == SpecialTags.Read);
				if (readAllItem != null)
				{
					_tileManager.UpdateAsync(readAllItem.UnreadCount);
				}

				await _localStorageManager.SaveSubscriptionsAsync(subscriptionItems);
			}
			catch (AuthenticationApiException)
			{
				_signInManager.SignOut();
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

			if (subscriptionItems == null)
			{
				IsOffline = true;

				IsBusy = true;
				subscriptionItems = await _localStorageManager.LoadSubscriptionsAsync();
				IsBusy = false;
			}

			if (subscriptionItems != null)
			{
				_rootItems = subscriptionItems;

				var cat = subscriptionItems.OfType<CategoryItem>()
					.FirstOrDefault(c => !_isRoot && String.Equals(c.Id, _categoryId, StringComparison.OrdinalIgnoreCase));

				if (cat != null)
				{
					SubscriptionsHeader = cat.Title;
					_isRoot = false;
					TreeItems = new List<SubscriptionItemBase>(cat.Subscriptions);
				}
				else
				{
					SubscriptionsHeader = Strings.Resources.SubscriptionsSectionHeader;
					_isRoot = true;
					TreeItems = _rootItems;
				}
			}

			if (error != null)
			{
				var msgbox = new MessageDialog(error.Message, Strings.Resources.ErrorDialogTitle);
				await msgbox.ShowAsync();
			}
		}

		private void OnItemClick(object args)
		{
			var clickEventArgs = (ItemClickEventArgs)args;

			var categoryItem = clickEventArgs.ClickedItem as CategoryItem;
			if (categoryItem != null)
			{
				SubscriptionsHeader = categoryItem.Title;
				TreeItems = new List<SubscriptionItemBase>(categoryItem.Subscriptions);
				_isRoot = false;
				_categoryId = categoryItem.Id;
			}
			else
			{
				var subscriptionItem = clickEventArgs.ClickedItem as SubscriptionItem;
				if (subscriptionItem != null)
				{
					var pageToken = _settingsService.StreamView == StreamView.ExpandedView ? PageTokens.ExpandedStream : PageTokens.ListStream;
					var navParam = new StreamPageNavigationParameter
					{
						StreamId = subscriptionItem.Id,
						Title = subscriptionItem.PageTitle
					};
					_navigationService.Navigate(pageToken, navParam.ToJson());
				}
			}
		}

		private void OnRefresh()
		{
			_telemetryManager.TrackEvent(TelemetryEvents.ManualRefreshSubscriptions);
			LoadSubscriptions();
		}

		private async void OnMarkAllAsRead()
		{
			if (TreeItems == null)
				return;

			var item = TreeItems.FirstOrDefault();
			if (item == null)
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

				var timestamp = GetUnixTimeStamp();
				await _streamManager.MarkAllAsReadAsync(item.Id, timestamp);

				LoadSubscriptions();
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

		private int GetUnixTimeStamp()
		{
			var epochDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
			return (int)(DateTimeOffset.UtcNow - epochDate).TotalSeconds;
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			// The base implementation uses RestorableStateAttribute and Reflection to save and restore state
			// If you do not use this attribute, do not invoke base impkementation to prevent execution this useless code.

			LoadState(viewModelState);
			LoadSubscriptions();		
		}

		public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
		{
			// The base implementation uses RestorableStateAttribute and Reflection to save and restore state
			// If you do not use this attribute, do not invoke base impkementation to prevent execution this useless code.

			if (!suspending)
			{
				Application.Current.Resuming -= Application_Resuming;
				_networkManager.NetworkChanged -= _networkManager_NetworkChanged;
			}

			if (viewModelState != null)
				SaveState(viewModelState);		
		}

		private void SaveState(Dictionary<string, object> viewModelState)
		{
			viewModelState["SubscriptionsHeader"] = SubscriptionsHeader;
			viewModelState["RootItems"] = _rootItems;
			viewModelState["IsRoot"] = _isRoot;
			viewModelState["TreeItems"] = TreeItems;
			viewModelState["CategoryId"] = _categoryId;
		}

		private bool LoadState(Dictionary<string, object> viewModelState)
		{
			if (viewModelState == null)
				return false;

			SubscriptionsHeader = viewModelState.GetValue<string>("SubscriptionsHeader");
			_rootItems = viewModelState.GetValue<List<SubscriptionItemBase>>("RootItems");
			_isRoot = viewModelState.GetValue<bool>("IsRoot");
			TreeItems = viewModelState.GetValue<List<SubscriptionItemBase>>("TreeItems");
			_categoryId = viewModelState.GetValue<string>("CategoryId");

			return _rootItems != null && TreeItems != null;
		}

		public bool NavigateBack()
		{
			if (!_isRoot)
			{
				SubscriptionsHeader = Strings.Resources.SubscriptionsSectionHeader;
				TreeItems = _rootItems;
				_isRoot = true;
				return false;
			}

			return true;
		}

		private void OnSettingsPage()
		{
			_navigationService.Navigate(PageTokens.Settings, null);
		}

		private void OnAboutPage()
		{
			_navigationService.Navigate(PageTokens.About, null);
		}

		private void OnSignOut()
		{
			_signInManager.SignOut();
			_navigationService.Navigate(PageTokens.SignIn, null);			
		}

		private void OnStars()
		{
			var pageToken = _settingsService.StreamView == StreamView.ExpandedView ? PageTokens.ExpandedStream : PageTokens.ListStream;
			var navParam = new StreamPageNavigationParameter
			{
				StreamId = SpecialTags.Starred,
				Title = Strings.Resources.StartPageHeader
			};
			_navigationService.Navigate(pageToken, navParam.ToJson());
		}
		
		private void OnSaved()
		{
			_navigationService.Navigate(PageTokens.Saved, null);
		}
	}
}