using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Html;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Inoreader.Annotations;
using Inoreader.Api;
using Inoreader.Api.Models;
using Inoreader.Domain.Models.States;
using Inoreader.Domain.Services;
using Inoreader.Domain.Services.Interfaces;


namespace Inoreader.Domain.Models
{
	public class StreamItemCollection : List<StreamItem>, ISupportIncrementalLoading, INotifyCollectionChanged, INotifyPropertyChanged
	{
		private readonly ApiClient _apiClient;
		private readonly string _streamId;
		private readonly bool _showNewestFirst;
		private readonly ITelemetryManager _telemetryManager;
		private readonly Action<bool> _onBusy;
		private string _continuation;
		private int _streamTimestamp;
		private bool _fault;

		public string StreamId
		{
			get { return _streamId; }
		}

		public int StreamTimestamp
		{
			get { return _streamTimestamp; }
		}

		bool _busy;
		private bool _allArticles;
		private readonly int _preloadItemsCount;

		public event EventHandler LoadMoreItemsError;

		public StreamItemCollection(ApiClient apiClient, string streamId, bool showNewestFirst, ITelemetryManager telemetryManager, bool allArticles, Action<bool> onBusy, int preloadItemsCount)
			: base(preloadItemsCount)
		{
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (streamId == null) throw new ArgumentNullException("streamId");
			if (telemetryManager == null) throw new ArgumentNullException("telemetryManager");
			if (onBusy == null) throw new ArgumentNullException("onBusy");

			_apiClient = apiClient;
			_streamId = streamId;
			_telemetryManager = telemetryManager;
			_onBusy = onBusy;
			_showNewestFirst = showNewestFirst;
			_allArticles = allArticles;
			_preloadItemsCount = preloadItemsCount;
		}

		public StreamItemCollection([NotNull] StreamItemCollectionState state,
			[NotNull] ApiClient apiClient,
			[NotNull] ITelemetryManager telemetryManager,
			[NotNull] Action<bool> onBusy,
			int preloadItemsCount)
			: base(state.Items.Length)
		{
			if (state == null) throw new ArgumentNullException("state");
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (telemetryManager == null) throw new ArgumentNullException("telemetryManager");
			if (onBusy == null) throw new ArgumentNullException("onBusy");

			_apiClient = apiClient;
			_telemetryManager = telemetryManager;
			_onBusy = onBusy;

			_streamId = state.StreamId;
			_showNewestFirst = state.ShowNewestFirst;
			_continuation = state.Continuation;
			_streamTimestamp = state.StreamTimestamp;
			_fault = state.Fault;
			AddRange(state.Items);
			_preloadItemsCount = preloadItemsCount;
		}

		public async Task InitAsync()
		{
			var stream = await LoadAsync(_preloadItemsCount, null);
			_continuation = stream.continuation;
			var itemsQuery = GetItems(stream);

			Add(new HeaderSpaceStreamItem());
			AddRange(itemsQuery);
			Add(new EmptySpaceStreamItem());
			OnPropertyChanged("Count");
		}

		private static IEnumerable<StreamItem> GetItems(StreamResponse stream)
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

		private async Task<StreamResponse> LoadAsync(int count, string continuation)
		{
			StreamResponse stream;

			_onBusy(true);
			try
			{
				var stopwatch = Stopwatch.StartNew();

				stream = await _apiClient.GetStreamAsync(_streamId, _showNewestFirst, count, continuation, _allArticles);
				_streamTimestamp = stream.updated;

				stopwatch.Stop();
				_telemetryManager.TrackMetric(TemetryMetrics.GetStreamResponseTime, stopwatch.Elapsed.TotalSeconds);
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
			get { return !String.IsNullOrEmpty(_continuation) && !_fault; }
		}

		public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
		{
			if (_busy)
			{
				throw new InvalidOperationException("Only one operation in flight at a time");
			}

			_busy = true;

			var loadCount = Math.Max(count, (uint)_preloadItemsCount);

			return AsyncInfo.Run(c => LoadMoreItemsAsync(c, loadCount));
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
				OnPropertyChanged("Count");

				// Now notify of the new items
				NotifyOfInsertedItems(baseIndex, items.Length);

				return new LoadMoreItemsResult { Count = (uint)items.Length };
			}
			catch (Exception ex)
			{
				_fault = true;
				_telemetryManager.TrackError(ex);

				if (LoadMoreItemsError != null)
					LoadMoreItemsError(this, EventArgs.Empty);

				return new LoadMoreItemsResult { Count = 0 };
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

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		public StreamItemCollectionState GetSate()
		{
			var state = new StreamItemCollectionState();

			state.StreamId = _streamId;
			state.Continuation = _continuation;
			state.Items = this.ToArray();
			state.ShowNewestFirst = _showNewestFirst;
			state.StreamTimestamp = _streamTimestamp;
			state.Fault = _fault;

			return state;
		}
	}
}