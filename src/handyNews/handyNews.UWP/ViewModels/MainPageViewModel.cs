using System;
using handyNews.Domain.Services.Interfaces;
using handyNews.UWP.Events;
using handyNews.UWP.Model;
using JetBrains.Annotations;
using PubSub;

namespace handyNews.UWP.ViewModels
{
    public class MainPageViewModel : BindableBase
    {
        private readonly IAuthenticationManager _authenticationManager;

        private bool _showStreamView;

        public bool ShowStreamView
        {
            get { return _showStreamView; }
            set { SetProperty(ref _showStreamView, value, nameof(ShowStreamView)); }
        }

        public MainPageViewModel([NotNull] IAuthenticationManager authenticationManager)
        {
            if (authenticationManager == null)
            {
                throw new ArgumentNullException(nameof(authenticationManager));
            }
            _authenticationManager = authenticationManager;

            this.Subscribe<ShowSubscriptionStreamEvent>(OnShowSubscriptionStreamEvent);
        }

        public async void OnNavigatedTo()
        {
            if (!_authenticationManager.IsUserAuthenticated)
            {
                await _authenticationManager.SignInAsync();
            }

            this.Publish(new RefreshTreeEvent());
        }

        private void OnShowSubscriptionStreamEvent(ShowSubscriptionStreamEvent data)
        {
            ShowStreamView = true;
        }
    }
}