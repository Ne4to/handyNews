using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using handyNews.Domain.Models;

namespace handyNews.UWP.ViewModels.Controls.Interfaces
{
    public interface ISubscriptionsTreeViewModel : INotifyPropertyChanged
    {
        bool IsBusy { get; set; }
        ICommand ItemClickCommand { get; }
        List<SubscriptionItemBase> TreeItems { get; }
        void LoadSubscriptionsAsync();
        void ShowRoot();
        void OnNavigatedTo();
    }
}