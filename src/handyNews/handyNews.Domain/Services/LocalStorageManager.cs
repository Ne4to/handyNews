﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using handyNews.Domain.Models;
using handyNews.Domain.Models.SQLiteStorage;
using handyNews.Domain.Models.States;
using JetBrains.Annotations;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;

namespace handyNews.Domain.Services
{
    public class LocalStorageManager
    {
        private const string DatabaseFilename = "localdata.db";
        private const long AddMaxCountColumnSchemaVersion = 10L;

        public LocalStorageManager()
        {
            Init();
        }

        private SQLiteConnection GetConnection()
        {
            var fullPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseFilename);
            return new SQLiteConnection(new SQLitePlatformWinRT(), fullPath);
        }

        private void Init()
        {
            using (var connection = GetConnection())
            {
                EnableForeignKeys(connection);

                connection.Execute(@"CREATE TABLE IF NOT EXISTS SAVED_STREAM_ITEM (ID TEXT PRIMARY KEY NOT NULL, 
																			TITLE TEXT, 
																			PUBLISHED TEXT, 
																			WEBURI TEXT, 
																			SHORT_CONTENT TEXT, 
																			CONTENT TEXT, 
																			IMAGE_FOLDER TEXT);");

                connection.Execute(
                    @"CREATE TABLE IF NOT EXISTS TAG_ACTION (ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
							ITEM_ID TEXT,
							TAG TEXT,
							ACTION_KIND INTEGER);");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS SUB_ITEM (ID TEXT PRIMARY KEY NOT NULL, 
										SORT_ID TEXT, TITLE TEXT, UNREAD_COUNT INTEGER, URL TEXT, HTML_URL TEXT, ICON_URL TEXT, FIRST_ITEM_MSEC INTEGER);");

                connection.Execute(@"CREATE TABLE IF NOT EXISTS SUB_CAT (ID TEXT PRIMARY KEY NOT NULL, 
										SORT_ID TEXT, TITLE TEXT, UNREAD_COUNT INTEGER);");


                connection.Execute(@"CREATE TABLE IF NOT EXISTS SUB_CAT_SUB_ITEM (CAT_ID TEXT, ITEM_ID TEXT);");

                connection.Execute(
                    @"CREATE TABLE IF NOT EXISTS STREAM_COLLECTION (STREAM_ID TEXT PRIMARY KEY NOT NULL, CONTINUATION TEXT, SHOW_NEWEST_FIRST INTEGER, STREAM_TIMESTAMP INTEGER, FAULT INTEGER);");

                connection.Execute(
                    @"CREATE TABLE IF NOT EXISTS STREAM_ITEM (ID TEXT PRIMARY KEY NOT NULL, STREAM_ID TEXT REFERENCES STREAM_COLLECTION(STREAM_ID) ON DELETE CASCADE, PUBLISHED TEXT, TITLE TEXT, WEB_URI TEXT, CONTENT TEXT, UNREAD INTEGER, NEED_SET_READ_EXPLICITLY INTEGER, IS_SELECTED INTEGER, STARRED INTEGER, SAVED INTEGER);");

                if (GetSchemaVersion(connection) < AddMaxCountColumnSchemaVersion)
                {
                    connection.Execute(@"ALTER TABLE SUB_CAT ADD COLUMN IS_MAX_COUNT INTEGER NOT NULL DEFAULT 0;");
                    connection.Execute(@"ALTER TABLE SUB_ITEM ADD COLUMN IS_MAX_COUNT INTEGER NOT NULL DEFAULT 0;");
                    SetSchemaVersion(connection, AddMaxCountColumnSchemaVersion);
                }
            }
        }

        private void EnableForeignKeys(SQLiteConnection connection)
        {
            connection.Execute(@"PRAGMA foreign_keys = ON;");
        }

        private long GetSchemaVersion(SQLiteConnection connection)
        {
            return connection.ExecuteScalar<long>(@"PRAGMA main.user_version;");
        }

        private void SetSchemaVersion(SQLiteConnection connection, long version)
        {
            connection.Execute($@"PRAGMA main.user_version = {version};");
        }

        public void Save([NotNull] SavedStreamItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            using (var connection = GetConnection())
            {
                var command =
                    connection.CreateCommand(
                        @"INSERT OR REPLACE INTO SAVED_STREAM_ITEM(ID, TITLE, PUBLISHED, WEBURI, SHORT_CONTENT, CONTENT, IMAGE_FOLDER) 
																			VALUES(@ID, @TITLE, @PUBLISHED, @WEBURI, @SHORT_CONTENT, @CONTENT, @IMAGE_FOLDER);");

                command.Bind("@ID", item.Id);
                command.Bind("@TITLE", item.Title);
                command.Bind("@PUBLISHED", item.Published.ToString("O"));
                command.Bind("@WEBURI", item.WebUri);
                command.Bind("@SHORT_CONTENT", item.ShortContent);
                command.Bind("@CONTENT", item.Content);
                command.Bind("@IMAGE_FOLDER", item.ImageFolder);

                command.ExecuteNonQuery();

                //// Resets the statement, to that it can be used again (with different parameters).
                //statement.Reset();
                //statement.ClearBindings();
            }
        }

        public List<SavedStreamItem> LoadSavedStreamItems()
        {
            var result = new List<SavedStreamItem>();

            using (var connection = GetConnection())
            {
                throw new NotImplementedException();
                //using (var statement = connection.Query(@"SELECT ID, TITLE, PUBLISHED, WEBURI, SHORT_CONTENT, CONTENT, IMAGE_FOLDER FROM SAVED_STREAM_ITEM;"))
                //{
                //    while (statement.Step() == SQLiteResult.ROW)
                //    {
                //        var item = new SavedStreamItem
                //        {
                //            Id = (string)statement[0],
                //            Title = (string)statement[1],
                //            Published = DateTimeOffset.Parse((string)statement[2]),
                //            WebUri = (string)statement[3],
                //            ShortContent = (string)statement[4],
                //            Content = (string)statement[5],
                //            ImageFolder = (string)statement[6],
                //        };

                //        result.Add(item);
                //    }
                //}
            }

            return result;
        }

        public void DeleteSavedStreamItem([NotNull] string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            using (var connection = GetConnection())
            {
                throw new NotImplementedException();
                //using (var statement = connection.Prepare(@"DELETE FROM SAVED_STREAM_ITEM WHERE ID = @ID;"))
                //{
                //    statement.Bind("@ID", id);
                //    statement.Step();
                //}
            }
        }

        public void AddTagAction(string itemId, string tag, TagActionKind kind)
        {
            using (var connection = GetConnection())
            {
                throw new NotImplementedException();
                //using (
                //    var statement =
                //        connection.Prepare(@"INSERT INTO TAG_ACTION(ITEM_ID, TAG, ACTION_KIND) VALUES(@ITEM_ID, @TAG, @ACTION_KIND);"))
                //{
                //    statement.Bind("@ITEM_ID", itemId);
                //    statement.Bind("@TAG", tag);
                //    statement.Bind("@ACTION_KIND", (int)kind);

                //    statement.Step();
                //}
            }
        }

        public TagAction GetNextTagAction()
        {
            using (var connection = GetConnection())
            {
                throw new NotImplementedException();
                //using (var statement = connection.Prepare(@"SELECT ID, ITEM_ID, TAG, ACTION_KIND FROM TAG_ACTION ORDER BY ID LIMIT 1;"))
                //{
                //    if (statement.Step() != SQLiteResult.ROW)
                //        return null;

                //    var item = new TagAction
                //    {
                //        Id = statement.GetInteger(0),
                //        ItemId = statement.GetText(1),
                //        Tag = statement.GetText(2),
                //        Kind = (TagActionKind)statement.GetInteger(3)
                //    };

                //    return item;
                //}
            }
        }

        public void DeleteTagAction(long id)
        {
            using (var connection = GetConnection())
            {
                throw new NotImplementedException();
                //using (var statement = connection.Prepare(@"DELETE FROM TAG_ACTION WHERE ID = @ID;"))
                //{
                //    statement.Bind("@ID", id);
                //    statement.Step();
                //}
            }
        }

        public Task SaveSubscriptionsAsync(List<Feed> items)
        {
            throw new NotImplementedException();
            //    var cats = items.OfType<CategoryItem>()
            //                    .ToArray();
            //    var subItems = items.OfType<SubscriptionItem>()
            //                        .Union(cats.SelectMany(c => c.Subscriptions))
            //                        .ToArray();
            //    var catItemsLinks = (from c in cats
            //        from s in c.Subscriptions
            //        select new Tuple<string, string>(c.Id, s.Id)).ToArray();

            //    return Task.Run(() =>
            //                    {
            //                        using (var connection = GetConnection())
            //                        {
            //                            connection.BeginTransaction();

            //                            ClearSubscriptions(connection);
            //                            //SaveSubscriptionCategories(cats, connection);
            //                            SaveSubscriptionItems(subItems, connection);
            //                            //SaveSubscriptionLinks(catItemsLinks, connection);

            //                            connection.Commit();
            //                        }
            //                    });
        }

        private void ClearSubscriptions(SQLiteConnection connection)
        {
            connection.Execute("DELETE FROM SUB_CAT_SUB_ITEM;");
            connection.Execute("DELETE FROM SUB_CAT;");
            connection.Execute("DELETE FROM SUB_ITEM;");
        }

        private void SaveSubscriptionCategories(Feed[] cats, SQLiteConnection connection)
        {
            if (cats.Length == 0)
            {
                return;
            }
            throw new NotImplementedException();
            //using (var statement = connection.Prepare(@"INSERT INTO SUB_CAT(ID, SORT_ID, TITLE, UNREAD_COUNT, IS_MAX_COUNT) 
            //							VALUES(@ID, @SORT_ID, @TITLE, @UNREAD_COUNT, @IS_MAX_COUNT);"))
            //{
            //    foreach (var categoryItem in cats)
            //    {
            //        statement.Bind("@ID", categoryItem.Id);
            //        statement.Bind("@SORT_ID", categoryItem.SortId);
            //        statement.Bind("@TITLE", categoryItem.Title);
            //        statement.Bind("@UNREAD_COUNT", categoryItem.UnreadCount);
            //        statement.Bind("@IS_MAX_COUNT", categoryItem.ApproxUnreadCount ? 1 : 0);

            //        statement.Step();

            //        // Resets the statement, to that it can be used again (with different parameters).
            //        statement.Reset();
            //        statement.ClearBindings();
            //    }
            //}
        }

        private void SaveSubscriptionItems(Feed[] subItems, SQLiteConnection connection)
        {
            if (subItems.Length == 0)
            {
                return;
            }

            var skipIdList = new List<string>(subItems.Length);

            var statement =
                connection.CreateCommand(
                    "INSERT INTO SUB_ITEM(ID, SORT_ID, TITLE, UNREAD_COUNT, URL, HTML_URL, ICON_URL, FIRST_ITEM_MSEC, IS_MAX_COUNT) VALUES(@ID, @SORT_ID, @TITLE, @UNREAD_COUNT, @URL, @HTML_URL, @ICON_URL, @FIRST_ITEM_MSEC, @IS_MAX_COUNT);");

            foreach (var item in subItems)
            {
                if (skipIdList.Contains(item.Id))
                {
                    continue;
                }

                statement.Bind("@ID", item.Id);
                statement.Bind("@SORT_ID", item.SortId);
                statement.Bind("@TITLE", item.Title);
                statement.Bind("@UNREAD_COUNT", item.UnreadCount);
                statement.Bind("@URL", item.Url);
                statement.Bind("@HTML_URL", item.HtmlUrl);
                statement.Bind("@ICON_URL", item.IconUrl);
                statement.Bind("@FIRST_ITEM_MSEC", item.FirstItemMsec);
                statement.Bind("@IS_MAX_COUNT", item.ApproxUnreadCount ? 1 : 0);

                statement.ExecuteNonQuery();
                //statement.Step();

                skipIdList.Add(item.Id);

                // Resets the statement, to that it can be used again (with different parameters).                
                //statement.Reset();
                //statement.ClearBindings();
            }
        }

        private void SaveSubscriptionLinks(Tuple<string, string>[] catItemsLinks, SQLiteConnection connection)
        {
            if (catItemsLinks.Length == 0)
            {
                return;
            }
            throw new NotImplementedException();
            //using (var statement = connection.Prepare(@"INSERT INTO SUB_CAT_SUB_ITEM(CAT_ID, ITEM_ID) 
            //							VALUES(@CAT_ID, @ITEM_ID);"))
            //{
            //    foreach (var lnk in catItemsLinks)
            //    {
            //        statement.Bind("@CAT_ID", lnk.Item1);
            //        statement.Bind("@ITEM_ID", lnk.Item2);

            //        statement.Step();

            //        // Resets the statement, to that it can be used again (with different parameters).
            //        statement.Reset();
            //        statement.ClearBindings();
            //    }
            //}
        }

        public Task<List<Feed>> LoadSubscriptionsAsync()
        {
            throw new NotImplementedException();

            //return Task.Run(() =>
            //                {
            //                    List<CategoryItem> cats;
            //                    List<SubscriptionItem> items;
            //                    List<SubCatSubItemTableRow> links;

            //                    using (var connection = GetConnection())
            //                    {
            //                        cats = LoadSubscriptionCategories(connection);
            //                        items = LoadSubscriptionItems(connection);
            //                        links = LoadSubscriptionLinks(connection);
            //                    }

            //                    var result = new List<Feed>();

            //                    foreach (var categoryItem in cats)
            //                        categoryItem.Subscriptions = new List<SubscriptionItem>();
            //                    result.AddRange(cats.OrderBy(c => c.Title));

            //                    var linkDict = links.GroupBy(l => l.ItemId, l => l.CatId)
            //                                        .ToDictionary(g => g.Key, g => g.ToArray());

            //                    foreach (var subscriptionItem in items.OrderBy(s => s.Title))
            //                    {
            //                        string[] l;

            //                        if (!linkDict.TryGetValue(subscriptionItem.Id, out l) || (l.Length == 0))
            //                        {
            //                            if (subscriptionItem.Id == SpecialTags.Read)
            //                            {
            //                                result.Insert(0, subscriptionItem);
            //                            }
            //                            else
            //                            {
            //                                result.Add(subscriptionItem);
            //                            }

            //                            continue;
            //                        }

            //                        foreach (var c in cats.Where(c => l.Contains(c.Id)))
            //                            if (subscriptionItem.Id == c.Id)
            //                            {
            //                                c.Subscriptions.Insert(0, subscriptionItem);
            //                            }
            //                            else
            //                            {
            //                                c.Subscriptions.Add(subscriptionItem);
            //                            }
            //                    }

            //                    return result;
            //                });
        }

        //private List<CategoryItem> LoadSubscriptionCategories(SQLiteConnection connection)
        //{
        //    return connection
        //        .Table<SubCatTableRow>()
        //        .Select(t => t.ToModel())
        //        .ToList();
        //}

        //private List<SubscriptionItem> LoadSubscriptionItems(SQLiteConnection connection)
        //{
        //    return connection
        //        .Table<SubItemTableRow>()
        //        .Select(t => t.ToModel())
        //        .ToList();
        //}

        private List<SubCatSubItemTableRow> LoadSubscriptionLinks(SQLiteConnection connection)
        {
            return connection
                .Table<SubCatSubItemTableRow>()
                .ToList();
        }

        public Task SaveStreamCollectionAsync(StreamItemCollectionState collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            return Task.Run(() => { SaveStreamCollectionInternal(collection); });
        }

        private void SaveStreamCollectionInternal(StreamItemCollectionState collection)
        {
            using (var connection = GetConnection())
            {
                EnableForeignKeys(connection);

                connection.BeginTransaction();

                SaveStreamCollection(connection, collection);
                SaveStreamCollectionItems(connection, collection);

                connection.Commit();
            }
        }

        private void SaveStreamCollection(SQLiteConnection connection, StreamItemCollectionState collection)
        {
            throw new NotImplementedException();
            //using (var statement = connection.Prepare(@"INSERT OR REPLACE INTO STREAM_COLLECTION(STREAM_ID, CONTINUATION, SHOW_NEWEST_FIRST, STREAM_TIMESTAMP, FAULT) 
            //							VALUES(@STREAM_ID, @CONTINUATION, @SHOW_NEWEST_FIRST, @STREAM_TIMESTAMP, @FAULT);"))
            //{
            //    statement.Bind("@STREAM_ID", collection.StreamId);
            //    statement.Bind("@CONTINUATION", collection.Continuation);
            //    statement.Bind("@SHOW_NEWEST_FIRST", collection.ShowNewestFirst ? 1 : 0);
            //    statement.Bind("@STREAM_TIMESTAMP", collection.StreamTimestamp);
            //    statement.Bind("@FAULT", collection.Fault ? 1 : 0);

            //    statement.Step();
            //}
        }

        private void SaveStreamCollectionItems(SQLiteConnection connection, StreamItemCollectionState collection)
        {
            throw new NotImplementedException();
            //using (var statement = connection.Prepare(@"INSERT OR REPLACE INTO STREAM_ITEM(ID, STREAM_ID, PUBLISHED, TITLE, WEB_URI, CONTENT, UNREAD, NEED_SET_READ_EXPLICITLY, IS_SELECTED, STARRED, SAVED) VALUES(@ID, @STREAM_ID, @PUBLISHED, @TITLE, @WEB_URI, @CONTENT, @UNREAD, @NEED_SET_READ_EXPLICITLY, @IS_SELECTED, @STARRED, @SAVED);"))
            //{
            //    foreach (var item in collection.Items)
            //    {
            //        if (item is EmptySpaceStreamItem || item is HeaderSpaceStreamItem)
            //            continue;

            //        statement.Bind("@ID", item.Id);
            //        statement.Bind("@STREAM_ID", collection.StreamId);
            //        statement.Bind("@PUBLISHED", item.Published.ToString("O"));
            //        statement.Bind("@TITLE", item.Title);
            //        statement.Bind("@WEB_URI", item.WebUri);
            //        statement.Bind("@CONTENT", item.Content);
            //        statement.Bind("@UNREAD", item.Unread ? 1 : 0);
            //        statement.Bind("@NEED_SET_READ_EXPLICITLY", item.NeedSetReadExplicitly ? 1 : 0);
            //        statement.Bind("@IS_SELECTED", item.IsSelected ? 1 : 0);
            //        statement.Bind("@STARRED", item.Starred ? 1 : 0);
            //        statement.Bind("@SAVED", item.Saved ? 1 : 0);

            //        statement.Step();

            //        // Resets the statement, to that it can be used again (with different parameters).
            //        statement.Reset();
            //        statement.ClearBindings();
            //    }
            //}
        }

        public Task<StreamItemCollectionState> LoadStreamCollectionAsync(string streamId)
        {
            return Task.Run(() => LoadStreamCollectionInternal(streamId));
        }

        private StreamItemCollectionState LoadStreamCollectionInternal(string streamId)
        {
            using (var connection = GetConnection())
            {
                EnableForeignKeys(connection);

                var result = LoadStreamCollection(connection, streamId);

                if (result == null)
                {
                    return null;
                }

                result.Items = LoadStreamCollectionItems(connection, streamId);
                return result;
            }
        }

        private StreamItemCollectionState LoadStreamCollection(SQLiteConnection connection, string streamId)
        {
            throw new NotImplementedException();
            //using (var statement = connection.Prepare(@"SELECT CONTINUATION, SHOW_NEWEST_FIRST, STREAM_TIMESTAMP, FAULT FROM STREAM_COLLECTION WHERE STREAM_ID = @STREAM_ID;"))
            //{
            //    statement.Bind("@STREAM_ID", streamId);

            //    if (statement.Step() != SQLiteResult.ROW)
            //        return null;

            //    return new StreamItemCollectionState
            //    {
            //        StreamId = streamId,
            //        Continuation = (string)statement[0],
            //        ShowNewestFirst = statement.GetInteger(1) == 1,
            //        StreamTimestamp = (int)statement.GetInteger(2),
            //        Fault = statement.GetInteger(3) == 1
            //    };
            //}
        }

        private StreamItem[] LoadStreamCollectionItems(SQLiteConnection connection, string streamId)
        {
            var items = new List<StreamItem>();

            throw new NotImplementedException();

            //using (var statement = connection.Prepare(@"SELECT ID, PUBLISHED, TITLE, WEB_URI, CONTENT, UNREAD, NEED_SET_READ_EXPLICITLY, IS_SELECTED, STARRED, SAVED FROM STREAM_ITEM WHERE STREAM_ID = @STREAM_ID;"))
            //{
            //    statement.Bind("@STREAM_ID", streamId);

            //    while (statement.Step() == SQLiteResult.ROW)
            //    {
            //        var item = new StreamItem
            //        {
            //            Id = (string)statement[0],
            //            Published = DateTimeOffset.Parse((string)statement[1]),
            //            Title = (string)statement[2],
            //            WebUri = (string)statement[3],
            //            Content = (string)statement[4],
            //            Unread = statement.GetInteger(5) == 1,
            //            NeedSetReadExplicitly = statement.GetInteger(6) == 1,
            //            IsSelected = statement.GetInteger(7) == 1,
            //            Starred = statement.GetInteger(8) == 1,
            //            Saved = statement.GetInteger(9) == 1,
            //        };
            //        items.Add(item);
            //    }
            //}


            return items.ToArray();
        }

        public Task<ulong> GetTotalCacheSizeAsync()
        {
            return Task.Run(() => GetTotalCacheSizeInternal());
            //return GetTempFilesSizeAsync();
        }

        private ulong GetTotalCacheSizeInternal()
        {
            using (var connection = GetConnection())
            {
                throw new NotImplementedException();

                //using (var statement = connection.Prepare(@"SELECT SUM(LENGTH(CAST(CONTENT AS BLOB))) FROM STREAM_ITEM;"))
                //{
                //    if (statement.Step() != SQLiteResult.ROW)
                //        return 0UL;

                //    return (ulong)statement.GetInteger(0);
                //}
            }
        }

        private async Task<ulong> GetTempFilesSizeAsync()
        {
            var tempSize = 0UL;

            var folder = ApplicationData.Current.TemporaryFolder;
            var files = await folder.GetFilesAsync()
                .AsTask()
                .ConfigureAwait(false);
            foreach (var storageFile in files)
                tempSize += (await storageFile.GetBasicPropertiesAsync()
                    .AsTask()
                    .ConfigureAwait(false)).Size;

            return tempSize;
        }

        public Task ClearCacheAsync()
        {
            return Task.Run(() => ClearCacheInternal());
        }

        private void ClearCacheInternal()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(@"DELETE FROM STREAM_ITEM;");
            }
        }

        public async Task ClearTempFilesAsync()
        {
            var folder = ApplicationData.Current.TemporaryFolder;
            var files = await folder.GetFilesAsync()
                .AsTask()
                .ConfigureAwait(false);
            foreach (var storageFile in files)
                await storageFile.DeleteAsync()
                    .AsTask()
                    .ConfigureAwait(false);
        }

        public Task SetCachedItemAsReadAsync(string id, bool newValue)
        {
            return Task.Run(() => SetCachedItemAsReadInternal(id, newValue));
        }

        private void SetCachedItemAsReadInternal(string id, bool newValue)
        {
            using (var connection = GetConnection())
            {
                throw new NotImplementedException();
                //using (var statement = connection.Prepare(@"UPDATE STREAM_ITEM SET UNREAD = @UNREAD WHERE ID = @ID;"))
                //{
                //    statement.Bind("@ID", id);
                //    statement.Bind("@UNREAD", newValue ? 0 : 1);

                //    statement.Step();
                //}
            }
        }

        public Task SetCachedItemAsStarredAsync(string id, bool newValue)
        {
            return Task.Run(() => SetCachedItemAsStarredInternal(id, newValue));
        }

        private void SetCachedItemAsStarredInternal(string id, bool newValue)
        {
            using (var connection = GetConnection())
            {
                throw new NotImplementedException();
                //using (var statement = connection.Prepare(@"UPDATE STREAM_ITEM SET STARRED = @STARRED WHERE ID = @ID;"))
                //{
                //    statement.Bind("@ID", id);
                //    statement.Bind("@STARRED", newValue ? 1 : 0);

                //    statement.Step();
                //}
            }
        }
    }

    public class SavedStreamItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset Published { get; set; }
        public string WebUri { get; set; }
        public string ShortContent { get; set; }
        public string Content { get; set; }
        public string ImageFolder { get; set; }
    }

    public enum TagActionKind
    {
        Remove = 0,
        Add = 1
    }

    public class TagAction
    {
        public long Id { get; set; }
        public string ItemId { get; set; }
        public string Tag { get; set; }
        public TagActionKind Kind { get; set; }
    }
}