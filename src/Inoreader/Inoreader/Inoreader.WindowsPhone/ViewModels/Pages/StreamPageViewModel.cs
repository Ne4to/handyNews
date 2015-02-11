using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using Inoreader.Api;
using Inoreader.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Newtonsoft.Json.Linq;

namespace Inoreader.ViewModels.Pages
{
	public class StreamPageViewModel : ViewModel
	{
		#region Fields

		private readonly ApiClient _apiClient;
		private readonly INavigationService _navigationService;
		private readonly TelemetryClient _telemetryClient;
		private string _steamId;

		private string _title;
		private List<SteamItem> _items;
		private bool _isBusy;
		private bool _currentItemRead;
		private SteamItem _currentItem;

		private ICommand _itemsScrollCommand;
		private ICommand _selectItemCommand;

		#endregion

		#region Properties

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

		public bool CurrentItemRead
		{
			get { return _currentItemRead; }
			set
			{
				if (SetProperty(ref _currentItemRead, value))
					OnCurrentItemReadChanged(value);
			}
		}

		#endregion

		#region Commands

		public ICommand ItemsScrollCommand
		{
			get { return _itemsScrollCommand ?? (_itemsScrollCommand = new DelegateCommand<object>(OnItemsScroll)); }
		}

		public ICommand SelectItemCommand
		{
			get { return _selectItemCommand ?? (_selectItemCommand = new DelegateCommand<object>(OnSelectItem)); }
		}

		#endregion


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

			_steamId = (string)navigationParameter;
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
									 Id = it.id,
									 Published = UnixTimeStampToDateTime(it.published),
									 Title = it.title,
									 Content = it.summary.content,
								 };

				var steamItems = new List<SteamItem>(itemsQuery);
				steamItems.Add(new EmptySpaceSteamItem());
				Items = steamItems;

				_currentItem = Items.FirstOrDefault();
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

		private void OnItemsScroll(object obj)
		{
			var items = (object[])obj;
			foreach (SteamItem item in items)
			{
				if (item is EmptySpaceSteamItem)
					continue;

				if (!item.NeedSetReadExplicitly && item.Unread)
				{
					item.Unread = false;
					MarkAsRead(item.Id, true);
				}
			}

			var firstItem = (SteamItem)items[0];
			if (firstItem is EmptySpaceSteamItem)
				return;

			_currentItem = firstItem;
			SetCurrentItemRead(!_currentItem.Unread);
		}

		private void OnSelectItem(object obj)
		{
			var item = obj as SteamItem;
			if (item != null && !(item is EmptySpaceSteamItem))
			{
				_currentItem = item;
				SetCurrentItemRead(!_currentItem.Unread);
			}
		}

		private async void MarkAsRead(string id, bool newValue)
		{
			var eventTelemetry = new EventTelemetry(TelemetryEvents.MarkAsRead);
			eventTelemetry.Properties.Add("AsRead", newValue.ToString());			
			_telemetryClient.TrackEvent(eventTelemetry);		
			
			if (newValue)
			{
				await _apiClient.AddTagAsync(SpecialTags.MarkItemAsRead, id);
			}
			else
			{
				await _apiClient.RemoveTagAsync(SpecialTags.MarkItemAsRead, id);
			}
		}

		private void SetCurrentItemRead(bool newValue)
		{			
			_currentItemRead = newValue;
			OnPropertyChanged("CurrentItemRead");
		}

		private void OnCurrentItemReadChanged(bool newValue)
		{
			if (_currentItem == null || _currentItem is EmptySpaceSteamItem)
				return;

			if (newValue)
			{
				_currentItem.Unread = false;
				_currentItem.NeedSetReadExplicitly = false;
				SetCurrentItemRead(true);
				MarkAsRead(_currentItem.Id, true);
			}
			else
			{
				_currentItem.Unread = true;
				_currentItem.NeedSetReadExplicitly = true;
				SetCurrentItemRead(false);
				MarkAsRead(_currentItem.Id, false);
			}
		}
	}

	public class SteamItem : BindableBase
	{
		private bool _unread = true;
		private bool _needSetReadExplicitly;

		public string Id { get; set; }
		public DateTimeOffset Published { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }

		public bool Unread
		{
			get { return _unread; }
			set { SetProperty(ref _unread, value); }
		}

		public bool NeedSetReadExplicitly
		{
			get { return _needSetReadExplicitly; }
			set { SetProperty(ref _needSetReadExplicitly, value); }
		}
	}

	public class EmptySpaceSteamItem : SteamItem
	{
	}
}