using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Inoreader.Api;
using Inoreader.Models;
using Inoreader.Models.States;
using Inoreader.Services;
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
		public readonly TelemetryClient TelemetryClient = new TelemetryClient();

		// New up the singleton container that will be used for type resolution in the app
		readonly IUnityContainer _container = new UnityContainer();
		private ApiClient _apiClient;
		readonly AppSettingsService _appSettingsService = new AppSettingsService();
		private TagsManager _tagsManager;
		private CacheManager _cacheManager;

		public App()
		{
			InitializeComponent();

			Suspending += App_Suspending;
		}

		protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
		{
			var pageToken = _apiClient.SignInRequired ? PageTokens.SignIn : PageTokens.Subscriptions;
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
			_container.RegisterInstance(_appSettingsService);
			_container.RegisterInstance(TelemetryClient);

			var uri = new Uri("ms-appx:///Assets/ApiAuth.json");
			var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
			var strData = await FileIO.ReadTextAsync(file);
			var data = JObject.Parse(strData);

			var appId = data["AppId"].ToString();
			var appKey = data["AppKey"].ToString();
			_apiClient = new ApiClient(appId, appKey);
			_container.RegisterInstance(_apiClient);

			_cacheManager = new CacheManager(TelemetryClient);
			await _cacheManager.InitAsync();
			_container.RegisterInstance(_cacheManager);

			var tagsManagerState = await _cacheManager.LoadTagsManagerStateAsync();
			_tagsManager = new TagsManager(tagsManagerState, _apiClient, TelemetryClient, _container.Resolve<NetworkManager>());
			_container.RegisterInstance(_tagsManager);
			_tagsManager.ProcessQueue();			

			Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = _appSettingsService.DisplayCulture;

			var unityServiceLocator = new UnityServiceLocator(_container);
			ServiceLocator.SetLocatorProvider(() => unityServiceLocator);

			ViewModelLocationProvider.SetDefaultViewModelFactory(ViewModelFactory);
			ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(ViewModelTypeResolver);
		}

		async void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
		{
			if (_tagsManager == null || _cacheManager == null)
				return;

			var deferral = e.SuspendingOperation.GetDeferral();

			var state = _tagsManager.GetState();
			await _cacheManager.SaveTagsManagerStateAsync(state);

			deferral.Complete();
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
			SessionStateService.RegisterKnownType(typeof(TreeItemBase));
			SessionStateService.RegisterKnownType(typeof(SubscriptionItem));
			SessionStateService.RegisterKnownType(typeof(CategoryItem));
			SessionStateService.RegisterKnownType(typeof(List<TreeItemBase>));
			
			// Stream page state
			SessionStateService.RegisterKnownType(typeof(StreamItem));
			SessionStateService.RegisterKnownType(typeof(EmptySpaceStreamItem));
			SessionStateService.RegisterKnownType(typeof(StreamItemCollectionState));
		}
	}
}