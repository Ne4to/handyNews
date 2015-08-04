using System;
using handyNews.UWP.Model;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using Inoreader.Domain.Models;
using Inoreader.Domain.Services.Interfaces;

namespace handyNews.UWP.ViewModels.Controls
{
    public class StreamViewViewModel : BindableBase, IStreamViewViewModel
    {
        #region Fields

        private readonly ISettingsManager _settingsManager;
        private readonly IStreamManager _streamManager;
        private readonly ITelemetryManager _telemetryManager;

        private StreamItemCollection _items;

        #endregion

        #region Properties

        public StreamItemCollection Items
        {
            get { return _items; }
            private set { SetProperty(ref _items, value); }
        }

        #endregion


        public StreamViewViewModel(ISettingsManager settingsManager, IStreamManager streamManager, ITelemetryManager telemetryManager)
        {
            if (settingsManager == null) throw new ArgumentNullException(nameof(settingsManager));
            if (streamManager == null) throw new ArgumentNullException(nameof(streamManager));
            if (telemetryManager == null) throw new ArgumentNullException(nameof(telemetryManager));
            
            _settingsManager = settingsManager;
            _streamManager = streamManager;
            _telemetryManager = telemetryManager;
        }

        public async void UpdateItems(string streamId)
        {
            var streamItems = new StreamItemCollection(_streamManager, streamId, _settingsManager.ShowNewestFirst,
                _telemetryManager, false, _settingsManager.PreloadItemCount);
            await streamItems.InitAsync();
            Items = streamItems;
        }
    }
}