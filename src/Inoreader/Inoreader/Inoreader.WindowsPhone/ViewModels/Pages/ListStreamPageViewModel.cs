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
			[NotNull] CacheManager cacheManager, 
			[NotNull] TagsManager tagsManager, 
			[NotNull] AppSettingsService settingsService,
			[NotNull] SavedStreamManager savedStreamManager) 
			: base(apiClient, 
				navigationService, 
				telemetryClient, 
				cacheManager, 
				tagsManager, 
				settingsService,
				savedStreamManager)
		{
		}
	}
}
