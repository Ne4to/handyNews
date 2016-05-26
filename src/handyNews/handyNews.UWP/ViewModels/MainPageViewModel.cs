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

        public MainPageViewModel([NotNull] IAuthenticationManager authenticationManager)
        {
            if (authenticationManager == null) throw new ArgumentNullException(nameof(authenticationManager));
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