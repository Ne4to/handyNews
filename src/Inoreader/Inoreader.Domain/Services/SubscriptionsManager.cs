﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Html;
using Inoreader.Api;
using Inoreader.Api.Models;
using Inoreader.Domain.Models;
using Microsoft.ApplicationInsights;

namespace Inoreader.Domain.Services
{
	public class SubscriptionsManager
	{
		private const string ReadAllIconUrl = "ms-appx:///Assets/ReadAll.png";
		private static readonly Regex CategoryRegex = new Regex("^user/[0-9]*/label/", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		private readonly ApiClient _apiClient;
		private readonly TelemetryClient _telemetryClient;
		private readonly AppSettingsService _settingsService;

		public SubscriptionsManager(ApiClient apiClient, TelemetryClient telemetryClient, AppSettingsService settingsService)
		{
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			if (settingsService == null) throw new ArgumentNullException("settingsService");

			_apiClient = apiClient;
			_telemetryClient = telemetryClient;
			_settingsService = settingsService;
		}

		public async Task<List<TreeItemBase>> LoadSubscriptionsAsync()
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
									  select HtmlUtilities.ConvertToText(c.Label)).FirstOrDefault();

				categoryItem.UnreadCount = categoryItem.Subscriptions.Sum(t => t.UnreadCount);

				var readAllItem = new SubscriptionItem
				{
					Id = categoryItem.Id,
					SortId = categoryItem.SortId,
					IconUrl = ReadAllIconUrl,
					Title = Strings.Resources.ReadAllSubscriptionItem,
					PageTitle = categoryItem.Title,
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

			var totalUnreadCount = allItems.Sum(t => t.UnreadCount);
			var readAllRootItem = new SubscriptionItem
			{
				Id = SpecialTags.Read,
				IconUrl = ReadAllIconUrl,
				Title = Strings.Resources.ReadAllSubscriptionItem,
				PageTitle = Strings.Resources.ReadAllSubscriptionItem,
				UnreadCount = totalUnreadCount
			};
			allItems.Insert(0, readAllRootItem);

			if (_settingsService.HideEmptySubscriptions)
			{
				HideEmpty(allItems);
			}

			return allItems;			
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
				Title = HtmlUtilities.ConvertToText(s.Title),
				PageTitle = HtmlUtilities.ConvertToText(s.Title),
				FirstItemMsec = s.FirstItemMsec,
				UnreadCount = GetUnreadCount(unreadCountDictionary, s.Id)
			};
		}

		private static int GetUnreadCount(Dictionary<string, int> unreadCounts, string id)
		{
			int count;
			return unreadCounts.TryGetValue(id, out count) ? count : 0;
		}

		private void HideEmpty(List<TreeItemBase> allItems)
		{
			allItems.RemoveAll(c => c.UnreadCount == 0);
			foreach (var cat in allItems.OfType<CategoryItem>())
			{
				cat.Subscriptions.RemoveAll(c => c.UnreadCount == 0);
			}
		}
	}
}