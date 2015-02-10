using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.UI.Xaml.Navigation;
using Inoreader.ViewModels.Details;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Microsoft.Practices.Unity;

namespace Inoreader.ViewModels.Pages
{
	public class SubscriptionsPageViewModel : ViewModel, INavigateBackwards
	{
		private readonly INavigationService _navigationService;

		private ICommand _settingsPageCommand;
		private ICommand _aboutPageCommand;

		public SubscriptionsViewModel Subscriptions { get; private set; }

		public ICommand SettingsPageCommand
		{
			get { return _settingsPageCommand ?? (_settingsPageCommand = new DelegateCommand(OnSettingsPage)); }
		}

		public ICommand AboutPageCommand
		{
			get { return _aboutPageCommand ?? (_aboutPageCommand = new DelegateCommand(OnAboutPage)); }
		}

		public SubscriptionsPageViewModel(IUnityContainer container, INavigationService navigationService)
		{
			if (container == null) throw new ArgumentNullException("container");
			if (navigationService == null) throw new ArgumentNullException("navigationService");

			_navigationService = navigationService;

			Subscriptions = container.Resolve<SubscriptionsViewModel>();
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

			Subscriptions.LoadSubscriptions();
		}

		public bool NavigateBack()
		{
			return Subscriptions.NavigateBack();
		}

		private void OnSettingsPage()
		{
			_navigationService.Navigate(PageTokens.Settings, null);
		}

		private void OnAboutPage()
		{
			_navigationService.Navigate(PageTokens.About, null);
		}
	}
}