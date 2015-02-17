using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using Inoreader.Api;
using Inoreader.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace Inoreader.ViewModels.Pages
{
	public class SignInPageViewModel : ViewModel, INavigateBackwards
	{
		private readonly INavigationService _navigationService;
		private readonly ICredentialService _credentialService;
		private readonly ApiClient _apiClient;
		private readonly TelemetryClient _telemetryClient;

		#region Fields

		private bool _isBusy;
		private string _email;
		private string _password;
		private bool _rememberMe = true;

		private ReactiveCommand<object> _signInCommand;

		#endregion

		#region Properties

		public bool IsBusy
		{
			get { return _isBusy; }
			set { SetProperty(ref _isBusy, value); }
		}

		public string Email
		{
			get { return _email; }
			set { SetProperty(ref _email, value); }
		}

		public string Password
		{
			get { return _password; }
			set { SetProperty(ref _password, value); }
		}

		public bool RememberMe
		{
			get { return _rememberMe; }
			set { SetProperty(ref _rememberMe, value); }
		}

		#endregion

		#region Commands

		public ICommand SignInCommand
		{
			get
			{
				if (_signInCommand == null)
				{
					var canExecute = this.WhenAny(vm => vm.Email,
						vm => vm.Password, vm => vm.IsBusy,
						(e, p, b) => !String.IsNullOrEmpty(e.Value) && !String.IsNullOrEmpty(p.Value) && !b.Value);
					_signInCommand = ReactiveCommand.Create(canExecute);
					_signInCommand.Subscribe(OnSignIn);
				}

				return _signInCommand;
			}
		}

		#endregion

		public SignInPageViewModel(INavigationService navigationService, ICredentialService credentialService, ApiClient apiClient, TelemetryClient telemetryClient)
		{
			if (navigationService == null) throw new ArgumentNullException("navigationService");
			if (credentialService == null) throw new ArgumentNullException("credentialService");
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");

			_navigationService = navigationService;
			_credentialService = credentialService;
			_apiClient = apiClient;
			_telemetryClient = telemetryClient;
		}

#if DEBUG
		public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			// The base implementation uses RestorableStateAttribute and Reflection to save and restore state
			// If you do not use this attribute, do not invoke base impkementation to prevent execution this useless code.

			var uri = new Uri("ms-appx:///Assets/DebugSignInData.json");
			var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
			var strData = await FileIO.ReadTextAsync(file);
			var data = JObject.Parse(strData);

			Email = data["username"].ToString();
			Password = data["password"].ToString();
		}
#endif

		public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
		{
			// The base implementation uses RestorableStateAttribute and Reflection to save and restore state
			// If you do not use this attribute, do not invoke base impkementation to prevent execution this useless code.

			if (!suspending)
			{
				if (_signInCommand != null)
					_signInCommand.Dispose();
			}
		}

		private async void OnSignIn(object obj)
		{
			IsBusy = true;

			Exception error = null;

			try
			{
				var stopwatch = Stopwatch.StartNew();
				
				await _apiClient.SignInAsync(Email, Password);
				
				stopwatch.Stop();
				_telemetryClient.TrackMetric(TemetryMetrics.SignInResponseTime, stopwatch.Elapsed.TotalSeconds);
				_telemetryClient.TrackEvent(TelemetryEvents.SignIn);
				
				if (RememberMe)
				{
					_credentialService.Save(Email, Password);
				}

				_navigationService.Navigate(PageTokens.Subscriptions, null);
			}
			catch (Exception ex)
			{
				error = ex;
				_telemetryClient.TrackExceptionFull(ex);				
			}
			finally
			{
				IsBusy = false;
			}

			if (error != null)
			{
				MessageDialog msgbox = new MessageDialog(error.Message, Strings.Resources.ErrorDialogTitle);
				await msgbox.ShowAsync();
			}
		}

		public bool NavigateBack()
		{
			return true;
		}
	}


}