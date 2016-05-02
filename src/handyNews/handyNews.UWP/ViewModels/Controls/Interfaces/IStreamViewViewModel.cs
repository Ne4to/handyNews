using System.ComponentModel;
using handyNews.Domain.Models;

namespace handyNews.UWP.ViewModels.Controls.Interfaces
{
    public interface IStreamViewViewModel : INotifyPropertyChanged
    {
        StreamItemCollection Items { get; }
        void UpdateItems(string streamId);
        void OnNavigatedTo();
    }
}