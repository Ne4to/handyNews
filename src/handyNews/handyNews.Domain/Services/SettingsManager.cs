using Windows.Storage;
using Windows.UI.Xaml;
using handyNews.Domain.Services.Interfaces;
using handyNews.Domain.Utils;

namespace handyNews.Domain.Services
{
    public class SettingsManager : ISettingsManager
    {
        private const double DEFAULT_FONT_SIZE = 14D;
        private const double STREAM_TITLE_FONT_SIZE_MULT = 14D/DEFAULT_FONT_SIZE;
        private const double STREAM_DATE_FONT_SIZE_MULT = 11D/DEFAULT_FONT_SIZE;
        private const double FONT_SIZE_H1_MULT = 2D; // in em, em = 16px
        private const double FONT_SIZE_H2_MULT = 1.5D; // in em, em = 16px
        private const double FONT_SIZE_H3_MULT = 1.17D; // in em, em = 16px
        private const double FONT_SIZE_H4_MULT = 1D; // in em, em = 16px
        private const double FONT_SIZE_H5_MULT = .83D; // in em, em = 16px
        private const double FONT_SIZE_H6_MULT = .67D; // in em, em = 16px

        private const double PAGE_HEADER_FONT_SIZE_MULT = 24D/DEFAULT_FONT_SIZE;
        private const double SUBSCRIPTION_TREE_ITEM_FONT_SIZE_MULT = 18D/DEFAULT_FONT_SIZE;

        private const string SETTINGS_CONTAINER_NAME = "AppSettings";
        private readonly ApplicationDataContainer _rootContainer = ApplicationData.Current.LocalSettings;

        public SettingsManager()
        {
            DisplayCulture = string.Empty;
            HideEmptySubscriptions = true;
            ShowNewestFirst = true;
            StreamView = StreamView.ExpandedView;
            FontSize = DEFAULT_FONT_SIZE;
            TextAlignment = TextAlignment.Justify;
            AutoMarkAsRead = StreamView == StreamView.ExpandedView;
            PreloadItemCount = 10;

            Load();
        }

        public string DisplayCulture { get; set; }
        public bool HideEmptySubscriptions { get; set; }
        public bool ShowNewestFirst { get; set; }
        public bool AutoMarkAsRead { get; set; }
        public StreamView StreamView { get; set; }
        public TextAlignment TextAlignment { get; set; }
        public double FontSize { get; set; }
        public int PreloadItemCount { get; set; }

        public double StreamTitleFontSize => FontSize*STREAM_TITLE_FONT_SIZE_MULT;

        public double StreamDateFontSize => FontSize*STREAM_DATE_FONT_SIZE_MULT;

        public double FontSizeH1 => FontSize*FONT_SIZE_H1_MULT;

        public double FontSizeH2 => FontSize*FONT_SIZE_H2_MULT;

        public double FontSizeH3 => FontSize*FONT_SIZE_H3_MULT;

        public double FontSizeH4 => FontSize*FONT_SIZE_H4_MULT;

        public double FontSizeH5 => FontSize*FONT_SIZE_H5_MULT;

        public double FontSizeH6 => FontSize*FONT_SIZE_H6_MULT;

        public double PageHeaderFontSize => FontSize*PAGE_HEADER_FONT_SIZE_MULT;

        public double SubscriptionTreeItemFontSize => FontSize*SUBSCRIPTION_TREE_ITEM_FONT_SIZE_MULT;

        public void Save()
        {
            var container = _rootContainer.CreateContainer(SETTINGS_CONTAINER_NAME, ApplicationDataCreateDisposition.Always);

            container.Values["DisplayCulture"] = DisplayCulture;
            container.Values["HideEmptySubscriptions"] = HideEmptySubscriptions;
            container.Values["ShowNewestFirst"] = ShowNewestFirst;
            container.Values["StreamView"] = (int) StreamView;
            container.Values["FontSize"] = FontSize;
            container.Values["TextAlignment"] = (int) TextAlignment;
            container.Values["AutoMarkAsRead"] = AutoMarkAsRead;
            container.Values["PreloadItemCount"] = PreloadItemCount;
        }

        private void Load()
        {
            ApplicationDataContainer container;
            if (!_rootContainer.Containers.TryGetValue(SETTINGS_CONTAINER_NAME, out container))
            {
                return;
            }

            DisplayCulture = container.GetValue("DisplayCulture", string.Empty);
            HideEmptySubscriptions = container.GetValue("HideEmptySubscriptions", true);
            ShowNewestFirst = container.GetValue("ShowNewestFirst", true);
            StreamView = (StreamView) container.GetValue("StreamView", (int) StreamView.ExpandedView);
            FontSize = container.GetValue("FontSize", 11D);
            TextAlignment = (TextAlignment) container.GetValue("TextAlignment", (int) TextAlignment.Justify);
            PreloadItemCount = container.GetValue("PreloadItemCount", 10);

            // This setting did not exist in app version <= 1.1.3.15
            // If user updates app do not change behaviour
            AutoMarkAsRead = StreamView == StreamView.ExpandedView;
            if (container.Values.ContainsKey("AutoMarkAsRead"))
            {
                AutoMarkAsRead = container.GetValue("AutoMarkAsRead", true);
            }
        }
    }

    public enum StreamView
    {
        ExpandedView = 0,
        ListView = 1
    }
}