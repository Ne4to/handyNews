using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.UI.Xaml.Navigation;
using Inoreader.Api;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Newtonsoft.Json.Linq;

namespace Inoreader.ViewModels.Pages
{
	public class StreamPageViewModel : ViewModel
	{
		private readonly ApiClient _apiClient;
		private readonly INavigationService _navigationService;
		private string _steamId;

		private string _title;
		private List<SteamItem> _items;

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


		public StreamPageViewModel(ApiClient apiClient, INavigationService navigationService)
		{
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			_apiClient = apiClient;
			_navigationService = navigationService;
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

		class AAA
		{
			public List<string> Items { get; set; }

			public AAA()
			{
				Items = new List<string>();
			}
		}
		static AAA testData = new AAA();

		private async void LoadData()
		{
			var stream = await _apiClient.GetStreamAsync(_steamId);

			Title = stream.title;

			var itemsQuery = from it in stream.items
				select new SteamItem
				{
					Published = UnixTimeStampToDateTime(it.published),
					Title = it.title,
					Content = it.summary.content,
				};

			Items = new List<SteamItem>(itemsQuery);
			testData.Items.AddRange(Items.Select(i => i.Content));

			var tt = JObject.FromObject(testData).ToString();
		}

		public static DateTimeOffset UnixTimeStampToDateTime(int unixTimeStamp)
		{
			var epochDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
			return epochDate.AddSeconds(unixTimeStamp);

			// Unix timestamp is seconds past epoch
			var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);			
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
			return dtDateTime;
		}
	}

	public class SteamItem
	{
		public DateTimeOffset Published { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
	}
}