using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.Views.Pages
{
	public sealed partial class SettingsPage : IView
	{
		public SettingsPage()
		{
			InitializeComponent();
		}

		private void SettingsPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			ItemsLayoutRoot.Width = Window.Current.Bounds.Width - 10;
			DisplayInformation.GetForCurrentView().OrientationChanged += SettingsPage_OrientationChanged;
		}

		private void SettingsPage_OnUnloaded(object sender, RoutedEventArgs e)
		{
			DisplayInformation.GetForCurrentView().OrientationChanged -= SettingsPage_OrientationChanged;
		}

		void SettingsPage_OrientationChanged(DisplayInformation sender, object args)
		{
			ItemsLayoutRoot.Width = Window.Current.Bounds.Width - 10;
		}
	}
}
