﻿using System;
using System.Threading;
using System.Threading.Tasks;
using handyNews.Domain.Models;
using handyNews.Domain.Services;
using handyNews.Domain.Services.Interfaces;
using JetBrains.Annotations;

namespace handyNews.Inoreader
{
    public class TagsManager : ITagsManager
    {
        private readonly InoreaderClient _inoreaderClient;
        private readonly LocalStorageManager _localStorageManager;
        private readonly ITelemetryManager _telemetryManager;

        private int _currentBusy = FalseValue;
        private const int TrueValue = 1;
        private const int FalseValue = 0;

        public TagsManager([NotNull] InoreaderClient inoreaderClient,
            [NotNull] ITelemetryManager telemetryManager,
            [NotNull] INetworkManager networkManager,
            [NotNull] LocalStorageManager localStorageManager)
        {
            if (inoreaderClient == null)
            {
                throw new ArgumentNullException(nameof(inoreaderClient));
            }
            if (telemetryManager == null)
            {
                throw new ArgumentNullException(nameof(telemetryManager));
            }
            if (networkManager == null)
            {
                throw new ArgumentNullException(nameof(networkManager));
            }
            if (localStorageManager == null)
            {
                throw new ArgumentNullException(nameof(localStorageManager));
            }

            _inoreaderClient = inoreaderClient;
            _telemetryManager = telemetryManager;
            _localStorageManager = localStorageManager;

            networkManager.NetworkChanged += networkManager_NetworkChanged;
        }

        public void AddTag(string itemId, string tag)
        {
            if (itemId == null)
            {
                throw new ArgumentNullException(nameof(itemId));
            }

            switch (tag)
            {
                case SpecialTags.Read:
                    MarkAsRead(itemId);
                    return;

                case SpecialTags.Starred:
                    AddToStarred(itemId);
                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(tag));
            }
        }

        public void RemoveTag(string itemId, string tag)
        {
            if (itemId == null)
            {
                throw new ArgumentNullException(nameof(itemId));
            }

            switch (tag)
            {
                case SpecialTags.Read:
                    MarkAsUnreadTagAction(itemId);
                    return;

                case SpecialTags.Starred:
                    RemoveFromStarred(itemId);
                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(tag));
            }
        }

        private void networkManager_NetworkChanged(object sender, NetworkChangedEventArgs e)
        {
            if (e.Connected)
            {
                ProcessQueue();
            }
        }

        private async void MarkAsRead(string id)
        {
            await _localStorageManager.SetCachedItemAsReadAsync(id, true)
                .ConfigureAwait(false);
            await AddTagInternalAsync(id, SpecialTags.Read)
                .ConfigureAwait(false);
        }

        private async void MarkAsUnreadTagAction(string id)
        {
            await _localStorageManager.SetCachedItemAsReadAsync(id, false)
                .ConfigureAwait(false);
            await RemoveTagInternalAsync(id, SpecialTags.Read)
                .ConfigureAwait(false);
        }

        private async void AddToStarred(string id)
        {
            await _localStorageManager.SetCachedItemAsStarredAsync(id, true)
                .ConfigureAwait(false);
            await AddTagInternalAsync(id, SpecialTags.Starred)
                .ConfigureAwait(false);
        }

        private async void RemoveFromStarred(string id)
        {
            await _localStorageManager.SetCachedItemAsStarredAsync(id, false)
                .ConfigureAwait(false);
            await RemoveTagInternalAsync(id, SpecialTags.Starred)
                .ConfigureAwait(false);
        }

        private async Task AddTagInternalAsync(string id, string tag)
        {
            try
            {
                await _inoreaderClient.AddTagAsync(tag, id)
                    .ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                _telemetryManager.TrackError(ex);
            }

            try
            {
                _localStorageManager.AddTagAction(id, tag, TagActionKind.Add);
            }
            catch (Exception ex)
            {
                _telemetryManager.TrackError(ex);
            }
        }

        private async Task RemoveTagInternalAsync(string id, string tag)
        {
            try
            {
                await _inoreaderClient.RemoveTagAsync(tag, id)
                    .ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                _telemetryManager.TrackError(ex);
            }

            try
            {
                _localStorageManager.AddTagAction(id, tag, TagActionKind.Remove);
            }
            catch (Exception ex)
            {
                _telemetryManager.TrackError(ex);
            }
        }

        public async void ProcessQueue()
        {
            if (Interlocked.CompareExchange(ref _currentBusy, TrueValue, FalseValue) != FalseValue)
            {
                return;
            }

            try
            {
                while (true)
                {
                    var action = _localStorageManager.GetNextTagAction();
                    if (action == null)
                    {
                        return;
                    }

                    switch (action.Kind)
                    {
                        case TagActionKind.Add:
                            await _inoreaderClient.AddTagAsync(action.Tag, action.ItemId)
                                .ConfigureAwait(false);
                            break;

                        case TagActionKind.Remove:
                            await _inoreaderClient.RemoveTagAsync(action.Tag, action.ItemId)
                                .ConfigureAwait(false);
                            break;
                    }

                    _localStorageManager.DeleteTagAction(action.Id);
                }
            }
            catch (Exception ex)
            {
                _telemetryManager.TrackError(ex);
            }
            finally
            {
                _currentBusy = FalseValue;
            }
        }
    }
}