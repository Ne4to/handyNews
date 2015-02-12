using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Inoreader.Api;
using Inoreader.Api.Models;
using Microsoft.ApplicationInsights;

namespace Inoreader.Models
{
	public class SteamItemCollection : List<SteamItem>, ISupportIncrementalLoading, INotifyCollectionChanged
	{
		private readonly ApiClient _apiClient;
		private readonly string _steamId;
		private readonly TelemetryClient _telemetryClient;
		private readonly Action<bool> _onBusy;
		private string _continuation;

		bool _busy = false;

		public SteamItemCollection(ApiClient apiClient, string steamId, TelemetryClient telemetryClient, Action<bool> onBusy)
			: base(20)
		{
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (steamId == null) throw new ArgumentNullException("steamId");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			if (onBusy == null) throw new ArgumentNullException("onBusy");

			_apiClient = apiClient;
			_steamId = steamId;
			_telemetryClient = telemetryClient;
			_onBusy = onBusy;
		}

		public async Task<string> InitAsync()
		{
			var stream = await LoadAsync(20, null);
			_continuation = stream.continuation;
			var itemsQuery = GetItems(stream);

			AddRange(itemsQuery);
			Add(new EmptySpaceSteamItem());

			return stream.title;
		}

		private static IEnumerable<SteamItem> GetItems(StreamResponse stream)
		{
			var itemsQuery = from it in stream.items
							 select new SteamItem
							 {
								 Id = it.id,
								 Published = UnixTimeStampToDateTime(it.published),
								 Title = it.title,
								 Content = it.summary.content,
								 WebUri = GetWebUri(it)
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

		private async Task<StreamResponse> LoadAsync(int count, string continuation)
		{
			StreamResponse stream;

			_onBusy(true);
			try
			{
				var stopwatch = Stopwatch.StartNew();

				stream = await _apiClient.GetStreamAsync(_steamId, count, continuation);

				stopwatch.Stop();
				_telemetryClient.TrackMetric(TemetryMetrics.GetStreamResponseTime, stopwatch.Elapsed.TotalSeconds);
			}
			finally
			{
				_onBusy(false);
			}

			return stream;
		}

		public static DateTimeOffset UnixTimeStampToDateTime(int unixTimeStamp)
		{
			var epochDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
			return epochDate.AddSeconds(unixTimeStamp);
		}

		#region ISupportIncrementalLoading

		public bool HasMoreItems
		{
			get { return !String.IsNullOrEmpty(_continuation); }
		}

		public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
		{
			if (_busy)
			{
				throw new InvalidOperationException("Only one operation in flight at a time");
			}

			_busy = true;

			return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
		}

		#endregion

		#region INotifyCollectionChanged

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
		{
			try
			{
				var stream = await LoadAsync((int)count, _continuation);
				_continuation = stream.continuation;

				var items = GetItems(stream).ToArray();
				var baseIndex = Count - 1;

				InsertRange(Count - 1, items);

				// Now notify of the new items
				NotifyOfInsertedItems(baseIndex, items.Length);

				return new LoadMoreItemsResult { Count = (uint)items.Length };
			}
			finally
			{
				_busy = false;
			}
		}

		void NotifyOfInsertedItems(int baseIndex, int count)
		{
			if (CollectionChanged == null)
			{
				return;
			}

			for (int i = 0; i < count; i++)
			{
				var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this[baseIndex], baseIndex);
				CollectionChanged(this, args);
			}
		}
	}
}