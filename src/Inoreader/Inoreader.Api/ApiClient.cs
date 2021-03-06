﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Inoreader.Api.Exceptions;
using Inoreader.Api.Models;
using Newtonsoft.Json.Linq;

namespace Inoreader.Api
{
	public class ApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly ApiSessionStore _sessionStore;

		public bool SignInRequired
		{
			get { return String.IsNullOrEmpty(_sessionStore.Auth); }
		}

		public ApiClient(string appId, string appKey)
		{
			var httpClientHandler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
			_httpClient = new HttpClient(httpClientHandler);
			_httpClient.DefaultRequestHeaders.Add("AppId", appId);
			_httpClient.DefaultRequestHeaders.Add("AppKey", appKey);
			_httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue()
			{
				NoCache = true
			};
			_sessionStore = new ApiSessionStore();
		}

		public void ClearSession()
		{
			_sessionStore.Clear();
			_sessionStore.Save();
		}

		/// <exception cref="AuthenticationApiException"></exception>		
		public async Task<SignInResponse> SignInAsync(string email, string password)
		{
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("Email", email), 
				new KeyValuePair<string, string>("Passwd", password)
			});

			var response = await _httpClient.PostAsync(@"https://www.inoreader.com/accounts/ClientLogin", content).ConfigureAwait(false);

			if (response.StatusCode == HttpStatusCode.Unauthorized)
				throw new AuthenticationApiException(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

			response.EnsureSuccessStatusCode();

			var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			var signInResponse = GetSignInResult(responseString);

			_sessionStore.SID = signInResponse.SID;
			_sessionStore.LSID = signInResponse.LSID;
			_sessionStore.Auth = signInResponse.Auth;
			_sessionStore.Save();

			return signInResponse;
		}

		private static SignInResponse GetSignInResult(string responseString)
		{
			SignInResponse response = new SignInResponse();

			var items = responseString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var item in items)
			{
				var delimerPos = item.IndexOf('=');
				if (delimerPos == -1)
					continue;

				var key = item.Substring(0, delimerPos);
				var value = item.Substring(delimerPos + 1);
				if (value == "null")
					value = null;

				switch (key)
				{
					case "SID":
						response.SID = value;
						break;

					case "LSID":
						response.LSID = value;
						break;

					case "Auth":
						response.Auth = value;
						break;

					default:
						continue;
				}
			}

			return response;
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

		public Task<StreamResponse> GetStreamAsync(string id, bool showNewestFirst = true, int count = 20, string continuation = null, bool includeRead = false)
		{
			var uri = String.Format("https://www.inoreader.com/reader/api/0/stream/contents/{0}?n={1}&output=json&r={2}", WebUtility.UrlEncode(id), count, showNewestFirst ? "n" : "o");

			if (!includeRead)
				uri += "&xt=user/-/state/com.google/read";

			if (continuation != null)
				uri += "&c=" + continuation;
			
			return GetAsync<StreamResponse>(uri);
		}

		public Task AddTagAsync(string tag, string itemId)
		{
			var uri = String.Format("https://www.inoreader.com/reader/api/0/edit-tag?a={0}&i={1}",  WebUtility.UrlEncode(tag),  WebUtility.UrlEncode(itemId));
			return GetNoResultAsync(uri);
		}

		public Task RemoveTagAsync(string tag, string itemId)
		{
			var uri = String.Format("https://www.inoreader.com/reader/api/0/edit-tag?r={0}&i={1}",  WebUtility.UrlEncode(tag),  WebUtility.UrlEncode(itemId));
			return GetNoResultAsync(uri);
		}
		
		public Task MarkAllAsReadAsync(string streamId, int streamTimestamp)
		{
			var uri = String.Format("https://www.inoreader.com/reader/api/0/mark-all-as-read?s={0}&ts={1}", WebUtility.UrlEncode(streamId), streamTimestamp);
			return GetNoResultAsync(uri);
		}

		private async Task<T> GetAsync<T>(string requestUri)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

			string auth = _sessionStore.Auth;
			if (!String.IsNullOrEmpty(auth))
			{
				requestMessage.Headers.Authorization = new AuthenticationHeaderValue("GoogleLogin", "auth=" + auth);
			}

			var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

			if (response.StatusCode == HttpStatusCode.Unauthorized)
				throw new AuthenticationApiException(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

			response.EnsureSuccessStatusCode();

			var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			return JObject.Parse(responseString).ToObject<T>();
		}

		private async Task GetNoResultAsync(string requestUri)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

			string auth = _sessionStore.Auth;
			if (!String.IsNullOrEmpty(auth))
			{
				requestMessage.Headers.Authorization = new AuthenticationHeaderValue("GoogleLogin", "auth=" + auth);
			}

			var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

			response.EnsureSuccessStatusCode();
		}
	}
}


#region Google auth

//			POST https://www.inoreader.com/accounts/ClientLogin HTTP/1.1
//User-Agent: curl/7.40.0
//Host: www.inoreader.com
//Accept: */*
//Content-Length: 46
//Content-Type: application/x-www-form-urlencoded

//Email=username@gmail.com&Passwd=password

//throw new NotImplementedException("asdas");
//			var uri =
//new Uri(
//"https://accounts.google.com/o/oauth2/auth?response_type=code&access_type=offline&scope=https://www.googleapis.com/auth/userinfo.profile%20https://www.googleapis.com/auth/userinfo.email&redirect_uri=" +
//"https://www.inoreader.com/api/oauth2callback/"
//				//		Uri.EscapeUriString(WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString())
//+ "&client_id=499570744979.apps.googleusercontent.com");
//			//var res = await WebAuthenticationBroker.AuthenticateSilentlyAsync(uri);

//			WebAuthenticationBroker.AuthenticateAndContinue(uri, WebAuthenticationBroker.GetCurrentApplicationCallbackUri());

#endregion

