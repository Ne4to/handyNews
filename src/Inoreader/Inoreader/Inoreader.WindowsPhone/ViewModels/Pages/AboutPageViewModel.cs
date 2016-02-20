using System;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.System;
using handyNews.Domain.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.ViewModels.Pages
{
	public class AboutPageViewModel : ViewModel
	{
		private readonly ITelemetryManager _telemetryManager;

		#region Fields

		private ICommand _reviewCommand;
		private ICommand _submitBugCommand;
		private ICommand _contributeCommand;

		#endregion

		public string AppVersion
		{
			get
			{
				var v = Package.Current.Id.Version;
				return String.Format("{0}.{1}.{2}.{3}", v.Major, v.Minor, v.Build, v.Revision);
			}
		}
		
		#region Commands

		public ICommand ReviewCommand
		{
			get { return _reviewCommand ?? (_reviewCommand = new DelegateCommand(OnReview)); }
		}

		public ICommand SubmitBugCommand
		{
			get { return _submitBugCommand ?? (_submitBugCommand = new DelegateCommand(OnSubmitBug)); }
		}

		public ICommand ContributeCommand
		{
			get { return _contributeCommand ?? (_contributeCommand = new DelegateCommand(OnContribute)); }
		}

		#endregion

		public AboutPageViewModel(ITelemetryManager telemetryManager)
		{
			if (telemetryManager == null) throw new ArgumentNullException("telemetryManager");
			_telemetryManager = telemetryManager;
		}

		private async void OnReview()
		{
			_telemetryManager.TrackEvent(TelemetryEvents.Review);
			await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
		}

		private async void OnSubmitBug()
		{
			_telemetryManager.TrackEvent(TelemetryEvents.SubmitBug);
			await Launcher.LaunchUriAsync(new Uri("https://github.com/Ne4to/handyNews/issues/new"));
		}
		
		private async void OnContribute()
		{			
			_telemetryManager.TrackEvent(TelemetryEvents.Contribute);
			await Launcher.LaunchUriAsync(new Uri("https://github.com/Ne4to/handyNews"));
		}
	}
}