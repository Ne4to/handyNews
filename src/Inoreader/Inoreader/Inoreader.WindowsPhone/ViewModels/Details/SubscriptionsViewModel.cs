using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Data.Html;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Inoreader.Annotations;
using Inoreader.Api;
using Inoreader.Api.Exceptions;
using Inoreader.Api.Models;
using Inoreader.Domain.Models;
using Inoreader.Domain.Services;
using Inoreader.ViewModels.Pages;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

namespace Inoreader.ViewModels.Details
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class SubscriptionsViewModel : BindableBase
	{
		#region Fields

		private readonly INavigationService _navigationService;
		private readonly ApiClient _apiClient;
		private readonly TelemetryClient _telemetryClient;
		private readonly AppSettingsService _settingsService;
		private readonly TileManager _tileManager;
		private readonly LocalStorageManager _localStorageManager;
		private readonly SubscriptionsManager _subscriptionsManager;

		private bool _isBusy;
		private bool _isOffline;
		private List<TreeItemBase> _treeItems;
		private List<TreeItemBase> _rootItems;
		private bool _isRoot = true;
		private string _categoryId;
		private string _subscriptionsHeader = Strings.Resources.SubscriptionsSectionHeader;

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

		public List<TreeItemBase> TreeItems
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

		public SubscriptionsViewModel([NotNull] INavigationService navigationService,
			[NotNull] ApiClient apiClient,
			[NotNull] TelemetryClient telemetryClient,
			[NotNull] AppSettingsService settingsService,
			[NotNull] TileManager tileManager, 
			[NotNull] LocalStorageManager localStorageManager,
			[NotNull] SubscriptionsManager subscriptionsManager)
		{
			if (navigationService == null) throw new ArgumentNullException("navigationService");
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			if (settingsService == null) throw new ArgumentNullException("settingsService");
			if (tileManager == null) throw new ArgumentNullException("tileManager");
			if (localStorageManager == null) throw new ArgumentNullException("localStorageManager");
			if (subscriptionsManager == null) throw new ArgumentNullException("subscriptionsManager");

			_navigationService = navigationService;
			_apiClient = apiClient;
			_telemetryClient = telemetryClient;
			_settingsService = settingsService;
			_tileManager = tileManager;
			_localStorageManager = localStorageManager;
			_subscriptionsManager = subscriptionsManager;

			Application.Current.Resuming += Application_Resuming;
		}

		void Application_Resuming(object sender, object e)
		{
			LoadSubscriptions();
		}

		public async void LoadSubscriptions()
		{
			List<TreeItemBase> subscriptionItems = null;
			
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
				_apiClient.ClearSession();
				_navigationService.Navigate(PageTokens.SignIn, null);
				return;
			}
			catch (Exception ex)
			{
				error = ex;
				_telemetryClient.TrackExceptionFull(ex);
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
					TreeItems = new List<TreeItemBase>(cat.Subscriptions);
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
				TreeItems = new List<TreeItemBase>(categoryItem.Subscriptions);
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
			_telemetryClient.TrackEvent(TelemetryEvents.ManualRefreshSubscriptions);
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
				_telemetryClient.TrackEvent(TelemetryEvents.MarkAllAsRead);

				var timestamp = GetUnixTimeStamp();
				await _apiClient.MarkAllAsReadAsync(item.Id, timestamp);

				LoadSubscriptions();
			}
			catch (Exception ex)
			{
				error = ex;
				_telemetryClient.TrackException(ex);
			}

			if (error == null) return;

			MessageDialog msgbox = new MessageDialog(error.Message, Strings.Resources.ErrorDialogTitle);
			await msgbox.ShowAsync();
		}

		public int GetUnixTimeStamp()
		{
			var epochDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
			return (int)(DateTimeOffset.UtcNow - epochDate).TotalSeconds;
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

		public void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			LoadState(viewModelState);
			LoadSubscriptions();
		}

		public void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
		{
			if (!suspending)
				Application.Current.Resuming -= Application_Resuming;

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
			_rootItems = viewModelState.GetValue<List<TreeItemBase>>("RootItems");
			_isRoot = viewModelState.GetValue<bool>("IsRoot");
			TreeItems = viewModelState.GetValue<List<TreeItemBase>>("TreeItems");
			_categoryId = viewModelState.GetValue<string>("CategoryId");

			return _rootItems != null && TreeItems != null;
		}
	}
}