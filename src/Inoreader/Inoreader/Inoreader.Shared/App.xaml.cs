using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Inoreader.Api;
using Inoreader.Services;
using Inoreader.ViewModels.Pages;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Microsoft.Practices.Unity;
using Microsoft.ApplicationInsights;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Inoreader
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public sealed partial class App
	{
		/// <summary>
		/// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
		/// </summary>
		public readonly TelemetryClient TelemetryClient = new TelemetryClient();

		// New up the singleton container that will be used for type resolution in the app
		readonly IUnityContainer _container = new UnityContainer();
		readonly ApiClient _apiClient = new ApiClient();
		readonly AppSettingsService _appSettingsService = new AppSettingsService();

		public App()
		{
			InitializeComponent();		
		}

		protected override async Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
		{
			// Hide the status bar
			StatusBar statusBar = StatusBar.GetForCurrentView();
			await statusBar.HideAsync();

			var pageToken = _apiClient.SignInRequired ? PageTokens.SignIn : PageTokens.Subscriptions;
			NavigationService.Navigate(pageToken, null);			
		}

		protected override async Task OnInitializeAsync(IActivatedEventArgs args)
		{
			await base.OnInitializeAsync(args);

			_container.RegisterInstance<ISessionStateService>(SessionStateService);
			_container.RegisterInstance<INavigationService>(NavigationService);

			_container.RegisterType<ICredentialService, CredentialService>(new ContainerControlledLifetimeManager());
			_container.RegisterInstance(_apiClient);
			_container.RegisterInstance(_appSettingsService);
			_container.RegisterInstance(TelemetryClient);			

			// Set a factory for the ViewModelLocator to use the container to construct view models so their 
			// dependencies get injected by the container
			//ViewModelLocationProvider.
			//ViewModelLocator.SetDefaultViewModelFactory((viewModelType) => _container.Resolve(viewModelType));

			Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = _appSettingsService.DisplayCulture;

			ViewModelLocationProvider.SetDefaultViewModelFactory(ViewModelFactory);
			ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(ViewModelTypeResolver);
		}

		protected override void OnHardwareButtonsBackPressed(object sender, BackPressedEventArgs e)
		{
			TelemetryClient.TrackEvent(TelemetryEvents.HardwareButtonsBackPressed);

			var view = ((Frame)Window.Current.Content).Content as IView;
			if (view != null)
			{
				var navigateBackwards = view.DataContext as INavigateBackwards;
				if (navigateBackwards != null)
				{
					if (!navigateBackwards.NavigateBack())
					{
						e.Handled = true;
						return;
					}
				}
			}

			base.OnHardwareButtonsBackPressed(sender, e);
		}

		protected override Type GetPageType(string pageToken)
		{
			var pageType = Type.GetType(String.Format("Inoreader.Views.Pages.{0}Page", pageToken));
			return pageType;
		}

		private object ViewModelFactory(Type viewModelType)
		{
			return _container.Resolve(viewModelType);
		}

		private Type ViewModelTypeResolver(Type viewType)
		{
			var typeName = viewType.ToString().Replace("Inoreader.Views", "Inoreader.ViewModels") + "ViewModel";
			return Type.GetType(typeName);
		}
	}
}