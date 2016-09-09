using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using handyNews.Inoreader.Models;
using Newtonsoft.Json.Linq;

namespace handyNews.Inoreader
{
    public class InoreaderClient
    {
        private readonly HttpClient _httpClient;

        public InoreaderClient(DelegatingHandler authorizationHandler)
        {
            if (authorizationHandler == null)
            {
                throw new ArgumentNullException(nameof(authorizationHandler));
            }

            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            authorizationHandler.InnerHandler = httpClientHandler;

            _httpClient = new HttpClient(authorizationHandler);
            _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
        }

        public Task<UserInfoResponse> GetUserInfoAsync()
        {
            return GetAsync<UserInfoResponse>(@"https://www.inoreader.com/reader/api/0/user-info");
        }

        public Task<TagsResponse> GetTagsAsync()
        {
            return GetAsync<TagsResponse>(@"https://www.inoreader.com/reader/api/0/tag/list");
        }

        public Task<SubscriptionsResponse> GetSubscriptionsAsync()
        {
            return GetAsync<SubscriptionsResponse>(@"https://www.inoreader.com/reader/api/0/subscription/list");
        }

        public Task<UnreadCountResponse> GetUnreadCountAsync()
        {
            return GetAsync<UnreadCountResponse>(@"https://www.inoreader.com/reader/api/0/unread-count?output=json");
        }

        public Task<StreamResponse> GetStreamAsync(string id, bool showNewestFirst = true, int count = 20,
            string continuation = null, bool includeRead = false)
        {
            var uri = string.Format(
                "https://www.inoreader.com/reader/api/0/stream/contents/{0}?n={1}&output=json&r={2}",
                WebUtility.UrlEncode(id), count, showNewestFirst ? "n" : "o");

            if (!includeRead)
            {
                uri += "&xt=user/-/state/com.google/read";
            }

            if (continuation != null)
            {
                uri += "&c=" + continuation;
            }

            return GetAsync<StreamResponse>(uri);
        }

        public Task AddTagAsync(string tag, string itemId)
        {
            var uri = string.Format("https://www.inoreader.com/reader/api/0/edit-tag?a={0}&i={1}",
                WebUtility.UrlEncode(tag), WebUtility.UrlEncode(itemId));
            return GetNoResultAsync(uri);
        }

        public Task RemoveTagAsync(string tag, string itemId)
        {
            var uri = string.Format("https://www.inoreader.com/reader/api/0/edit-tag?r={0}&i={1}",
                WebUtility.UrlEncode(tag), WebUtility.UrlEncode(itemId));
            return GetNoResultAsync(uri);
        }

        public Task MarkAllAsReadAsync(string streamId, int streamTimestamp)
        {
            var uri = string.Format("https://www.inoreader.com/reader/api/0/mark-all-as-read?s={0}&ts={1}",
                WebUtility.UrlEncode(streamId), streamTimestamp);
            return GetNoResultAsync(uri);
        }

        private async Task<T> GetAsync<T>(string requestUri)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await _httpClient.SendAsync(requestMessage)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            return JObject.Parse(responseString)
                .ToObject<T>();
        }

        private async Task GetNoResultAsync(string requestUri)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await _httpClient.SendAsync(requestMessage)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }
    }
}