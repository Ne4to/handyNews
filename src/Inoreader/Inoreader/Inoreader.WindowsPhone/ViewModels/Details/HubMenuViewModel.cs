using System;
using System.Windows.Input;
using Inoreader.Api;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

namespace Inoreader.ViewModels.Details
{
	public class HubMenuViewModel : BindableBase
	{
		private readonly INavigationService _navigationService;
		private readonly ApiClient _apiClient;
		private ICommand _signOutCommand;

		private string _userName;

		public string UserName
		{
			get { return _userName; }
			set { SetProperty(ref _userName, value); }
		}

		public ICommand SignOutCommand
		{
			get { return _signOutCommand ?? (_signOutCommand = new DelegateCommand(OnSignOut)); }
		}

		public HubMenuViewModel(INavigationService navigationService, ApiClient apiClient)
		{
			if (navigationService == null) throw new ArgumentNullException("navigationService");
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			
			_navigationService = navigationService;
			_apiClient = apiClient;
		}

		public async void LoadData()
		{
			var data = await _apiClient.GetUserInfoAsync();
			UserName = data.UserName;
		}

		private void OnSignOut()
		{
			_apiClient.ClearSession();
			_navigationService.Navigate(PageTokens.SignIn, null);
			_navigationService.ClearHistory();
		}
	}
}