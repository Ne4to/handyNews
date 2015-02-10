using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using Inoreader.Api;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Newtonsoft.Json.Linq;

namespace Inoreader.ViewModels.Pages
{
	public class StreamPageViewModel : ViewModel
	{
		private readonly ApiClient _apiClient;
		private readonly INavigationService _navigationService;
		private readonly TelemetryClient _telemetryClient;
		private string _steamId;

		private string _title;
		private List<SteamItem> _items;
		private bool _isBusy;

		public string Title
		{
			get { return _title; }
			set { SetProperty(ref _title, value); }
		}

		public List<SteamItem> Items
		{
			get { return _items; }
			set { SetProperty(ref _items, value); }
		}

		public bool IsBusy
		{
			get { return _isBusy; }
			set { SetProperty(ref _isBusy, value); }
		}

		public StreamPageViewModel(ApiClient apiClient, INavigationService navigationService, TelemetryClient telemetryClient)
		{
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (navigationService == null) throw new ArgumentNullException("navigationService");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			
			_apiClient = apiClient;
			_navigationService = navigationService;
			_telemetryClient = telemetryClient;
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

			_steamId = (string) navigationParameter;
			LoadData();
		}

		public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
		{
			base.OnNavigatedFrom(viewModelState, suspending);
		}
#if DEBUG
		class AAA
		{
			public List<string> Items { get; set; }

			public AAA()
			{
				Items = new List<string>();
			}
		}
		static AAA testData = new AAA();
#endif
		private async void LoadData()
		{
			IsBusy = true;

			Exception error = null;			
			try
			{
				var stopwatch = Stopwatch.StartNew();

				var stream = await _apiClient.GetStreamAsync(_steamId);

				stopwatch.Stop();
				_telemetryClient.TrackMetric(TemetryMetrics.GetStreamResponseTime, stopwatch.Elapsed.TotalSeconds);

				Title = stream.title;

				var itemsQuery = from it in stream.items
					select new SteamItem
					{
						Published = UnixTimeStampToDateTime(it.published),
						Title = it.title,
						Content = it.summary.content,
					};

				Items = new List<SteamItem>(itemsQuery);
			}
			catch (Exception ex)
			{
				error = ex;
				_telemetryClient.TrackException(ex);
			}
			finally
			{
				IsBusy = false;
			}

			if (error != null)
			{
				MessageDialog msgbox = new MessageDialog(error.Message, Strings.Resources.ErrorDialogTitle);
				await msgbox.ShowAsync();
			}
#if DEBUG
			testData.Items.AddRange(Items.Select(i => i.Content));
			var tt = JObject.FromObject(testData).ToString();
#endif
		}

		public static DateTimeOffset UnixTimeStampToDateTime(int unixTimeStamp)
		{
			var epochDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
			return epochDate.AddSeconds(unixTimeStamp);
		}
	}

	public class SteamItem
	{
		public DateTimeOffset Published { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
	}
}