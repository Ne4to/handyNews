using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Inoreader.Services;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.ViewModels.Pages
{
	public class SettingsPageViewModel : ViewModel
	{
		#region Fields

		private readonly AppSettingsService _settingsService;
		private readonly string _initialDisplayCulture;

		private List<Lang> _languages;
		private Lang _selectedLang;
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

		public bool NeedAppRestart
		{
			get { return _needAppRestart; }
			set { SetProperty(ref _needAppRestart, value); }
		}

		#endregion

		public SettingsPageViewModel(AppSettingsService settingsService)
		{
			if (settingsService == null) throw new ArgumentNullException("settingsService");
			_settingsService = settingsService;
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
		}

		private void OnSelectedLangChanged()
		{
			_settingsService.DisplayCulture = SelectedLang.Name;
			_settingsService.Save();

			NeedAppRestart = SelectedLang.Name != _initialDisplayCulture;
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