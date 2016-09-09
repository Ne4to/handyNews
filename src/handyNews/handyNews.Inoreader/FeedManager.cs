using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using handyNews.Domain;
using handyNews.Domain.Models;
using handyNews.Domain.Services.Interfaces;
using handyNews.Domain.Strings;
using handyNews.Domain.Utils;
using handyNews.Inoreader.Models;

namespace handyNews.Inoreader
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FeedManager : IFeedManager
    {
        private readonly InoreaderClient _inoreaderClient;
        private readonly ISettingsManager _settingsService;
        private readonly ITelemetryManager _telemetryManager;
        private const string READ_ALL_ICON_URL = "ms-appx:///Assets/ReadAll.png";
        private const string CATEGORY_ALL_ICON_URL = "ms-appx:///Assets/CategoryIcon.png";

        private static readonly Regex CategoryRegex = new Regex("^user/[0-9]*/label/",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public FeedManager(InoreaderClient inoreaderClient, ITelemetryManager telemetryManager,
            ISettingsManager settingsService)
        {
            if (inoreaderClient == null)
            {
                throw new ArgumentNullException(nameof(inoreaderClient));
            }
            if (telemetryManager == null)
            {
                throw new ArgumentNullException(nameof(telemetryManager));
            }
            if (settingsService == null)
            {
                throw new ArgumentNullException(nameof(settingsService));
            }

            _inoreaderClient = inoreaderClient;
            _telemetryManager = telemetryManager;
            _settingsService = settingsService;
        }

        public async Task<IReadOnlyCollection<Feed>> GetFeedsAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            var tags = await _inoreaderClient.GetTagsAsync(); //.ConfigureAwait(false);
            var subscriptions = await _inoreaderClient.GetSubscriptionsAsync(); //.ConfigureAwait(false);
            var unreadCount = await _inoreaderClient.GetUnreadCountAsync(); //.ConfigureAwait(false);

            stopwatch.Stop();
            _telemetryManager.TrackMetric(TemetryMetrics.GetSubscriptionsTotalResponseTime,
                stopwatch.Elapsed.TotalSeconds);

            var unreadCountDictionary = new Dictionary<string, int>();
            foreach (var unreadcount in unreadCount.UnreadCounts)
                unreadCountDictionary[unreadcount.Id] = unreadcount.Count;

            var catsQuery = tags.Tags
                .Where(tag => CategoryRegex.IsMatch(tag.Id))
                .Select(tag => new Feed
                {
                    Id = tag.Id,
                    SortId = tag.SortId,
                    IconUrl = CATEGORY_ALL_ICON_URL
                });

            var categories = catsQuery.ToList();

            foreach (var categoryItem in categories)
            {
                var categoryItemChildren = new List<Feed>();
                var readAllItem = new Feed
                {
                    Id = categoryItem.Id,
                    SortId = categoryItem.SortId,
                    IconUrl = READ_ALL_ICON_URL,
                    Title = Resources.ReadAllSubscriptionItem,
                    PageTitle = categoryItem.Title,
                    UnreadCount = categoryItem.UnreadCount,
                    ApproxUnreadCount = categoryItem.ApproxUnreadCount
                };

                categoryItemChildren.Add(readAllItem);

                var subsQuery = subscriptions.Subscriptions
                    .Where(s => (s.Categories != null) && s.Categories.Any(c => c.Id.EqualsOrdinalIgnoreCase(categoryItem.Id)))
                    .OrderBy(s => s.Title)
                    .Select(s => CreateSubscriptionItem(s, unreadCountDictionary, unreadCount.Max));

                categoryItemChildren.AddRange(subsQuery);
                categoryItem.Children = categoryItemChildren;

                categoryItem.Title = (from s in subscriptions.Subscriptions
                    from c in s.Categories
                    where c.Id.EqualsOrdinalIgnoreCase(categoryItem.Id)
                    select c.Label.ConvertHtmlToText()).FirstOrDefault();

                categoryItem.UnreadCount = categoryItem.Children.Sum(t => t.UnreadCount);
                categoryItem.ApproxUnreadCount = categoryItem.Children.Any(t => t.ApproxUnreadCount);
            }

            // hide empty groups
            categories.RemoveAll(c => c.Children.Count == 0);

            var singleItems = (from s in subscriptions.Subscriptions
                where (s.Categories == null) || (s.Categories.Length == 0)
                orderby s.Title
                select CreateSubscriptionItem(s, unreadCountDictionary, unreadCount.Max)).ToList();

            var allItems = new List<Feed>(categories.OrderBy(c => c.Title));
            allItems.AddRange(singleItems);

            var totalUnreadCount = allItems.Sum(t => t.UnreadCount);
            var isTotalMax = allItems.Any(t => t.ApproxUnreadCount);
            var readAllRootItem = new Feed
            {
                Id = SpecialTags.Read,
                IconUrl = READ_ALL_ICON_URL,
                Title = Resources.ReadAllSubscriptionItem,
                PageTitle = Resources.ReadAllSubscriptionItem,
                UnreadCount = totalUnreadCount,
                ApproxUnreadCount = isTotalMax
            };
            allItems.Insert(0, readAllRootItem);

            if (_settingsService.HideEmptySubscriptions)
            {
                HideEmpty(allItems);
            }

            return allItems;
        }

        private static Feed CreateSubscriptionItem(Subscription s,
            Dictionary<string, int> unreadCountDictionary, int maxUnread)
        {
            var unreadCount = GetUnreadCount(unreadCountDictionary, s.Id);

            return new Feed
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
                ApproxUnreadCount = unreadCount == maxUnread
            };
        }

        private static int GetUnreadCount(Dictionary<string, int> unreadCounts, string id)
        {
            int count;
            return unreadCounts.TryGetValue(id, out count) ? count : 0;
        }

        private void HideEmpty(List<Feed> allItems)
        {
            allItems.RemoveAll(c => c.UnreadCount == 0);
            //foreach (var cat in allItems.OfType<Feed>())
            //    cat.Children.RemoveAll(c => c.UnreadCount == 0);
        }
    }
}