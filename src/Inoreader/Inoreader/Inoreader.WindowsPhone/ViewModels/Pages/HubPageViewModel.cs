using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using Inoreader.ViewModels.Details;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Microsoft.Practices.Unity;

namespace Inoreader.ViewModels.Pages
{
	public class HubPageViewModel : ViewModel, INavigateBackwards
	{
		private readonly INavigationService _navigationService;

		public HubMenuViewModel Menu { get; private set; }
		public SubscriptionsViewModel Subscriptions { get; private set; }

		public HubPageViewModel(IUnityContainer container, INavigationService navigationService)
		{
			if (container == null) throw new ArgumentNullException("container");
			if (navigationService == null) throw new ArgumentNullException("navigationService");

			_navigationService = navigationService;

			Menu = container.Resolve<HubMenuViewModel>();
			Subscriptions = container.Resolve<SubscriptionsViewModel>();
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

			// Forbid return to the Sign in page
			if (navigationMode == NavigationMode.New)
				_navigationService.ClearHistory();

			Subscriptions.LoadSubscriptions();
			Menu.LoadData();
		}

		public bool NavigateBack()
		{
			return Subscriptions.NavigateBack();
		}
	}

}