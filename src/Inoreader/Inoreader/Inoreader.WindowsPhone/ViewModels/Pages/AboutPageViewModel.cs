using System;
using System.Windows.Input;
using Windows.ApplicationModel.Store;
using Windows.System;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.ViewModels.Pages
{
	public class AboutPageViewModel : ViewModel
	{
		#region Fields

		private ICommand _reviewCommand;
		private ICommand _submitBugCommand;
		private ICommand _contributeCommand;

		#endregion
		
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
		
		private async void OnReview()
		{
			await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
		}

		private async void OnSubmitBug()
		{
			await Launcher.LaunchUriAsync(new Uri("https://github.com/Ne4to/InoreaderFree/issues/new"));
		}
		
		private async void OnContribute()
		{
			await Launcher.LaunchUriAsync(new Uri("https://github.com/Ne4to/InoreaderFree"));
		}
	}
}