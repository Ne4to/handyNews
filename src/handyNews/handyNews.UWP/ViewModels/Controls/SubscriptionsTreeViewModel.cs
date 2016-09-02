using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using handyNews.API.Exceptions;
using handyNews.Domain.Models;
using handyNews.Domain.Services.Interfaces;
using handyNews.Domain.Utils;
using handyNews.UWP.Events;
using handyNews.UWP.Model;
using handyNews.UWP.Services;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using JetBrains.Annotations;
using PubSub;

namespace handyNews.UWP.ViewModels.Controls
{
    public class SubscriptionsTreeViewModel : BindableBase, ISubscriptionsTreeViewModel
    {
        public SubscriptionsTreeViewModel([NotNull] ISubscriptionsManager subscriptionsManager,
                                          [NotNull] INavigationService navigationService)
        {
            if (subscriptionsManager == null)
            {
                throw new ArgumentNullException(nameof(subscriptionsManager));
            }
            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }
            _subscriptionsManager = subscriptionsManager;
            _navigationService = navigationService;
        }

        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new DelegateCommand(OnItemClick));

        public void OnNavigatedTo()
        {
            this.Subscribe<RefreshTreeEvent>(OnRefreshTreeEvent);
        }

        public async void LoadSubscriptionsAsync()
        {
            List<SubscriptionItemBase> subscriptionItems = null;

            IsBusy = true;

            //Exception error = null;
            try
            {
                //IsOffline = false;

                subscriptionItems = await _subscriptionsManager.LoadSubscriptionsAsync();

                //var readAllItem = subscriptionItems.FirstOrDefault(t => t.Id == SpecialTags.Read);
                //if (readAllItem != null)
                //{
                //    _tileManager.UpdateAsync(readAllItem.UnreadCount);
                //}

                // await _localStorageManager.SaveSubscriptionsAsync(subscriptionItems);
            }
            catch (AuthenticationApiException)
            {
                //_navigationService.Navigate(PageTo);
                //_signInManager.SignOut();
                //_navigationService.Navigate(PageTokens.SignIn, null);

                throw;
                //SignInDialog dialog = new SignInDialog();
                //await dialog.ShowAsync();

                return;
            }
            catch (Exception ex)
            {
                //error = ex;
                //_telemetryManager.TrackError(ex);
            }
            finally
            {
                IsBusy = false;
            }

            //if (subscriptionItems == null)
            //{
            //    IsOffline = true;

            //    IsBusy = true;
            //    subscriptionItems = await _localStorageManager.LoadSubscriptionsAsync();
            //    IsBusy = false;
            //}

            if (subscriptionItems != null)
            {
                _rootItems = subscriptionItems;

                var cat = subscriptionItems.OfType<CategoryItem>()
                                           .FirstOrDefault(
                                               c => !_isRoot && c.Id.EqualsOrdinalIgnoreCase(_categoryId));

                if (cat != null)
                {
                    //SubscriptionsHeader = cat.Title;
                    _isRoot = false;
                    TreeItems = new List<SubscriptionItemBase>(cat.Subscriptions);
                }
                else
                {
                    //SubscriptionsHeader = Strings.Resources.SubscriptionsSectionHeader;
                    _isRoot = true;
                    TreeItems = _rootItems;
                }
            }

            //if (error != null)
            //{
            //    var msgbox = new MessageDialog(error.Message, Strings.Resources.ErrorDialogTitle);
            //    await msgbox.ShowAsync();
            //}
        }

        public void ShowRoot()
        {
            if (!_isRoot)
            {
                //SubscriptionsHeader = Strings.Resources.SubscriptionsSectionHeader;
                TreeItems = _rootItems;
                _isRoot = true;
                //return false;
            }
        }

        private void OnRefreshTreeEvent(RefreshTreeEvent data)
        {
            LoadSubscriptionsAsync();
        }

        private void OnItemClick(object args)
        {
            var clickEventArgs = (ItemClickEventArgs) args;

            var categoryItem = clickEventArgs.ClickedItem as CategoryItem;
            if (categoryItem != null)
            {
                //SubscriptionsHeader = categoryItem.Title;
                TreeItems = new List<SubscriptionItemBase>(categoryItem.Subscriptions);
                _isRoot = false;
                _categoryId = categoryItem.Id;
            }
            else
            {
                var subscriptionItem = clickEventArgs.ClickedItem as SubscriptionItem;
                if (subscriptionItem != null)
                {
                    this.Publish(new ShowSubscriptionStreamEvent(subscriptionItem));
                }
            }
        }

        #region Fields

        private readonly ISubscriptionsManager _subscriptionsManager;
        private readonly INavigationService _navigationService;

        private bool _isRoot = true;
        private List<SubscriptionItemBase> _rootItems;
        private string _categoryId;

        private ICommand _itemClickCommand;

        private bool _isBusy;
        private List<SubscriptionItemBase> _treeItems;

        #endregion

        #region Properties

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value, nameof(IsBusy)); }
        }

        public List<SubscriptionItemBase> TreeItems
        {
            get { return _treeItems; }
            private set { SetProperty(ref _treeItems, value, nameof(TreeItems)); }
        }

        #endregion
    }
}