using System;
using System.Threading;
using System.Threading.Tasks;
using Inoreader.Annotations;
using Inoreader.Api;
using Microsoft.ApplicationInsights;

namespace Inoreader.Services
{
	public class TagsManager
	{
		private const int TrueValue = 1;
		private const int FalseValue = 0;

		private readonly ApiClient _apiClient;
		private readonly TelemetryClient _telemetryClient;
		private readonly LocalStorageManager _localStorageManager;

		private int _currentBusy = FalseValue;

		public TagsManager([NotNull] ApiClient apiClient,
			[NotNull] TelemetryClient telemetryClient,
			[NotNull] NetworkManager networkManager,
			[NotNull] LocalStorageManager localStorageManager)
		{
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			if (networkManager == null) throw new ArgumentNullException("networkManager");
			if (localStorageManager == null) throw new ArgumentNullException("localStorageManager");

			_apiClient = apiClient;
			_telemetryClient = telemetryClient;
			_localStorageManager = localStorageManager;

			networkManager.NetworkChanged += networkManager_NetworkChanged;
		}

		void networkManager_NetworkChanged(object sender, NetworkChangedEventArgs e)
		{
			if (e.Connected)
				ProcessQueue();
		}

		public async void MarkAsRead(string id)
		{
			await _localStorageManager.SetCachedItemAsReadAsync(id, true).ConfigureAwait(false);
			await AddTagInternalAsync(id, SpecialTags.Read).ConfigureAwait(false);
		}

		public async void MarkAsUnreadTagAction(string id)
		{
			await _localStorageManager.SetCachedItemAsReadAsync(id, false).ConfigureAwait(false);
			await RemoveTagInternalAsync(id, SpecialTags.Read).ConfigureAwait(false);
		}

		public async void AddToStarred(string id)
		{
			await _localStorageManager.SetCachedItemAsStarredAsync(id, true).ConfigureAwait(false);
			await AddTagInternalAsync(id, SpecialTags.Starred).ConfigureAwait(false);
		}

		public async void RemoveFromStarred(string id)
		{
			await _localStorageManager.SetCachedItemAsStarredAsync(id, false).ConfigureAwait(false);
			await RemoveTagInternalAsync(id, SpecialTags.Starred).ConfigureAwait(false);
		}

		private async Task AddTagInternalAsync(string id, string tag)
		{
			try
			{
				await _apiClient.AddTagAsync(tag, id).ConfigureAwait(false);
				return;
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackException(ex);
			}

			try
			{
				_localStorageManager.AddTagAction(id, tag, TagActionKind.Add);
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackException(ex);
			}
		}

		private async Task RemoveTagInternalAsync(string id, string tag)
		{
			try
			{
				await _apiClient.RemoveTagAsync(tag, id).ConfigureAwait(false);
				return;
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackException(ex);
			}

			try
			{
				_localStorageManager.AddTagAction(id, tag, TagActionKind.Remove);
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackException(ex);
			}
		}

		public async void ProcessQueue()
		{
			if (Interlocked.CompareExchange(ref _currentBusy, TrueValue, FalseValue) != FalseValue)
				return;

			try
			{
				while (true)
				{
					var action = _localStorageManager.GetNextTagAction();
					if (action == null)
						return;

					switch (action.Kind)
					{
						case TagActionKind.Add:
							await _apiClient.AddTagAsync(action.Tag, action.ItemId).ConfigureAwait(false);
							break;

						case TagActionKind.Remove:
							await _apiClient.RemoveTagAsync(action.Tag, action.ItemId).ConfigureAwait(false);
							break;
					}

					_localStorageManager.DeleteTagAction(action.Id);
				}
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackExceptionFull(ex);
			}
			finally
			{
				_currentBusy = FalseValue;
			}
		}
	}
}