using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inoreader.Annotations;
using Inoreader.Api;
using Inoreader.Domain.Services;
using Inoreader.Domain.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

namespace Inoreader.ViewModels.Pages
{
	public class ExpandedStreamPageViewModel : StreamPageViewModel
	{
		public ExpandedStreamPageViewModel([NotNull] ApiClient apiClient,
			[NotNull] INavigationService navigationService,
			[NotNull] ITelemetryManager telemetryManager,
			[NotNull] TagsManager tagsManager,
			[NotNull] AppSettingsService settingsService,
			[NotNull] SavedStreamManager savedStreamManager,
			[NotNull] LocalStorageManager localStorageManager,
			[NotNull] NetworkManager networkManager)
			: base(apiClient,
				navigationService,
				telemetryManager,
				tagsManager,
				settingsService,
				savedStreamManager,
				localStorageManager,
				networkManager)
		{
		}
	}
}
