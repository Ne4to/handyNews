using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Inoreader.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.ViewModels.Pages
{
	public class SettingsPageViewModel : ViewModel
	{
		#region Fields

		private readonly AppSettingsService _settingsService;
		private readonly TelemetryClient _telemetryClient;
		private readonly string _initialDisplayCulture;

		private List<Lang> _languages;
		private Lang _selectedLang;
		private bool _hideEmptySubscriptions;
		private bool _needAppRestart;

		#endregion

		#region Properties

		public List<Lang> Languages
		{
			get { return _languages; }
			set { SetProperty(ref _languages, value); }
		}

		public Lang SelectedLang
		{
			get { return _selectedLang; }
			set
			{
				if (SetProperty(ref _selectedLang, value))
					OnSelectedLangChanged();
			}
		}

		public bool HideEmptySubscriptions
		{
			get { return _hideEmptySubscriptions; }
			set
			{
				if (SetProperty(ref _hideEmptySubscriptions, value))
					OnHideEmptySubscriptionsChanged();
			}
		}

		public bool NeedAppRestart
		{
			get { return _needAppRestart; }
			set { SetProperty(ref _needAppRestart, value); }
		}

		#endregion

		public SettingsPageViewModel(AppSettingsService settingsService, TelemetryClient telemetryClient)
		{
			if (settingsService == null) throw new ArgumentNullException("settingsService");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			_settingsService = settingsService;
			_telemetryClient = telemetryClient;
			_initialDisplayCulture = _settingsService.DisplayCulture;
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

			Languages = new List<Lang>(new[]
										{
											new Lang(),
											new Lang("en-US"),
											new Lang("ru-RU")
										});

			SelectedLang = Languages.FirstOrDefault(l => l.Name == _initialDisplayCulture) ?? Languages.FirstOrDefault();
			NeedAppRestart = false;
			HideEmptySubscriptions = _settingsService.HideEmptySubscriptions;
		}

		private void OnSelectedLangChanged()
		{
			if (_initialDisplayCulture != SelectedLang.Name)
			{
				var eventTelemetry = new EventTelemetry(TelemetryEvents.ChangeDisplayCulture);
				eventTelemetry.Properties.Add("OldValue", _initialDisplayCulture);
				eventTelemetry.Properties.Add("NewValue", SelectedLang.Name);
				_telemetryClient.TrackEvent(eventTelemetry);
			}

			_settingsService.DisplayCulture = SelectedLang.Name;
			_settingsService.Save();

			NeedAppRestart = SelectedLang.Name != _initialDisplayCulture;
		}

		private void OnHideEmptySubscriptionsChanged()
		{
			if (HideEmptySubscriptions != _settingsService.HideEmptySubscriptions)
			{
				var eventTelemetry = new EventTelemetry(TelemetryEvents.ChangeHideEmptySubscriptions);
				eventTelemetry.Properties.Add("OldValue", _settingsService.HideEmptySubscriptions.ToString());
				eventTelemetry.Properties.Add("NewValue", HideEmptySubscriptions.ToString());
				_telemetryClient.TrackEvent(eventTelemetry);
			}

			_settingsService.HideEmptySubscriptions = HideEmptySubscriptions;
			_settingsService.Save();
		}
	}

	public class Lang
	{
		public string Name { get; set; }
		public string Title { get; set; }

		public Lang(string name)
		{
			var cultureInfo = new CultureInfo(name);
			Name = cultureInfo.Name;
			Title = cultureInfo.NativeName;
		}

		public Lang()
		{
			Name = String.Empty;
			Title = Strings.Resources.SettingsSystemLanguage;
		}
	}
}