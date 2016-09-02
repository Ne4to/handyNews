using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using handyNews.API;
using handyNews.API.Models;
using handyNews.Domain.Models;
using handyNews.Domain.Services.Interfaces;
using handyNews.Domain.Strings;
using handyNews.Domain.Utils;

namespace handyNews.Domain.Services
{
    public class SubscriptionsManager : ISubscriptionsManager
    {
        private const string READ_ALL_ICON_URL = "ms-appx:///Assets/ReadAll.png";
        private const string CATEGORY_ALL_ICON_URL = "ms-appx:///Assets/CategoryIcon.png";

        private static readonly Regex CategoryRegex = new Regex("^user/[0-9]*/label/",
                                                                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private readonly ApiClient _apiClient;
        private readonly ISettingsManager _settingsService;
        private readonly ITelemetryManager _telemetryManager;

        public SubscriptionsManager(ApiClient apiClient, ITelemetryManager telemetryManager,
                                    ISettingsManager settingsService)
        {
            if (apiClient == null)
            {
                throw new ArgumentNullException(nameof(apiClient));
            }
            if (telemetryManager == null)
            {
                throw new ArgumentNullException(nameof(telemetryManager));
            }
            if (settingsService == null)
            {
                throw new ArgumentNullException(nameof(settingsService));
            }

            _apiClient = apiClient;
            _telemetryManager = telemetryManager;
            _settingsService = settingsService;
        }

        public async Task<List<SubscriptionItemBase>> LoadSubscriptionsAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            var tags = await _apiClient.GetTagsAsync(); //.ConfigureAwait(false);
            var subscriptions = await _apiClient.GetSubscriptionsAsync(); //.ConfigureAwait(false);
            var unreadCount = await _apiClient.GetUnreadCountAsync(); //.ConfigureAwait(false);

            stopwatch.Stop();
            _telemetryManager.TrackMetric(TemetryMetrics.GetSubscriptionsTotalResponseTime,
                                          stopwatch.Elapsed.TotalSeconds);

            var unreadCountDictionary = new Dictionary<string, int>();
            foreach (var unreadcount in unreadCount.UnreadCounts)
                unreadCountDictionary[unreadcount.Id] = unreadcount.Count;

            var catsQuery = from tag in tags.Tags
                where CategoryRegex.IsMatch(tag.Id)
                select new CategoryItem
                       {
                           Id = tag.Id,
                           SortId = tag.SortId,
                           IconUrl = CATEGORY_ALL_ICON_URL
                       };

            var categories = catsQuery.ToList();

            foreach (var categoryItem in categories)
            {
                var subsQuery = from s in subscriptions.Subscriptions
                    where (s.Categories != null)
                          && s.Categories.Any(c => c.Id.EqualsOrdinalIgnoreCase(categoryItem.Id))
                    orderby s.Title
                    // descending 
                    select CreateSubscriptionItem(s, unreadCountDictionary, unreadCount.Max);

                categoryItem.Subscriptions = new List<SubscriptionItem>(subsQuery);

                categoryItem.Title = (from s in subscriptions.Subscriptions
                    from c in s.Categories
                    where c.Id.EqualsOrdinalIgnoreCase(categoryItem.Id)
                    select c.Label.ConvertHtmlToText()).FirstOrDefault();

                categoryItem.UnreadCount = categoryItem.Subscriptions.Sum(t => t.UnreadCount);
                categoryItem.IsMaxUnread = categoryItem.Subscriptions.Any(t => t.IsMaxUnread);

                var readAllItem = new SubscriptionItem
                                  {
                                      Id = categoryItem.Id,
                                      SortId = categoryItem.SortId,
                                      IconUrl = READ_ALL_ICON_URL,
                                      Title = Resources.ReadAllSubscriptionItem,
                                      PageTitle = categoryItem.Title,
                                      UnreadCount = categoryItem.UnreadCount,
                                      IsMaxUnread = categoryItem.IsMaxUnread
                                  };

                categoryItem.Subscriptions.Insert(0, readAllItem);
            }

            // hide empty groups
            categories.RemoveAll(c => c.Subscriptions.Count == 0);

            var singleItems = (from s in subscriptions.Subscriptions
                where (s.Categories == null) || (s.Categories.Length == 0)
                orderby s.Title
                select CreateSubscriptionItem(s, unreadCountDictionary, unreadCount.Max)).ToList();

            var allItems = new List<SubscriptionItemBase>(categories.OrderBy(c => c.Title));
            allItems.AddRange(singleItems);

            var totalUnreadCount = allItems.Sum(t => t.UnreadCount);
            var isTotalMax = allItems.Any(t => t.IsMaxUnread);
            var readAllRootItem = new SubscriptionItem
                                  {
                                      Id = SpecialTags.Read,
                                      IconUrl = READ_ALL_ICON_URL,
                                      Title = Resources.ReadAllSubscriptionItem,
                                      PageTitle = Resources.ReadAllSubscriptionItem,
                                      UnreadCount = totalUnreadCount,
                                      IsMaxUnread = isTotalMax
                                  };
            allItems.Insert(0, readAllRootItem);

            if (_settingsService.HideEmptySubscriptions)
            {
                HideEmpty(allItems);
            }

            return allItems;
        }

        private static SubscriptionItem CreateSubscriptionItem(Subscription s,
                                                               Dictionary<string, int> unreadCountDictionary, int maxUnread)
        {
            var unreadCount = GetUnreadCount(unreadCountDictionary, s.Id);

            return new SubscriptionItem
                   {
                       Id = s.Id,
                       SortId = s.SortId,
                       Url = s.Url,
                       HtmlUrl = s.HtmlUrl,
                       IconUrl = s.IconUrl,
                       Title = s.Title.ConvertHtmlToText(),
                       PageTitle = s.Title.ConvertHtmlToText(),
                       FirstItemMsec = s.FirstItemMsec,
                       UnreadCount = unreadCount,
                       IsMaxUnread = unreadCount == maxUnread
                   };
        }

        private static int GetUnreadCount(Dictionary<string, int> unreadCounts, string id)
        {
            int count;
            return unreadCounts.TryGetValue(id, out count) ? count : 0;
        }

        private void HideEmpty(List<SubscriptionItemBase> allItems)
        {
            allItems.RemoveAll(c => c.UnreadCount == 0);
            foreach (var cat in allItems.OfType<CategoryItem>())
                cat.Subscriptions.RemoveAll(c => c.UnreadCount == 0);
        }
    }
}