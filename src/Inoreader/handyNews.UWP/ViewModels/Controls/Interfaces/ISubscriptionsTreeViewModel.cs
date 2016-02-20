using System;
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
        event EventHandler<SubscriptionSelectedEventArgs> SubscriptionSelected;
        List<SubscriptionItemBase> TreeItems { get; }
        void LoadSubscriptionsAsync();
        void ShowRoot();
    }

    public class SubscriptionSelectedEventArgs : EventArgs
    {
        public SubscriptionItem Item { get; private set; }

        public SubscriptionSelectedEventArgs(SubscriptionItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            Item = item;
        }
    }
}