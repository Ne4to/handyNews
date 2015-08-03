using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Html;
using Inoreader.Api;
using Inoreader.Api.Models;
using Inoreader.Domain.Models;
using Inoreader.Domain.Services.Interfaces;

namespace Inoreader.Domain.Services
{
    public class StreamManager : IStreamManager
    {
        private readonly ApiClient _apiClient;

        public StreamManager(ApiClient apiClient)
        {
            if (apiClient == null) throw new ArgumentNullException(nameof(apiClient));
            _apiClient = apiClient;
        }

        public async Task<GetItemsResult> GetItemsAsync(GetItemsOptions options)
        {
            var stream = await LoadAsync(options);

            return new GetItemsResult
            {
                Items = GetItems(stream).ToArray(),
                Continuation = stream.continuation,
                Timestamp = stream.updated
            };
        }

        private IEnumerable<StreamItem> GetItems(StreamResponse stream)
        {
            var itemsQuery = from it in stream.items
                             select new StreamItem
                             {
                                 Id = it.id,
                                 Published = UnixTimeStampToDateTime(it.published),
                                 Title = HtmlUtilities.ConvertToText(it.title),
                                 Content = it.summary.content,
                                 WebUri = GetWebUri(it),
                                 Starred = it.categories != null
                                           && it.categories.Any(c => c.EndsWith("/state/com.google/starred", StringComparison.OrdinalIgnoreCase)),
                                 Unread = it.categories != null && !it.categories.Any(c => c.EndsWith("/state/com.google/read"))
                             };
            return itemsQuery;
        }

        private static string GetWebUri(Item item)
        {
            if (item.alternate == null)
                return null;

            var q = from a in item.alternate
                    where String.Equals(a.type, "text/html", StringComparison.OrdinalIgnoreCase)
                    select a.href;

            return q.FirstOrDefault();
        }

        private async Task<StreamResponse> LoadAsync(GetItemsOptions options)
        {
            return await _apiClient.GetStreamAsync(options.StreamId, options.ShowNewestFirst, options.Count, options.Continuation, options.IncludeRead);
        }

        private DateTimeOffset UnixTimeStampToDateTime(int unixTimeStamp)
        {
            var epochDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            return epochDate.AddSeconds(unixTimeStamp);
        }
    }
}