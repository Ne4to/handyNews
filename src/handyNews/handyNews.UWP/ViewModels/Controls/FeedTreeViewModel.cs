using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
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
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FeedTreeViewModel : BindableBase, IFeedTreeViewModel
    {
        private readonly IFeedManager _feedManager;
        private string _categoryId;

        private bool _isBusy;

        private bool _isRoot = true;

        private ICommand _itemClickCommand;
        private IReadOnlyCollection<Feed> _rootItems;
        private IReadOnlyCollection<Feed> _treeItems;
        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new DelegateCommand(OnItemClick));

        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetProperty(ref _isBusy, value, nameof(IsBusy)); }
        }

        public IReadOnlyCollection<Feed> TreeItems
        {
            get { return _treeItems; }
            private set { SetProperty(ref _treeItems, value, nameof(TreeItems)); }
        }

        public FeedTreeViewModel([NotNull] IFeedManager feedManager)
        {
            if (feedManager == null)
            {
                throw new ArgumentNullException(nameof(feedManager));
            }

            _feedManager = feedManager;
        }

        public void OnNavigatedTo()
        {
            this.Subscribe<RefreshTreeEvent>(OnRefreshTreeEvent);
        }

        public async void LoadSubscriptionsAsync()
        {
            IReadOnlyCollection<Feed> subscriptionItems = null;

            IsBusy = true;

            //Exception error = null;
            try
            {
                //IsOffline = false;

                subscriptionItems = await _feedManager.GetFeedsAsync();

                //var readAllItem = subscriptionItems.FirstOrDefault(t => t.Id == SpecialTags.Read);
                //if (readAllItem != null)
                //{
                //    _tileManager.UpdateAsync(readAllItem.UnreadCount);
                //}

                // await _localStorageManager.SaveSubscriptionsAsync(subscriptionItems);
            }
            //catch (AuthenticationApiException)
            //{
            //    //_navigationService.Navigate(PageTo);
            //    //_signInManager.SignOut();
            //    //_navigationService.Navigate(PageTokens.SignIn, null);

            //    throw;
            //    //SignInDialog dialog = new SignInDialog();
            //    //await dialog.ShowAsync();

            //    return;
            //}
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
            //    subscriptionItems = await _localStorageManager.GetFeedsAsync();
            //    IsBusy = false;
            //}

            if (subscriptionItems != null)
            {
                _rootItems = subscriptionItems;

                var cat = subscriptionItems
                    .FirstOrDefault(
                        c => !_isRoot && c.Id.EqualsOrdinalIgnoreCase(_categoryId));

                if (cat != null)
                {
                    //SubscriptionsHeader = cat.Title;
                    _isRoot = false;
                    TreeItems = new List<Feed>(cat.Children);
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

            var categoryItem = (Feed) clickEventArgs.ClickedItem;
            if (categoryItem.Children?.Any() ?? false)
            {
                //SubscriptionsHeader = categoryItem.Title;
                TreeItems = new List<Feed>(categoryItem.Children);
                _isRoot = false;
                _categoryId = categoryItem.Id;
            }
            else
            {
                this.Publish(new ShowSubscriptionStreamEvent(categoryItem));
            }
        }
    }
}