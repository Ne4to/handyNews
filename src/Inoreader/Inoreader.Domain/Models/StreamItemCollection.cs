using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using handyNews.Domain.Models.States;
using handyNews.Domain.Services.Interfaces;
using Inoreader.Annotations;

namespace handyNews.Domain.Models
{
	public class StreamItemCollection : List<StreamItem>, ISupportIncrementalLoading, INotifyCollectionChanged, INotifyPropertyChanged
	{
	    private readonly ITelemetryManager _telemetryManager;
	    private readonly IStreamManager _streamManager;

	    private readonly string _streamId;
	    private readonly bool _showNewestFirst;
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

	    bool _isBusy;
	    private bool _allArticles;
	    private readonly int _preloadItemsCount;

	    public bool IsBusy
	    {
	        get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
	    }

		public event EventHandler LoadMoreItemsError;

		public StreamItemCollection(IStreamManager streamManager, string streamId, bool showNewestFirst, ITelemetryManager telemetryManager, bool allArticles, int preloadItemsCount)
			: base(preloadItemsCount)
		{
		    if (streamManager == null) throw new ArgumentNullException(nameof(streamManager));
		    if (streamId == null) throw new ArgumentNullException(nameof(streamId));
		    if (telemetryManager == null) throw new ArgumentNullException(nameof(telemetryManager));

		    _streamManager = streamManager;
		    _streamId = streamId;
			_telemetryManager = telemetryManager;
			_showNewestFirst = showNewestFirst;
			_allArticles = allArticles;
			_preloadItemsCount = preloadItemsCount;
		}

		public StreamItemCollection([NotNull] StreamItemCollectionState state,
			[NotNull] IStreamManager streamManager,
			[NotNull] ITelemetryManager telemetryManager,
			int preloadItemsCount)
			: base(state.Items.Length)
		{
		    if (state == null) throw new ArgumentNullException(nameof(state));
		    if (streamManager == null) throw new ArgumentNullException(nameof(streamManager));
		    if (telemetryManager == null) throw new ArgumentNullException(nameof(telemetryManager));

		    _streamManager = streamManager;
		    _telemetryManager = telemetryManager;
			
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
            StreamItem[] items;

            IsBusy = true;

            try
            {
                var options = new GetItemsOptions
                {
                    Count = _preloadItemsCount,
                    Continuation = null,
                    ShowNewestFirst = _showNewestFirst,
                    StreamId = _streamId,
                    IncludeRead = _allArticles
                };

		        var result = await _streamManager.GetItemsAsync(options);
		        items = result.Items;
		        _continuation = result.Continuation;
                _streamTimestamp = result.Timestamp;
            }
		    finally
		    {
		        IsBusy = false;
		    }
            
			Add(new HeaderSpaceStreamItem());
			AddRange(items);
			Add(new EmptySpaceStreamItem());
			OnPropertyChanged("Count");
		}

	    #region ISupportIncrementalLoading

		public bool HasMoreItems
		{
			get { return /*!_initCompleted ||*/ (!String.IsNullOrEmpty(_continuation) && !_fault); }
		}

		public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
		{
			if (IsBusy)
			{
                //return AsyncInfo.Run(c => Task.FromResult(new LoadMoreItemsResult()));
                throw new InvalidOperationException("Only one operation in flight at a time");
            }

            IsBusy = true;

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
                StreamItem[] items;
                IsBusy = true;

                try
                {
                    var options = new GetItemsOptions
                    {
                        Count = (int) count,
                        Continuation = _continuation,
                        IncludeRead = _allArticles,
                        ShowNewestFirst = _showNewestFirst,
                        StreamId = _streamId
                    };

			        var result = await _streamManager.GetItemsAsync(options);
			        items = result.Items;
			        _continuation = result.Continuation;
			        _streamTimestamp = result.Timestamp;
			    }
			    finally
			    {
			        IsBusy = false;
			    }

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
                IsBusy = false;
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