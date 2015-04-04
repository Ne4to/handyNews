using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inoreader.Annotations;
using Inoreader.Api;
using Inoreader.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

namespace Inoreader.ViewModels.Pages
{
	public class ListStreamPageViewModel : StreamPageViewModel
	{
		public ListStreamPageViewModel([NotNull] ApiClient apiClient, 
			[NotNull] INavigationService navigationService, 
			[NotNull] TelemetryClient telemetryClient, 
			[NotNull] TagsManager tagsManager, 
			[NotNull] AppSettingsService settingsService,
			[NotNull] SavedStreamManager savedStreamManager,
			[NotNull] LocalStorageManager localStorageManager) 
			: base(apiClient, 
				navigationService, 
				telemetryClient, 
				tagsManager, 
				settingsService,
				savedStreamManager, 
				localStorageManager)
		{
		}
	}
}
