using System;
using handyNews.Domain.Models;
using handyNews.Domain.Services.Interfaces;
using handyNews.UWP.Events;
using handyNews.UWP.Model;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using PubSub;

namespace handyNews.UWP.ViewModels.Controls
{
    public class StreamViewViewModel : BindableBase, IStreamViewViewModel
    {
        public StreamViewViewModel(ISettingsManager settingsManager, IStreamManager streamManager,
                                   ITelemetryManager telemetryManager)
        {
            if (settingsManager == null)
            {
                throw new ArgumentNullException(nameof(settingsManager));
            }
            if (streamManager == null)
            {
                throw new ArgumentNullException(nameof(streamManager));
            }
            if (telemetryManager == null)
            {
                throw new ArgumentNullException(nameof(telemetryManager));
            }

            _settingsManager = settingsManager;
            _streamManager = streamManager;
            _telemetryManager = telemetryManager;
        }

        #region Properties

        public StreamItemCollection Items
        {
            get { return _items; }
            private set { SetProperty(ref _items, value, nameof(Items)); }
        }

        #endregion

        public async void UpdateItems(string streamId)
        {
            var streamItems = new StreamItemCollection(_streamManager, streamId, _settingsManager.ShowNewestFirst,
                                                       _telemetryManager, false, _settingsManager.PreloadItemCount);
            await streamItems.InitAsync();
            Items = streamItems;
        }

        public void OnNavigatedTo()
        {
            this.Subscribe<ShowSubscriptionStreamEvent>(OnShowSubscriptionStreamEvent);
        }

        private void OnShowSubscriptionStreamEvent(ShowSubscriptionStreamEvent eventData)
        {
            UpdateItems(eventData.Item.Id);
        }

        #region Fields

        private readonly ISettingsManager _settingsManager;
        private readonly IStreamManager _streamManager;
        private readonly ITelemetryManager _telemetryManager;

        private StreamItemCollection _items;

        #endregion
    }
}