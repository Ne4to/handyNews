using System;
using System.Collections.Generic;
using System.Linq;
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

		private readonly LinkedList<TagAction> _queue;
		private readonly object _queueLock = new object();
		private int _currentBusy = 0;

		public TagsManager(TagsManagerState state, [NotNull] ApiClient apiClient, [NotNull] TelemetryClient telemetryClient,
			[NotNull] NetworkManager networkManager)
		{
			if (apiClient == null) throw new ArgumentNullException("apiClient");
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			if (networkManager == null) throw new ArgumentNullException("networkManager");

			_apiClient = apiClient;
			_telemetryClient = telemetryClient;

			_queue = state != null ? new LinkedList<TagAction>(state.Actions) : new LinkedList<TagAction>();
			networkManager.NetworkChanged += networkManager_NetworkChanged;
		}

		void networkManager_NetworkChanged(object sender, NetworkChangedEventArgs e)
		{
			if (e.Connected)
				ProcessQueue();	
		}

		public TagsManagerState GetState()
		{
			TagAction[] actions;
			lock (_queueLock)
			{
				actions = _queue.ToArray();
			}

			return new TagsManagerState
			{
				Actions = actions
			};
		}

		private void AddItem(TagAction item)
		{
			lock (_queueLock)
			{
				_queue.AddLast(item);
			}

			ProcessQueue();
		}

		public void MarkAsRead(string id)
		{
			var item = new MarkAsReadTagAction
			{
				Id = id
			};

			AddItem(item);
		}

		public void MarkAsUnreadTagAction(string id)
		{
			var item = new MarkAsUnreadTagAction
			{
				Id = id
			};

			AddItem(item);
		}

		public void AddToStarred(string id)
		{
			var item = new MarkAsStarredTagAction
			{
				Id = id
			};

			AddItem(item);
		}

		public void RemoveFromStarred(string id)
		{
			var item = new MarkAsUnstarredTagAction
			{
				Id = id
			};

			AddItem(item);
		}

		private TagAction GetNext()
		{
			TagAction result = null;

			lock (_queueLock)
			{
				if (_queue.Count != 0)
				{
					result = _queue.First.Value;
					_queue.RemoveFirst();
				}
			}

			return result;
		}

		public async void ProcessQueue()
		{
			if (Interlocked.CompareExchange(ref _currentBusy, TrueValue, FalseValue) != FalseValue)
				return;
			
			TagAction action = null;

			try
			{
				while (true)
				{
					action = GetNext();
					if (action == null)
						return;

					await action.ExecuteAsync(_apiClient, _telemetryClient).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				if (action != null)
				{
					lock (_queueLock)
					{
						_queue.AddFirst(action);
					}					
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