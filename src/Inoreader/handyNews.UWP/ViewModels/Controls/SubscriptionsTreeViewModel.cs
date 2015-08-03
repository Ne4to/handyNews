﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using handyNews.UWP.Model;
using handyNews.UWP.Services;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using Inoreader.Api.Exceptions;
using Inoreader.Domain.Models;
using Inoreader.Domain.Services.Interfaces;

namespace handyNews.UWP.ViewModels.Controls
{
    public class SubscriptionsTreeViewModel : BindableBase, ISubscriptionsTreeViewModel
    {
        #region Fields

        private readonly ISubscriptionsManager _subscriptionsManager;

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
            set { SetProperty(ref _isBusy, value); }
        }

        public List<SubscriptionItemBase> TreeItems
        {
            get { return _treeItems; }
            private set { SetProperty(ref _treeItems, value); }
        }

        #endregion

        public event EventHandler<SubscriptionSelectedEventArgs> SubscriptionSelected = delegate { }; 

        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new DelegateCommand(OnItemClick));

        public SubscriptionsTreeViewModel(ISubscriptionsManager subscriptionsManager)
        {
            if (subscriptionsManager == null) throw new ArgumentNullException(nameof(subscriptionsManager));
            _subscriptionsManager = subscriptionsManager;
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
                //_signInManager.SignOut();
                //_navigationService.Navigate(PageTokens.SignIn, null);
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
                    .FirstOrDefault(c => !_isRoot && String.Equals(c.Id, _categoryId, StringComparison.OrdinalIgnoreCase));

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

        private void OnItemClick(object args)
        {
            var clickEventArgs = (ItemClickEventArgs)args;

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
                    SubscriptionSelected(this, new SubscriptionSelectedEventArgs(subscriptionItem));
                //    var pageToken = _settingsService.StreamView == StreamView.ExpandedView ? PageTokens.ExpandedStream : PageTokens.ListStream;
                //    var navParam = new StreamPageNavigationParameter
                //    {
                //        StreamId = subscriptionItem.Id,
                //        Title = subscriptionItem.PageTitle
                //    };
                //    _navigationService.Navigate(pageToken, navParam.ToJson());
                }
            }
        }
    }
}