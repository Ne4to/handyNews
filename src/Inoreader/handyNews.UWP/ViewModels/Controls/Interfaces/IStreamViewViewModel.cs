using System.ComponentModel;
using Inoreader.Domain.Models;

namespace handyNews.UWP.ViewModels.Controls.Interfaces
{
    public interface IStreamViewViewModel : INotifyPropertyChanged
    {
        StreamItemCollection Items { get; }
        void UpdateItems(string streamId);
    }
}