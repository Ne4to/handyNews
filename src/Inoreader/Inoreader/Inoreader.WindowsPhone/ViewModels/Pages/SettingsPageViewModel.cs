using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Inoreader.Annotations;
using Inoreader.Domain.Services;
using Inoreader.Domain.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Practices.Prism.Mvvm;
using ReactiveUI;

namespace Inoreader.ViewModels.Pages
{
    public class SettingsPageViewModel : ViewModel
    {
        #region Fields

        private readonly AppSettingsService _settingsService;
        private readonly ITelemetryManager _telemetryManager;
        private readonly LocalStorageManager _localStorageManager;
        private readonly string _initialDisplayCulture;

        private List<Lang> _languages;
        private Lang _selectedLang;
        private bool _hideEmptySubscriptions;
        private List<ShowOrderItem> _showOrderItems;
        private ShowOrderItem _selectedShowOrder;
        private List<StreamViewItem> _streamViewItems;
        private StreamViewItem _selectedStreamView;
        private double _fontSize;
        private bool _textJustification;
        private TextAlignment _textAlignment;
        private int _preloadItemCount;
        private bool _autoMarkAsRead;

        #endregion

        #region Properties

        public List<Lang> Languages
        {
            get { return _languages; }
            set { SetProperty(ref _languages, value); }
        }

        public Lang SelectedLang
        {
            get { return _selectedLang; }
            set
            {
                if (SetProperty(ref _selectedLang, value))
                    SaveLang();
            }
        }

        public bool HideEmptySubscriptions
        {
            get { return _hideEmptySubscriptions; }
            set
            {
                if (SetProperty(ref _hideEmptySubscriptions, value))
                    SaveHideEmptySubscriptions();
            }
        }

        public List<ShowOrderItem> ShowOrderItems
        {
            get { return _showOrderItems; }
            set { SetProperty(ref _showOrderItems, value); }
        }

        public ShowOrderItem SelectedShowOrder
        {
            get { return _selectedShowOrder; }
            set
            {
                if (SetProperty(ref _selectedShowOrder, value))
                    SaveShowOrder();
            }
        }

        public List<StreamViewItem> StreamViewItems
        {
            get { return _streamViewItems; }
            set { SetProperty(ref _streamViewItems, value); }
        }

