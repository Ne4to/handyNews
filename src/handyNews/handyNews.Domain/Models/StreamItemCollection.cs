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
using JetBrains.Annotations;

namespace handyNews.Domain.Models
{
    public class StreamItemCollection : List<StreamItem>, ISupportIncrementalLoading, INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        private readonly int _preloadItemsCount;
        private readonly bool _showNewestFirst;

        private readonly IStreamManager _streamManager;
        private readonly ITelemetryManager _telemetryManager;
        private readonly bool _allArticles;
        private string _continuation;
        private bool _fault;

        private bool _isBusy;

        public StreamItemCollection(IStreamManager streamManager, string streamId, bool showNewestFirst,
            ITelemetryManager telemetryManager, bool allArticles, int preloadItemsCount)
            : base(preloadItemsCount)
        {
            if (streamManager == null) throw new ArgumentNullException(nameof(streamManager));
            if (streamId == null) throw new ArgumentNullException(nameof(streamId));
            if (telemetryManager == null) throw new ArgumentNullException(nameof(telemetryManager));

            _streamManager = streamManager;
            StreamId = streamId;
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

            StreamId = state.StreamId;
            _showNewestFirst = state.ShowNewestFirst;
            _continuation = state.Continuation;
            StreamTimestamp = state.StreamTimestamp;
            _fault = state.Fault;
            AddRange(state.Items);
            _preloadItemsCount = preloadItemsCount;
        }

        public string StreamId { get; }

        public int StreamTimestamp { get; private set; }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        #region INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        public event EventHandler LoadMoreItemsError;

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
                    StreamId = StreamId,
                    IncludeRead = _allArticles
                };

                var result = await _streamManager.GetItemsAsync(options);
                items = result.Items;
                _continuation = result.Continuation;
                StreamTimestamp = result.Timestamp;
            }
            finally
            {
                IsBusy = false;
            }

            Add(new HeaderSpaceStreamItem());
            AddRange(items);
            Add(new EmptySpaceStreamItem());
            OnPropertyChanged(nameof(Count));
        }

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {
                StreamItem[] items;
                IsBusy = true;

                try
                {
                    var options = new GetItemsOptions
                    {
                        Count = (int)count,
                        Continuation = _continuation,
                        IncludeRead = _allArticles,
                        ShowNewestFirst = _showNewestFirst,
                        StreamId = StreamId
                    };

                    var result = await _streamManager.GetItemsAsync(options);
                    items = result.Items;
                    _continuation = result.Continuation;
                    StreamTimestamp = result.Timestamp;
                }
                finally
                {
                    IsBusy = false;
                }

                var baseIndex = Count - 1;

                InsertRange(Count - 1, items);
                OnPropertyChanged(nameof(Count));

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

        private void NotifyOfInsertedItems(int baseIndex, int count)
        {
            if (CollectionChanged == null)
            {
                return;
            }

            for (var i = 0; i < count; i++)
            {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this[baseIndex],
                    baseIndex);
                CollectionChanged(this, args);
            }
        }

        public StreamItemCollectionState GetSate()
        {
            var state = new StreamItemCollectionState();

            state.StreamId = StreamId;
            state.Continuation = _continuation;
            state.Items = ToArray();
            state.ShowNewestFirst = _showNewestFirst;
            state.StreamTimestamp = StreamTimestamp;
            state.Fault = _fault;

            return state;
        }

        #region ISupportIncrementalLoading

        public bool HasMoreItems
        {
            get { return /*!_initCompleted ||*/ !string.IsNullOrEmpty(_continuation) && !_fault; }
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}