using Windows.UI.Xaml;

namespace Inoreader.Domain.Services.Interfaces
{
    public interface ISettingsManager
    {
        string DisplayCulture { get; set; }
        bool HideEmptySubscriptions { get; set; }
        bool ShowNewestFirst { get; set; }
        bool AutoMarkAsRead { get; set; }
        StreamView StreamView { get; set; }
        TextAlignment TextAlignment { get; set; }
        double FontSize { get; set; }
        int PreloadItemCount { get; set; }
        double StreamTitleFontSize { get; }
        double StreamDateFontSize { get; }
        double FontSizeH1 { get; }
        double FontSizeH2 { get; }
        double FontSizeH3 { get; }
        double FontSizeH4 { get; }
        double FontSizeH5 { get; }
        double FontSizeH6 { get; }
        double PageHeaderFontSize { get; }
        double SubscriptionTreeItemFontSize { get; }

        void Save();
    }
}