        public StreamViewItem SelectedStreamView
        {
            get { return _selectedStreamView; }
            set
            {
                if (SetProperty(ref _selectedStreamView, value))
                    SaveStreamView();
            }
        }

        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                if (SetProperty(ref _fontSize, value))
                    SaveFontSize();
            }
        }

        public bool TextJustification
        {
            get { return _textJustification; }
            set
            {
                SetProperty(ref _textJustification, value);
                TextAlignment = TextJustification ? TextAlignment.Justify : TextAlignment.Left;
                SaveTextJustification();
            }
        }

        public TextAlignment TextAlignment
        {
            get { return _textAlignment; }
            set { SetProperty(ref _textAlignment, value); }
        }

        public bool AutoMarkAsRead
        {
            get { return _autoMarkAsRead; }
            set
            {
                if (SetProperty(ref _autoMarkAsRead, value))
                    SaveAutoMarkAsRead();
            }
        }

        public int PreloadItemCount
        {
            get { return _preloadItemCount; }
            set
            {
                if (SetProperty(ref _preloadItemCount, value))
                    SavePreloadItemCount();
            }
        }

        #endregion

        public SettingsPageViewModel([NotNull] AppSettingsService settingsService,
            [NotNull] ITelemetryManager telemetryManager,
            [NotNull] LocalStorageManager localStorageManager)
        {
            if (settingsService == null) throw new ArgumentNullException("settingsService");
            if (telemetryManager == null) throw new ArgumentNullException("telemetryManager");
            if (localStorageManager == null) throw new ArgumentNullException("localStorageManager");

            _settingsService = settingsService;
            _telemetryManager = telemetryManager;
            _localStorageManager = localStorageManager;

            _initialDisplayCulture = _settingsService.DisplayCulture;
        }

        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            // The base implementation uses RestorableStateAttribute and Reflection to save and restore state
            // If you do not use this attribute, do not invoke base impkementation to prevent execution this useless code.

            Languages = new List<Lang>(new[]
                                        {
                                            new Lang(),
                                            new Lang("en-US"),
                                            new Lang("ru-RU"),
                                            new Lang("pt-BR")
                                        });

            SelectedLang = Languages.FirstOrDefault(l => l.Name == _initialDisplayCulture) ?? Languages.FirstOrDefault();
            HideEmptySubscriptions = _settingsService.HideEmptySubscriptions;

            ShowOrderItems = new List<ShowOrderItem>(new[]
            {
                new ShowOrderItem(true),
                new ShowOrderItem(false)
            });

            SelectedShowOrder = ShowOrderItems.Single(s => s.Value == _settingsService.ShowNewestFirst);

            StreamViewItems = new List<StreamViewItem>(new[]
            {
                new StreamViewItem(StreamView.ExpandedView),
                new StreamViewItem(StreamView.ListView)
            });

            SelectedStreamView = StreamViewItems.Single(s => s.View == _settingsService.StreamView);
            FontSize = _settingsService.FontSize;
            TextJustification = _settingsService.TextAlignment == TextAlignment.Justify;
            AutoMarkAsRead = _settingsService.AutoMarkAsRead;
        }

        private void SaveLang()
        {
            if (_settingsService.DisplayCulture == SelectedLang.Name)
                return;

            var properties = new Dictionary<string, string>
            {
                { "OldValue", _settingsService.DisplayCulture },
                { "NewValue", SelectedLang.Name }
            };
            _telemetryManager.TrackEvent(TelemetryEvents.ChangeDisplayCulture, properties);

            _settingsService.DisplayCulture = SelectedLang.Name;
            _settingsService.Save();
        }

        private void SaveHideEmptySubscriptions()
        {
            if (HideEmptySubscriptions == _settingsService.HideEmptySubscriptions)
                return;

            TrackEvent(TelemetryEvents.ChangeHideEmptySubscriptions,
                _settingsService.HideEmptySubscriptions.ToString(),
                HideEmptySubscriptions.ToString());

            _settingsService.HideEmptySubscriptions = HideEmptySubscriptions;
            _settingsService.Save();
        }

        private void SaveStreamView()
        {
            if (SelectedStreamView.View == _settingsService.StreamView)
                return;

            TrackEvent(TelemetryEvents.ChangeStreamView, 
                _settingsService.StreamView.ToString(), 
                SelectedStreamView.View.ToString());

            _settingsService.StreamView = SelectedStreamView.View;
            _settingsService.Save();
        }

        private void SaveShowOrder()
        {
            if (SelectedShowOrder.Value == _settingsService.ShowNewestFirst)
                return;

            TrackEvent(TelemetryEvents.ChangeShowOrder, 
                _settingsService.ShowNewestFirst.ToString(), 
                SelectedShowOrder.Value.ToString());

            _settingsService.ShowNewestFirst = SelectedShowOrder.Value;
            _settingsService.Save();
        }

        private void SaveFontSize()
        {
            if (Math.Abs(FontSize - _settingsService.FontSize) < 0.1D)
                return;

            TrackEvent(TelemetryEvents.ChangeFontSize, 
                _settingsService.FontSize.ToString(), 
                FontSize.ToString());

            _settingsService.FontSize = FontSize;
            _settingsService.Save();
        }

        private void SaveTextJustification()
        {
            if (TextJustification == (_settingsService.TextAlignment == TextAlignment.Justify))
                return;

            var newValue = TextJustification ? TextAlignment.Justify : TextAlignment.Left;

            TrackEvent(TelemetryEvents.ChangeTextAlignment,
                _settingsService.TextAlignment.ToString(),
                newValue.ToString());

            _settingsService.TextAlignment = newValue;
            _settingsService.Save();
        }

        private void SaveAutoMarkAsRead()
        {
            if (AutoMarkAsRead == _settingsService.AutoMarkAsRead)
                return;

            TrackEvent(TelemetryEvents.ChangeAutoMarkAsRead, 
                _settingsService.AutoMarkAsRead.ToString(),
                AutoMarkAsRead.ToString());

            _settingsService.AutoMarkAsRead = AutoMarkAsRead;
            _settingsService.Save();
        }

        private void SavePreloadItemCount()
        {
            if (PreloadItemCount == _settingsService.PreloadItemCount)
                return;

            TrackEvent(TelemetryEvents.ChangePreloadItemCount, 
                _settingsService.PreloadItemCount.ToString(),
                 PreloadItemCount.ToString());

            _settingsService.PreloadItemCount = PreloadItemCount;
            _settingsService.Save();
        }

        private void TrackEvent(string eventName, string oldValue, string newValue)
        {
            var properties = new Dictionary<string, string>
            {
                {"OldValue", oldValue},
                {"NewValue", newValue}
            };
            _telemetryManager.TrackEvent(eventName, properties);
        }
    }

    public class Lang
    {
        public string Name { get; private set; }
        public string Title { get; private set; }

        public Lang(string name)
        {
            var cultureInfo = new CultureInfo(name);
            Name = cultureInfo.Name;
            Title = cultureInfo.NativeName;
        }

        public Lang()
        {
            Name = String.Empty;
            Title = Strings.Resources.SettingsSystemLanguage;
        }
    }

    public class ShowOrderItem
    {
        public bool Value { get; private set; }
        public string Title { get; private set; }

        public ShowOrderItem(bool value)
        {
            Value = value;
            Title = value ? Strings.Resources.NewestFirstShowOrder : Strings.Resources.OldestFirstShowOrder;
        }
    }

    public class StreamViewItem
    {
        public StreamView View { get; private set; }
        public string Title { get; private set; }

        public StreamViewItem(StreamView view)
        {
            View = view;
            Title = GetTitle(view);
        }

        private string GetTitle(StreamView view)
        {
            switch (view)
            {
                case StreamView.ExpandedView:
                    return Strings.Resources.StreamViewExpanded;

                case StreamView.ListView:
                    return Strings.Resources.StreamViewList;

                default:
                    return view.ToString("G");
            }
        }
    }
}