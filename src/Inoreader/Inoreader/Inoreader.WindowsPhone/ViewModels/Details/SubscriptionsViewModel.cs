using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Inoreader.Annotations;
using Inoreader.Api;
using Inoreader.Api.Exceptions;
using Inoreader.Api.Models;
using Inoreader.Models;
using Inoreader.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using NotificationsExtensions.BadgeContent;

namespace Inoreader.ViewModels.Details
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class SubscriptionsViewModel : BindableBase
	{
		private const string ReadAllIconUrl = "ms-appx:///Assets/ReadAll.png";

		#region Fields

		private static readonly Regex CategoryRegex = new Regex("^user/[0-9]*/label/", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		private readonly INavigationService _navigationService;
		private readonly ApiClient _apiClient;
		private readonly TelemetryClient _telemetryClient;
		private readonly AppSettingsService _settingsService;
		private readonly CacheManager _cacheManager;

		private bool _isBusy;
		private bool _isOffline;
		private List<TreeItemBase> _treeItems;
		private List<TreeItemBase> _rootItems;
		private bool _isRoot = true;
		private string _categoryId;
		private string _subscriptionsHeader = Strings.Resources.SubscriptionsSectionHeader;

		private ICommand _itemClickCommand;
		private ICommand _refreshCommand;

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

		#endregion

		public SubscriptionsViewModel([NotNull] INavigationService navigationService,
			[NotNull] ApiClient apiClient,
			[NotNull] TelemetryClient telemetryClient,
			[NotNull] AppSettingsService settingsService,
			[NotNull] CacheManager cacheManager)
		{
			if (navigationService == null) throw new ArgumentNullException("navigationService");
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			if (settingsService == null) throw new ArgumentNullException("settingsService");
			if (cacheManager == null) throw new ArgumentNullException("cacheManager");

			_navigationService = navigationService;
			_apiClient = apiClient;
			_telemetryClient = telemetryClient;
			_settingsService = settingsService;
			_cacheManager = cacheManager;

			Application.Current.Resuming += Application_Resuming;
		}

		void Application_Resuming(object sender, object e)
		{
			LoadSubscriptions();
		}

		public async void LoadSubscriptions()
		{
			IsBusy = true;

			Exception error = null;
			try
			{
				IsOffline = false;
				await LoadSubscriptionsInternalAsync();
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

			if (error == null) return;

			IsOffline = true;

			IsBusy = true;
			var cacheData = await _cacheManager.LoadSubscriptionsAsync();
			IsBusy = false;

			if (cacheData != null)
			{
				_rootItems = cacheData;

				var cat = cacheData.OfType<CategoryItem>()
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

			MessageDialog msgbox = new MessageDialog(error.Message, Strings.Resources.ErrorDialogTitle);
			await msgbox.ShowAsync();
		}

		private async Task LoadSubscriptionsInternalAsync()
		{
			var stopwatch = Stopwatch.StartNew();

			var tags = await _apiClient.GetTagsAsync();
			var subscriptions = await _apiClient.GetSubscriptionsAsync();
			var unreadCount = await _apiClient.GetUnreadCountAsync();

			stopwatch.Stop();
			_telemetryClient.TrackMetric(TemetryMetrics.GetSubscriptionsTotalResponseTime, stopwatch.Elapsed.TotalSeconds);

			var unreadCountDictionary = new Dictionary<string, int>();
			foreach (var unreadcount in unreadCount.UnreadCounts)
			{
				unreadCountDictionary[unreadcount.Id] = unreadcount.Count;
			}

			var catsQuery = from tag in tags.Tags
							where CategoryRegex.IsMatch(tag.Id)
							select new CategoryItem
							{
								Id = tag.Id,
								SortId = tag.SortId,
								UnreadCount = GetUnreadCount(unreadCountDictionary, tag.Id)
							};

			var categories = catsQuery.ToList();

			foreach (var categoryItem in categories)
			{
				var subsQuery = from s in subscriptions.Subscriptions
								where s.Categories != null
									  && s.Categories.Any(c => String.Equals(c.Id, categoryItem.Id, StringComparison.OrdinalIgnoreCase))
								orderby s.Title// descending 
								select CreateSubscriptionItem(s, unreadCountDictionary);

				categoryItem.Subscriptions = new List<SubscriptionItem>(subsQuery);
				categoryItem.Title = (from s in subscriptions.Subscriptions
									  from c in s.Categories
									  where String.Equals(c.Id, categoryItem.Id, StringComparison.OrdinalIgnoreCase)
									  select c.Label).FirstOrDefault();

				var readAllItem = new SubscriptionItem
				{
					Id = categoryItem.Id,
					SortId = categoryItem.SortId,
					IconUrl = ReadAllIconUrl,
					Title = Strings.Resources.ReadAllSubscriptionItem,
					UnreadCount = categoryItem.UnreadCount
				};

				categoryItem.Subscriptions.Insert(0, readAllItem);
			}

			// hide empty groups
			categories.RemoveAll(c => c.Subscriptions.Count == 0);

			var singleItems = (from s in subscriptions.Subscriptions
							   where s.Categories == null || s.Categories.Length == 0
							   orderby s.Title
							   select CreateSubscriptionItem(s, unreadCountDictionary)).ToList();

			var allItems = new List<TreeItemBase>(categories.OrderBy(c => c.Title));
			allItems.AddRange(singleItems);

			var readAllRootItem = new SubscriptionItem
			{
				Id = String.Empty,
				IconUrl = ReadAllIconUrl,
				Title = Strings.Resources.ReadAllSubscriptionItem,
				UnreadCount = allItems.Sum(s => s.UnreadCount)
			};
			allItems.Insert(0, readAllRootItem);

			if (_settingsService.HideEmptySubscriptions)
			{
				HideEmpty(allItems);
			}

			_rootItems = allItems;


			var cat = _rootItems.OfType<CategoryItem>()
				.FirstOrDefault(c => !_isRoot && String.Equals(c.Id, _categoryId, StringComparison.OrdinalIgnoreCase));

			if (cat != null)
			{
				SubscriptionsHeader = cat.Title;
				TreeItems = new List<TreeItemBase>(cat.Subscriptions);
				_isRoot = false;
			}
			else
			{
				SubscriptionsHeader = Strings.Resources.SubscriptionsSectionHeader;
				TreeItems = _rootItems;
				_isRoot = true;
			}

			UpdateBadge(unreadCount);

			await _cacheManager.SaveSubscriptionsAsync(_rootItems);
		}

		private void UpdateBadge(UnreadCountResponse unreadCount)
		{
			var unreadAllItem = unreadCount.UnreadCounts.FirstOrDefault(uc => uc.Id.EndsWith("/state/com.google/reading-list", StringComparison.OrdinalIgnoreCase));
			int totalUnreadCount = unreadAllItem != null ? unreadAllItem.Count : TreeItems.Sum(ti => ti.UnreadCount);

			BadgeNumericNotificationContent badgeContent = new BadgeNumericNotificationContent((uint)totalUnreadCount);
			BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badgeContent.CreateNotification());
		}

		private void HideEmpty(List<TreeItemBase> allItems)
		{
			allItems.RemoveAll(c => c.UnreadCount == 0);
			foreach (var cat in allItems.OfType<CategoryItem>())
			{
				cat.Subscriptions.RemoveAll(c => c.UnreadCount == 0);
			}
		}

		private static SubscriptionItem CreateSubscriptionItem(Subscription s, Dictionary<string, int> unreadCountDictionary)
		{
			return new SubscriptionItem
			{
				Id = s.Id,
				SortId = s.SortId,
				Url = s.Url,
				HtmlUrl = s.HtmlUrl,
				IconUrl = s.IconUrl,
				Title = s.Title,
				FirstItemMsec = s.FirstItemMsec,
				UnreadCount = GetUnreadCount(unreadCountDictionary, s.Id)
			};
		}

		private static int GetUnreadCount(Dictionary<string, int> unreadCounts, string id)
		{
			int count;
			return unreadCounts.TryGetValue(id, out count) ? count : 0;
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
					_navigationService.Navigate(PageTokens.Stream, subscriptionItem.Id);
			}
		}

		private void OnRefresh()
		{
			_telemetryClient.TrackEvent(TelemetryEvents.ManualRefreshSubscriptions);
			LoadSubscriptions();
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
			if (!LoadState(viewModelState))
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

			LoadSubscriptions();			

			return _rootItems != null && TreeItems != null;
		}
	}
}