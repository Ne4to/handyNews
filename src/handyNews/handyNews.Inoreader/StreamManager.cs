using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using handyNews.Domain.Models;
using handyNews.Domain.Services.Interfaces;
using handyNews.Domain.Utils;
using handyNews.Inoreader.Models;

namespace handyNews.Inoreader
{
    public class StreamManager : IStreamManager
    {
        private readonly InoreaderClient _inoreaderClient;

        public StreamManager(InoreaderClient inoreaderClient)
        {
            if (inoreaderClient == null)
            {
                throw new ArgumentNullException(nameof(inoreaderClient));
            }
            _inoreaderClient = inoreaderClient;
        }

        public async Task<GetItemsResult> GetItemsAsync(GetItemsOptions options)
        {
            var stream = await LoadAsync(options);

            return new GetItemsResult
            {
                Items = GetItems(stream)
                    .ToArray(),
                Continuation = stream.continuation,
                Timestamp = stream.updated
            };
        }

        public Task MarkAllAsReadAsync(string streamId, int streamTimestamp)
        {
            return _inoreaderClient.MarkAllAsReadAsync(streamId, streamTimestamp);
        }

        private IEnumerable<StreamItem> GetItems(StreamResponse stream)
        {
            var itemsQuery = from it in stream.items
                select new StreamItem
                {
                    Id = it.id,
                    Published = UnixTimeStampToDateTime(it.published),
                    Title = it.title.ConvertHtmlToText(),
                    Content = it.summary.content,
                    WebUri = GetWebUri(it),
                    Starred = (it.categories != null)
                        &&
                        it.categories.Any(
                            c => c.EndsWithOrdinalIgnoreCase("/state/com.google/starred")),
                    Unread = (it.categories != null) && !it.categories.Any(c => c.EndsWithOrdinalIgnoreCase("/state/com.google/read"))
                };
            return itemsQuery;
        }

        private static string GetWebUri(Item item)
        {
            if (item.alternate == null)
            {
                return null;
            }

            var q = from a in item.alternate
                where a.type.EqualsOrdinalIgnoreCase("text/html")
                select a.href;

            return q.FirstOrDefault();
        }

        private async Task<StreamResponse> LoadAsync(GetItemsOptions options)
        {
            return
                await
                    _inoreaderClient.GetStreamAsync(options.StreamId, options.ShowNewestFirst, options.Count,
                        options.Continuation, options.IncludeRead);
        }

        private DateTimeOffset UnixTimeStampToDateTime(int unixTimeStamp)
        {
            var epochDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            return epochDate.AddSeconds(unixTimeStamp);
        }
    }
}