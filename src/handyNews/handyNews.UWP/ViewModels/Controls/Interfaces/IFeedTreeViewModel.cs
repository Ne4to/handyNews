using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using handyNews.Domain.Models;

namespace handyNews.UWP.ViewModels.Controls.Interfaces
{
    public interface IFeedTreeViewModel : INotifyPropertyChanged
    {
        bool IsBusy { get; }
        ICommand ItemClickCommand { get; }
        IReadOnlyCollection<Feed> TreeItems { get; }
        void ShowRoot();
        void OnNavigatedTo();
    }
}