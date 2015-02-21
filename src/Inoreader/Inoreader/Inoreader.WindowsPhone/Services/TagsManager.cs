using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Inoreader.Annotations;
using Inoreader.Api;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Inoreader.Services
{
	public class TagsManager
	{
		private const int TrueValue = 1;
		private const int FalseValue = 0;

		private readonly ApiClient _apiClient;
		private readonly TelemetryClient _telemetryClient;

		private readonly ConcurrentQueue<TagAction> _queue;
		private int _currentBusy = 0;

		public TagsManager(TagsManagerState state, [NotNull] ApiClient apiClient, [NotNull] TelemetryClient telemetryClient,
			[NotNull] NetworkManager networkManager)
		{
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			if (networkManager == null) throw new ArgumentNullException("networkManager");

			_apiClient = apiClient;
			_telemetryClient = telemetryClient;

			_queue = state != null ? new ConcurrentQueue<TagAction>(state.Actions) : new ConcurrentQueue<TagAction>();
			networkManager.NetworkChanged += networkManager_NetworkChanged;
		}

		void networkManager_NetworkChanged(object sender, NetworkChangedEventArgs e)
		{
			if (e.Connected)
				ProcessQueue();	
		}

		public TagsManagerState GetState()
		{
			return new TagsManagerState
			{
				Actions = _queue.ToArray()
			};
		}

		public void MarkAsRead(string id)
		{
			var item = new MarkAsReadTagAction
			{
				Id = id
			};

			_queue.Enqueue(item);
			ProcessQueue();
		}

		public void MarkAsUnreadTagAction(string id)
		{
			var item = new MarkAsUnreadTagAction
			{
				Id = id
			};

			_queue.Enqueue(item);
			ProcessQueue();
		}

		public void AddToStarred(string id)
		{
			var item = new MarkAsStarredTagAction
			{
				Id = id
			};

			_queue.Enqueue(item);
			ProcessQueue();
		}

		public void RemoveFromStarred(string id)
		{
			var item = new MarkAsUnstarredTagAction
			{
				Id = id
			};

			_queue.Enqueue(item);
			ProcessQueue();
		}

		public async void ProcessQueue()
		{
			if (Interlocked.CompareExchange(ref _currentBusy, TrueValue, FalseValue) != FalseValue)
				return;
			
			TagAction action = null;

			try
			{
				while (_queue.TryDequeue(out action))
				{
					await action.ExecuteAsync(_apiClient, _telemetryClient).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				if (action != null)
				{
					_queue.Enqueue(action);
				}

				_telemetryClient.TrackExceptionFull(ex);
			}
			finally
			{
				_currentBusy = FalseValue;
			}
		}
	}

	[DataContract]
	public class TagsManagerState
	{
		[DataMember]
		public TagAction[] Actions { get; set; }
	}

	[DataContract]
	public abstract class TagAction
	{
		public abstract Task ExecuteAsync(ApiClient apiClient, TelemetryClient telemetryClient);
	}

	[DataContract]
	public class MarkAsReadTagAction : TagAction
	{
		[DataMember]
		public string Id { get; set; }

		public override Task ExecuteAsync(ApiClient apiClient, TelemetryClient telemetryClient)
		{
			var eventTelemetry = new EventTelemetry(TelemetryEvents.MarkAsRead);
			eventTelemetry.Properties.Add("AsRead", true.ToString());
			telemetryClient.TrackEvent(eventTelemetry);

			return apiClient.AddTagAsync(SpecialTags.MarkItemAsRead, Id);
		}
	}

	[DataContract]
	public class MarkAsUnreadTagAction : TagAction
	{
		[DataMember]
		public string Id { get; set; }

		public override Task ExecuteAsync(ApiClient apiClient, TelemetryClient telemetryClient)
		{
			var eventTelemetry = new EventTelemetry(TelemetryEvents.MarkAsRead);
			eventTelemetry.Properties.Add("AsRead", false.ToString());
			telemetryClient.TrackEvent(eventTelemetry);

			return apiClient.RemoveTagAsync(SpecialTags.MarkItemAsRead, Id);
		}
	}

	[DataContract]
	public class MarkAsStarredTagAction : TagAction
	{
		[DataMember]
		public string Id { get; set; }

		public override Task ExecuteAsync(ApiClient apiClient, TelemetryClient telemetryClient)
		{
			var eventTelemetry = new EventTelemetry(TelemetryEvents.MarkAsStarred);
			eventTelemetry.Properties.Add("Starred", true.ToString());
			telemetryClient.TrackEvent(eventTelemetry);

			return apiClient.AddTagAsync(SpecialTags.MarkItemAsStarred, Id);
		}
	}

	[DataContract]
	public class MarkAsUnstarredTagAction : TagAction
	{
		[DataMember]
		public string Id { get; set; }

		public override Task ExecuteAsync(ApiClient apiClient, TelemetryClient telemetryClient)
		{
			var eventTelemetry = new EventTelemetry(TelemetryEvents.MarkAsStarred);
			eventTelemetry.Properties.Add("Starred", false.ToString());
			telemetryClient.TrackEvent(eventTelemetry);

			return apiClient.RemoveTagAsync(SpecialTags.MarkItemAsStarred, Id);
		}
	}
}