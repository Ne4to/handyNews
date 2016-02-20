using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using handyNews.Domain.Services;
using handyNews.Domain.Services.Interfaces;
using Inoreader.Annotations;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

namespace Inoreader.ViewModels.Pages
{
	public class ExpandedStreamPageViewModel : StreamPageViewModel
	{
		public ExpandedStreamPageViewModel([NotNull] INavigationService navigationService,
			[NotNull] ITelemetryManager telemetryManager,
			[NotNull] ITagsManager tagsManager,
			[NotNull] ISettingsManager settingsService,
			[NotNull] SavedStreamManager savedStreamManager,
			[NotNull] LocalStorageManager localStorageManager,
			[NotNull] INetworkManager networkManager,
            [NotNull] ISignInManager signInManager,
            [NotNull] IStreamManager streamManager)
			: base(navigationService,
				telemetryManager,
				tagsManager,
				settingsService,
				savedStreamManager,
				localStorageManager,
				networkManager, 
                signInManager, 
                streamManager)
		{
		}
	}
}
