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

        private bool _abc = true;
        public bool Abc
        {
            get { return _abc; }
            set { SetProperty(ref _abc, value, nameof(Abc)); }
        }

        public MainPageViewModel([NotNull] IAuthenticationManager authenticationManager)
        {
            if (authenticationManager == null)
            {
                throw new ArgumentNullException(nameof(authenticationManager));
            }
            _authenticationManager = authenticationManager;
        }

        public async void OnNavigatedTo()
        {
            if (!_authenticationManager.IsUserAuthenticated)
            {
                await _authenticationManager.SignInAsync();
            }

            this.Publish(new RefreshTreeEvent());
        }
    }
}