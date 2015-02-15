using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Navigation;
using Inoreader.Annotations;
using Inoreader.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Practices.Prism.Mvvm;
using ReactiveUI;

namespace Inoreader.ViewModels.Pages
{
	public class SettingsPageViewModel : ViewModel
	{
		#region Fields

		private readonly AppSettingsService _settingsService;
		private readonly TelemetryClient _telemetryClient;
		private readonly CacheManager _cacheManager;
		private readonly string _initialDisplayCulture;

		private List<Lang> _languages;
		private Lang _selectedLang;
		private bool _hideEmptySubscriptions;
		private bool _needAppRestart;
		private ulong _totalCacheSize;
		private bool _isCacheBusy;
		private List<ShowOrderItem> _showOrderItems;
		private ShowOrderItem _selectedShowOrder;

		private ReactiveCommand<object> _clearCacheComand;

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

		public ulong TotalCacheSize
		{
			get { return _totalCacheSize; }
			set { SetProperty(ref _totalCacheSize, value); }
		}

		public bool IsCacheBusy
		{
			get { return _isCacheBusy; }
			set { SetProperty(ref _isCacheBusy, value); }
		}

		public List<ShowOrderItem> ShowOrderItems
		{
			get { return _showOrderItems; }
			set { SetProperty(ref _showOrderItems, value); }
		}

		public ShowOrderItem SelectedShowOrder
		{
			get { return _selectedShowOrder; }
			set { if (SetProperty(ref _selectedShowOrder, value))
				OnSelectedShowOrderChanged();
			}
		}

		#endregion

		public ICommand ClearCacheCommand
		{
			get
			{
				if (_clearCacheComand == null)
				{
					var canExecute = this.WhenAny(vm => vm.TotalCacheSize, vm => vm.IsCacheBusy, (ts, cb) => ts.Value != 0UL && !cb.Value);
					_clearCacheComand = ReactiveCommand.Create(canExecute);
					_clearCacheComand.Subscribe(OnClearCache);
				}

				return _clearCacheComand;
			}
		}

		public SettingsPageViewModel([NotNull] AppSettingsService settingsService,
			[NotNull] TelemetryClient telemetryClient,
			[NotNull] CacheManager cacheManager)
		{
			if (settingsService == null) throw new ArgumentNullException("settingsService");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			if (cacheManager == null) throw new ArgumentNullException("cacheManager");

			_settingsService = settingsService;
			_telemetryClient = telemetryClient;
			_cacheManager = cacheManager;
			_initialDisplayCulture = _settingsService.DisplayCulture;
		}

		public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			// The base implementation uses RestorableStateAttribute and Reflection to save and restore state
			// If you do not use this attribute, do not invoke base impkementation to prevent execution this useless code.

			Languages = new List<Lang>(new[]
										{
											new Lang(),
											new Lang("en-US"),
											new Lang("ru-RU")
										});

			SelectedLang = Languages.FirstOrDefault(l => l.Name == _initialDisplayCulture) ?? Languages.FirstOrDefault();
			NeedAppRestart = false;
			HideEmptySubscriptions = _settingsService.HideEmptySubscriptions;

			ShowOrderItems = new List<ShowOrderItem>(new[]
			{
				new ShowOrderItem(true), 
				new ShowOrderItem(false) 
			});

			SelectedShowOrder = ShowOrderItems.Single(s => s.Value == _settingsService.ShowNewestFirst);

			IsCacheBusy = true;
			TotalCacheSize = await _cacheManager.GetTotalCacheSizeAsync();
			IsCacheBusy = false;
		}

		public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
		{
			// The base implementation uses RestorableStateAttribute and Reflection to save and restore state
			// If you do not use this attribute, do not invoke base impkementation to prevent execution this useless code.

			if (!suspending && _clearCacheComand != null)
				_clearCacheComand.Dispose();
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
		
		private void OnSelectedShowOrderChanged()
		{
			if (SelectedShowOrder.Value != _settingsService.ShowNewestFirst)
			{
				var eventTelemetry = new EventTelemetry(TelemetryEvents.ChangeShowOrder);
				eventTelemetry.Properties.Add("OldValue", _settingsService.ShowNewestFirst.ToString());
				eventTelemetry.Properties.Add("NewValue", SelectedShowOrder.Value.ToString());
				_telemetryClient.TrackEvent(eventTelemetry);
			}

			_settingsService.ShowNewestFirst = SelectedShowOrder.Value;
			_settingsService.Save();
		}

		private async void OnClearCache(object obj)
		{
			IsCacheBusy = true;

			await _cacheManager.ClearCacheAsync();
			TotalCacheSize = 0UL;

			IsCacheBusy = false;
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

	public class ShowOrderItem
	{
		public bool Value { get; set; }
		public string Title { get; set; }

		public ShowOrderItem(bool value)
		{
			Value = value;
			Title = value ? Strings.Resources.NewestFirstShowOrder : Strings.Resources.OldestFirstShowOrder;
		}
	}
}