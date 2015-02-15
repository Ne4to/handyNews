using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Inoreader.Annotations;
using Inoreader.Models;
using Inoreader.Models.States;
using Microsoft.ApplicationInsights;

namespace Inoreader.Services
{
	public class CacheManager
	{
		#region Constants

		private const string CacheFolderName = "Cache";
		private const string SubscriptionsFileName = "Subscriptions.data";
		private const string StreamIndexFileName = "StreamIndex.data";
		private const string TagsManagerStateFileName = "TagsManagerState.data";

		#endregion


		#region Fields

		private readonly TelemetryClient _telemetryClient;
		private readonly StorageFolder _rootCacheFolder;
		private Dictionary<string, string> _streamIndex;
		private readonly DataContractSerializer _subscriptionsSerializer;
		private readonly DataContractSerializer _streamIndexSerializer;
		private readonly DataContractSerializer _streamSerializer;
		private readonly DataContractSerializer _tagsManagerStateSerializer;

		#endregion

		public CacheManager([NotNull] TelemetryClient telemetryClient)
		{
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			_telemetryClient = telemetryClient;

			_rootCacheFolder = ApplicationData.Current.LocalCacheFolder;

			var knownTypes = new[]
			{
				typeof(TreeItemBase),
				typeof(SubscriptionItem),
				typeof(CategoryItem)
			};
			_subscriptionsSerializer = new DataContractSerializer(typeof(List<TreeItemBase>), knownTypes);

			_streamIndexSerializer = new DataContractSerializer(typeof(Dictionary<string, string>));

			knownTypes = new[]
			{
				typeof(StreamItem),
				typeof(EmptySpaceStreamItem)			
			};
			_streamSerializer = new DataContractSerializer(typeof(StreamItemCollectionState), knownTypes);

			knownTypes = new[]
			{
				typeof(TagAction),
				typeof(MarkAsReadTagAction),				
				typeof(MarkAsUnreadTagAction)				
			};
			_tagsManagerStateSerializer = new DataContractSerializer(typeof(TagsManagerState), knownTypes);
		}

		public async Task InitAsync()
		{
			_streamIndex = await LoadAsync<Dictionary<string, string>>(StreamIndexFileName, _streamIndexSerializer).ConfigureAwait(false);
			if (_streamIndex == null)
				_streamIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		public async Task<ulong> GetTotalCacheSizeAsync()
		{
			try
			{
				var cacheFolder = await _rootCacheFolder.GetFolderAsync(CacheFolderName).AsTask().ConfigureAwait(false);
				var files = await cacheFolder.GetFilesAsync().AsTask().ConfigureAwait(false);

				var total = 0UL;
				foreach (var storageFile in files)
				{
					var basicProperties = await storageFile.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);
					total += basicProperties.Size;
				}

				return total;
			}
			catch (FileNotFoundException)
			{
				return 0UL;
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackException(ex);
				return 0UL;
			}
		}
		
		public async Task ClearCacheAsync()
		{
			try
			{
				var cacheFolder = await _rootCacheFolder.GetFolderAsync(CacheFolderName).AsTask().ConfigureAwait(false);
				var files = await cacheFolder.GetFilesAsync().AsTask().ConfigureAwait(false);
				
				foreach (var storageFile in files)
				{
					await storageFile.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask().ConfigureAwait(false);
				}
			}
			catch (FileNotFoundException)
			{				
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackException(ex);
			}
		}

		public Task<bool> SaveSubscriptionsAsync(List<TreeItemBase> items)
		{
			return SaveAsync(items, SubscriptionsFileName, _subscriptionsSerializer);
		}

		public Task<List<TreeItemBase>> LoadSubscriptionsAsync()
		{
			_telemetryClient.TrackEvent(TelemetryEvents.LoadSubscriptionsFromCache);
			return LoadAsync<List<TreeItemBase>>(SubscriptionsFileName, _subscriptionsSerializer);
		}

		public async Task<bool> SaveStreamAsync(StreamItemCollectionState streamState)
		{
			string fileName;
			if (!_streamIndex.TryGetValue(streamState.StreamId, out fileName))
			{
				fileName = Guid.NewGuid().ToString("N");
				_streamIndex.Add(streamState.StreamId, fileName);

				// index was not updated, without index we will never found the data
				if (!await SaveAsync(_streamIndex, StreamIndexFileName, _streamIndexSerializer).ConfigureAwait(false))
					return false;
			}

			return await SaveAsync(streamState, fileName, _streamSerializer).ConfigureAwait(false);
		}

		public Task<StreamItemCollectionState> LoadStreamAsync(string streamId)
		{
			_telemetryClient.TrackEvent(TelemetryEvents.LoadStreamFromCache);
			string fileName;
			if (!_streamIndex.TryGetValue(streamId, out fileName))
			{
				return Task.FromResult<StreamItemCollectionState>(null);
			}

			return LoadAsync<StreamItemCollectionState>(fileName, _streamSerializer);
		}

		public Task<bool> SaveTagsManagerStateAsync(TagsManagerState state)
		{
			return SaveAsync(state, TagsManagerStateFileName, _tagsManagerStateSerializer);
		}

		public Task<TagsManagerState> LoadTagsManagerStateAsync()
		{
			return LoadAsync<TagsManagerState>(TagsManagerStateFileName, _tagsManagerStateSerializer);
		}

		#region Utilities

		private async Task<bool> SaveAsync<T>(T data, string fileName, DataContractSerializer serializer)
		{
			try
			{
				var cacheFolder = await _rootCacheFolder.CreateFolderAsync(CacheFolderName, CreationCollisionOption.OpenIfExists)
							.AsTask()
							.ConfigureAwait(false);
				var file = await cacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting)
							.AsTask()
							.ConfigureAwait(false);

				using (var transaction = await file.OpenTransactedWriteAsync().AsTask().ConfigureAwait(false))
				{
					var stream = transaction.Stream.AsStreamForWrite();
					serializer.WriteObject(stream, data);
					await transaction.CommitAsync().AsTask().ConfigureAwait(false);
				}

				return true;
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackException(ex);
				return false;
			}
		}

		private async Task<T> LoadAsync<T>(string fileName, DataContractSerializer serializer)
			where T : class
		{
			try
			{
				var cacheFolder = await _rootCacheFolder.GetFolderAsync(CacheFolderName).AsTask().ConfigureAwait(false);
				var file = await cacheFolder.GetFileAsync(fileName).AsTask().ConfigureAwait(false);

				using (var stream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
				{
					return (T)serializer.ReadObject(stream);
				}
			}
			catch (FileNotFoundException)
			{
				return null;
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackException(ex);
				return null;
			}
		}

		#endregion
	}
}