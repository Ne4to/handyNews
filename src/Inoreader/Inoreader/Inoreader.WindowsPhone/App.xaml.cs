using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.API;
using handyNews.Domain.Models;
using handyNews.Domain.Models.States;
using handyNews.Domain.Services;
using handyNews.Domain.Services.Interfaces;
using Inoreader.ViewModels.Pages;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Microsoft.Practices.Unity;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json.Linq;

namespace Inoreader
{
	public sealed partial class App
	{
		/// <summary>
		/// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
		/// </summary>
		private readonly TelemetryClient _telemetryClient = new TelemetryClient();

		// New up the singleton container that will be used for type resolution in the app
		readonly IUnityContainer _container = new UnityContainer();
        private ApiClient _apiClient;
	    private ISignInManager _signInManager;
		readonly SettingsManager _settingsManager = new SettingsManager();
		
		public App()
		{
			InitializeComponent();
		}

		protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
		{
			var pageToken = _signInManager.SignInRequired ? PageTokens.SignIn : PageTokens.Subscriptions;
			NavigationService.Navigate(pageToken, null);
			return Task.FromResult(0);
		}

		protected override async void OnLaunched(LaunchActivatedEventArgs args)
		{
			base.OnLaunched(args);

			// Hide the status bar
			StatusBar statusBar = StatusBar.GetForCurrentView();
			await statusBar.HideAsync();
		}

		protected override async Task OnInitializeAsync(IActivatedEventArgs args)
		{
			await base.OnInitializeAsync(args);

			_container.RegisterInstance<ISessionStateService>(SessionStateService);
			_container.RegisterInstance<INavigationService>(NavigationService);

			_container.RegisterType<ICredentialService, CredentialService>(new ContainerControlledLifetimeManager());
			_container.RegisterType<NetworkManager>(new ContainerControlledLifetimeManager());
			_container.RegisterType<TileManager>(new ContainerControlledLifetimeManager());
			_container.RegisterInstance(_settingsManager);

            var telemetryManager = new ApplicationInsightsTelemetryManager(_telemetryClient);
            _container.RegisterInstance<ITelemetryManager>(telemetryManager);
            //_container.RegisterInstance(_telemetryClient);
            _container.RegisterType<ImageManager>(new ContainerControlledLifetimeManager());


            _container.RegisterType<INetworkManager, NetworkManager>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISettingsManager, SettingsManager>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISessionStore, SessionStore>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IApiSession, ApiSession>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISubscriptionsManager, SubscriptionsManager>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IStreamManager, StreamManager>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ITileManager, TileManager>(new ContainerControlledLifetimeManager());

            var uri = new Uri("ms-appx:///Assets/ApiAuth.json");
			var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
			var strData = await FileIO.ReadTextAsync(file);
			var data = JObject.Parse(strData);

			var appId = data["AppId"].ToString();
			var appKey = data["AppKey"].ToString();
		    var apiSession = _container.Resolve<IApiSession>();
			_apiClient = new ApiClient(appId, appKey, apiSession);
			_container.RegisterInstance(_apiClient);
            
		    _container.RegisterType<ISignInManager, SignInManager>(new ContainerControlledLifetimeManager());

		    _signInManager = _container.Resolve<ISignInManager>();
            
            var localStorageManager = new LocalStorageManager(telemetryManager);
			localStorageManager.Init();
			_container.RegisterInstance(localStorageManager);

			var savedStreamManager = new SavedStreamManager(localStorageManager);
			_container.RegisterInstance(savedStreamManager);
            
            _container.RegisterType<ITagsManager, TagsManager>(new ContainerControlledLifetimeManager());
            var tagsManager = _container.Resolve<ITagsManager>();
			((TagsManager)tagsManager).ProcessQueue();

			Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = _settingsManager.DisplayCulture;

			var unityServiceLocator = new UnityServiceLocator(_container);
			ServiceLocator.SetLocatorProvider(() => unityServiceLocator);

			ViewModelLocationProvider.SetDefaultViewModelFactory(ViewModelFactory);
			ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(ViewModelTypeResolver);
		}

		protected override void OnHardwareButtonsBackPressed(object sender, BackPressedEventArgs e)
		{
			var view = ((Frame)Window.Current.Content).Content as IView;
			if (view != null)
			{
				var navigateBackwards = view.DataContext as INavigateBackwards;
				if (navigateBackwards != null)
				{
					if (!navigateBackwards.NavigateBack())
					{
						e.Handled = true;
					}

					return;
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

		protected override void OnRegisterKnownTypesForSerialization()
		{
			base.OnRegisterKnownTypesForSerialization();

			// Subscriptions page state
			SessionStateService.RegisterKnownType(typeof(SubscriptionItemBase));
			SessionStateService.RegisterKnownType(typeof(SubscriptionItem));
			SessionStateService.RegisterKnownType(typeof(CategoryItem));
			SessionStateService.RegisterKnownType(typeof(List<SubscriptionItemBase>));

			// Stream page state
			SessionStateService.RegisterKnownType(typeof(StreamItem));
			SessionStateService.RegisterKnownType(typeof(HeaderSpaceStreamItem));
			SessionStateService.RegisterKnownType(typeof(EmptySpaceStreamItem));
			SessionStateService.RegisterKnownType(typeof(StreamItemCollectionState));
		}
	}
}