using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inoreader.Annotations;
using Inoreader.Models;
using Inoreader.Models.States;
using Microsoft.ApplicationInsights;
using SQLitePCL;

namespace Inoreader.Services
{
	public class LocalStorageManager
	{
		private readonly TelemetryClient _telemetryClient;
		private const string DatabaseFilename = "data.db";

		//INTEGER = 1,
		//FLOAT = 2,
		//TEXT = 3,
		//BLOB = 4,
		//NULL = 5,

		public LocalStorageManager(TelemetryClient telemetryClient)
		{
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			_telemetryClient = telemetryClient;
		}

		public void Init()
		{
			using (var connection = new SQLiteConnection(DatabaseFilename))
			{
				EnableForeignKeys(connection);

				using (var statement = connection.Prepare(@"CREATE TABLE IF NOT EXISTS SAVED_STREAM_ITEM (ID TEXT PRIMARY KEY NOT NULL, 
																			TITLE TEXT, 
																			PUBLISHED TEXT, 
																			WEBURI TEXT, 
																			SHORT_CONTENT TEXT, 
																			CONTENT TEXT, 
																			IMAGE_FOLDER TEXT);"))
				{
					statement.Step();
				}

				using (var statement = connection.Prepare(@"CREATE TABLE IF NOT EXISTS TAG_ACTION (ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
							ITEM_ID TEXT,
							TAG TEXT,
							ACTION_KIND INTEGER);"))
				{
					statement.Step();
				}

				using (var statement = connection.Prepare(@"CREATE TABLE IF NOT EXISTS SUB_ITEM (ID TEXT PRIMARY KEY NOT NULL, 
										SORT_ID TEXT, TITLE TEXT, UNREAD_COUNT INTEGER, URL TEXT, HTML_URL TEXT, ICON_URL TEXT, FIRST_ITEM_MSEC INTEGER);"))
				{
					statement.Step();
				}

				using (var statement = connection.Prepare(@"CREATE TABLE IF NOT EXISTS SUB_CAT (ID TEXT PRIMARY KEY NOT NULL, 
										SORT_ID TEXT, TITLE TEXT, UNREAD_COUNT INTEGER);"))
				{
					statement.Step();
				}

				using (var statement = connection.Prepare(@"CREATE TABLE IF NOT EXISTS SUB_CAT_SUB_ITEM (CAT_ID TEXT, ITEM_ID TEXT);"))
				{
					statement.Step();
				}

				using (var statement = connection.Prepare(@"CREATE TABLE IF NOT EXISTS STREAM_COLLECTION (STREAM_ID TEXT PRIMARY KEY NOT NULL, CONTINUATION TEXT, SHOW_NEWEST_FIRST INTEGER, STREAM_TIMESTAMP INTEGER, FAULT INTEGER);"))
				{
					statement.Step();
				}

				using (var statement = connection.Prepare(@"CREATE TABLE IF NOT EXISTS STREAM_ITEM (ID TEXT PRIMARY KEY NOT NULL, STREAM_ID TEXT REFERENCES STREAM_COLLECTION(STREAM_ID) ON DELETE CASCADE, PUBLISHED TEXT, TITLE TEXT, WEB_URI TEXT, CONTENT TEXT, UNREAD INTEGER, NEED_SET_READ_EXPLICITLY INTEGER, IS_SELECTED INTEGER, STARRED INTEGER, SAVED INTEGER);"))
				{
					statement.Step();
				}
			}
		}

		private void EnableForeignKeys(SQLiteConnection connection)
		{
			using (var statement = connection.Prepare(@"PRAGMA foreign_keys = ON;"))
			{
				statement.Step();
			}
		}

		private void BeginTransaction(SQLiteConnection connection)
		{
			using (var statement = connection.Prepare("Begin Transaction"))
			{
				statement.Step();
			}
		}

		private void CommitTransaction(SQLiteConnection connection)
		{
			using (var statement = connection.Prepare("Commit Transaction"))
			{
				statement.Step();
			}
		}

		#region Saved stream item

		public void Save([NotNull] SavedStreamItem item)
		{
			if (item == null) throw new ArgumentNullException("item");

			using (var connection = new SQLiteConnection(DatabaseFilename))
			{
				using (var statement = connection.Prepare(@"INSERT INTO SAVED_STREAM_ITEM(ID, TITLE, PUBLISHED, WEBURI, SHORT_CONTENT, CONTENT, IMAGE_FOLDER) 
																			VALUES(@ID, @TITLE, @PUBLISHED, @WEBURI, @SHORT_CONTENT, @CONTENT, @IMAGE_FOLDER);"))
				{
					statement.Bind("@ID", item.Id);
					statement.Bind("@TITLE", item.Title);
					statement.Bind("@PUBLISHED", item.Published.ToString("O"));
					statement.Bind("@WEBURI", item.WebUri);
					statement.Bind("@SHORT_CONTENT", item.ShortContent);
					statement.Bind("@CONTENT", item.Content);
					statement.Bind("@IMAGE_FOLDER", item.ImageFolder);

					statement.Step();

					//// Resets the statement, to that it can be used again (with different parameters).
					//statement.Reset();
					//statement.ClearBindings();
				}
			}
		}

		public List<SavedStreamItem> LoadSavedStreamItems()
		{
			var result = new List<SavedStreamItem>();

			using (var connection = new SQLiteConnection(DatabaseFilename))
			{
				using (var statement = connection.Prepare(@"SELECT ID, TITLE, PUBLISHED, WEBURI, SHORT_CONTENT, CONTENT, IMAGE_FOLDER FROM SAVED_STREAM_ITEM;"))
				{
					while (statement.Step() == SQLiteResult.ROW)
					{
						var item = new SavedStreamItem
						{
							Id = (string)statement[0],
							Title = (string)statement[1],
							Published = DateTimeOffset.Parse((string)statement[2]),
							WebUri = (string)statement[3],
							ShortContent = (string)statement[4],
							Content = (string)statement[5],
							ImageFolder = (string)statement[6],
						};

						result.Add(item);
					}
				}
			}

			return result;
		}

		public void DeleteSavedStreamItem([NotNull] string id)
		{
			if (id == null) throw new ArgumentNullException("id");

			using (var connection = new SQLiteConnection(DatabaseFilename))
			{
				using (var statement = connection.Prepare(@"DELETE FROM SAVED_STREAM_ITEM WHERE ID = @ID;"))
				{
					statement.Bind("@ID", id);
					statement.Step();
				}
			}
		}

		#endregion

		#region Tag actions

		public void AddTagAction(string itemId, string tag, TagActionKind kind)
		{
			using (var connection = new SQLiteConnection(DatabaseFilename))
			{
				using (
					var statement =
						connection.Prepare(@"INSERT INTO TAG_ACTION(ITEM_ID, TAG, ACTION_KIND) VALUES(@ITEM_ID, @TAG, @ACTION_KIND);"))
				{
					statement.Bind("@ITEM_ID", itemId);
					statement.Bind("@TAG", tag);
					statement.Bind("@ACTION_KIND", (int)kind);

					statement.Step();
				}
			}
		}

		public TagAction GetNextTagAction()
		{
			using (var connection = new SQLiteConnection(DatabaseFilename))
			{
				using (
					var statement = connection.Prepare(@"SELECT ID, ITEM_ID, TAG, ACTION_KIND FROM TAG_ACTION ORDER BY ID LIMIT 1;"))
				{
					if (statement.Step() != SQLiteResult.ROW)
						return null;

					var item = new TagAction
					{
						Id = statement.GetInteger(0),
						ItemId = statement.GetText(1),
						Tag = statement.GetText(2),
						Kind = (TagActionKind)statement.GetInteger(3)
					};

					return item;
				}
			}
		}

		public void DeleteTagAction(long id)
		{
			using (var connection = new SQLiteConnection(DatabaseFilename))
			{
				using (var statement = connection.Prepare(@"DELETE FROM TAG_ACTION WHERE ID = @ID;"))
				{
					statement.Bind("@ID", id);
					statement.Step();
				}
			}
		}

		#endregion

		#region Subscriptions

		public Task SaveSubscriptionsAsync(List<TreeItemBase> items)
		{
			var cats = items.OfType<CategoryItem>().ToArray();
			var subItems = items.OfType<SubscriptionItem>().Union(cats.SelectMany(c => c.Subscriptions)).ToArray();
			var catItemsLinks = (from c in cats
								 from s in c.Subscriptions
								 select new Tuple<string, string>(c.Id, s.Id)).ToArray();

			return Task.Run(() =>
			{
				try
				{
					using (var connection = new SQLiteConnection(DatabaseFilename))
					{
						BeginTransaction(connection);

						ClearSubscriptions(connection);
						SaveSubscriptionCategories(cats, connection);
						SaveSubscriptionItems(subItems, connection);
						SaveSubscriptionLinks(catItemsLinks, connection);

						CommitTransaction(connection);
					}
				}
				catch (Exception e)
				{
					_telemetryClient.TrackException(e);
				}
			});
		}

		private void ClearSubscriptions(SQLiteConnection connection)
		{
			using (var statement = connection.Prepare("DELETE FROM SUB_CAT_SUB_ITEM;"))
			{
				statement.Step();
			}

			using (var statement = connection.Prepare("DELETE FROM SUB_CAT;"))
			{
				statement.Step();
			}

			using (var statement = connection.Prepare("DELETE FROM SUB_ITEM;"))
			{
				statement.Step();
			}
		}

		private void SaveSubscriptionCategories(CategoryItem[] cats, SQLiteConnection connection)
		{
			if (cats.Length == 0) return;

			using (var statement = connection.Prepare(@"INSERT INTO SUB_CAT(ID, SORT_ID, TITLE, UNREAD_COUNT) 
																			VALUES(@ID, @SORT_ID, @TITLE, @UNREAD_COUNT);"))
			{
				foreach (var categoryItem in cats)
				{
					statement.Bind("@ID", categoryItem.Id);
					statement.Bind("@SORT_ID", categoryItem.SortId);
					statement.Bind("@TITLE", categoryItem.Title);
					statement.Bind("@UNREAD_COUNT", categoryItem.UnreadCount);

					statement.Step();

					// Resets the statement, to that it can be used again (with different parameters).
					statement.Reset();
					statement.ClearBindings();
				}
			}
		}

		private void SaveSubscriptionItems(SubscriptionItem[] subItems, SQLiteConnection connection)
		{
			if (subItems.Length == 0) return;

			var skipIdList = new List<string>(subItems.Length);

			using (
				var statement =
					connection.Prepare(
						@"INSERT INTO SUB_ITEM(ID, SORT_ID, TITLE, UNREAD_COUNT, URL, HTML_URL, ICON_URL, FIRST_ITEM_MSEC) 
																			VALUES(@ID, @SORT_ID, @TITLE, @UNREAD_COUNT, @URL, @HTML_URL, @ICON_URL, @FIRST_ITEM_MSEC);"))
			{
				foreach (var item in subItems)
				{
					if (skipIdList.Contains(item.Id))
						continue;

					statement.Bind("@ID", item.Id);
					statement.Bind("@SORT_ID", item.SortId);
					statement.Bind("@TITLE", item.Title);
					statement.Bind("@UNREAD_COUNT", item.UnreadCount);
					statement.Bind("@URL", item.Url);
					statement.Bind("@HTML_URL", item.HtmlUrl);
					statement.Bind("@ICON_URL", item.IconUrl);
					statement.Bind("@FIRST_ITEM_MSEC", item.FirstItemMsec);

					statement.Step();

					skipIdList.Add(item.Id);

					// Resets the statement, to that it can be used again (with different parameters).
					statement.Reset();
					statement.ClearBindings();
				}
			}
		}

		private void SaveSubscriptionLinks(Tuple<string, string>[] catItemsLinks, SQLiteConnection connection)
		{
			if (catItemsLinks.Length == 0) return;

			using (var statement = connection.Prepare(@"INSERT INTO SUB_CAT_SUB_ITEM(CAT_ID, ITEM_ID) 
																			VALUES(@CAT_ID, @ITEM_ID);"))
			{
				foreach (var lnk in catItemsLinks)
				{
					statement.Bind("@CAT_ID", lnk.Item1);
					statement.Bind("@ITEM_ID", lnk.Item2);

					statement.Step();

					// Resets the statement, to that it can be used again (with different parameters).
					statement.Reset();
					statement.ClearBindings();
				}
			}
		}

		public Task<List<TreeItemBase>> LoadSubscriptionsAsync()
		{
			_telemetryClient.TrackEvent(TelemetryEvents.LoadSubscriptionsFromCache);

			return Task.Run(() =>
			{
				try
				{
					List<CategoryItem> cats;
					List<SubscriptionItem> items;
					List<Tuple<string, string>> links;

					using (var connection = new SQLiteConnection(DatabaseFilename))
					{
						cats = LoadSubscriptionCategories(connection);
						items = LoadSubscriptionItems(connection);
						links = LoadSubscriptionLinks(connection);
					}

					var result = new List<TreeItemBase>();

					foreach (var categoryItem in cats)
					{
						categoryItem.Subscriptions = new List<SubscriptionItem>();
					}
					result.AddRange(cats.OrderBy(c => c.Title));

					var linkDict = links.GroupBy(l => l.Item2, l => l.Item1).ToDictionary(g => g.Key, g => g.ToArray());

					foreach (var subscriptionItem in items.OrderBy(s => s.Title))
					{
						string[] l;

						if (!linkDict.TryGetValue(subscriptionItem.Id, out l) || l.Length == 0)
						{
							if (subscriptionItem.Id == SpecialTags.Read)
							{
								result.Insert(0, subscriptionItem);
							}
							else
							{
								result.Add(subscriptionItem);
							}

							continue;
						}

						foreach (var c in cats.Where(c => l.Contains(c.Id)))
						{
							if (subscriptionItem.Id == c.Id)
							{
								c.Subscriptions.Insert(0, subscriptionItem);
							}
							else
							{
								c.Subscriptions.Add(subscriptionItem);
							}
						}
					}

					return result;
				}
				catch (Exception e)
				{
					_telemetryClient.TrackException(e);
					return null;
				}
			});
		}

		private List<CategoryItem> LoadSubscriptionCategories(SQLiteConnection connection)
		{
			var result = new List<CategoryItem>();

			using (var statement = connection.Prepare(@"SELECT ID, SORT_ID, TITLE, UNREAD_COUNT FROM SUB_CAT;"))
			{
				while (statement.Step() == SQLiteResult.ROW)
				{
					var item = new CategoryItem
					{
						Id = statement.GetText(0),
						SortId = statement.GetText(1),
						Title = statement.GetText(2),
						UnreadCount = statement.GetInteger(3)
					};

					result.Add(item);
				}
			}

			return result;
		}

		private List<SubscriptionItem> LoadSubscriptionItems(SQLiteConnection connection)
		{
			var result = new List<SubscriptionItem>();

			using (var statement = connection.Prepare(@"SELECT ID, SORT_ID, TITLE, UNREAD_COUNT, URL, HTML_URL, ICON_URL, FIRST_ITEM_MSEC FROM SUB_ITEM;"))
			{
				while (statement.Step() == SQLiteResult.ROW)
				{
					var item = new SubscriptionItem
					{
						Id = (string)statement[0],
						SortId = (string)statement[1],
						Title = (string)statement[2],
						UnreadCount = (long)statement[3],
						Url = (string)statement[4],
						HtmlUrl = (string)statement[5],
						IconUrl = (string)statement[6],
						FirstItemMsec = (long)statement[7]
					};

					result.Add(item);
				}
			}

			return result;
		}

		private List<Tuple<string, string>> LoadSubscriptionLinks(SQLiteConnection connection)
		{
			var result = new List<Tuple<string, string>>();

			using (var statement = connection.Prepare(@"SELECT CAT_ID, ITEM_ID FROM SUB_CAT_SUB_ITEM;"))
			{
				while (statement.Step() == SQLiteResult.ROW)
				{
					var item = new Tuple<string, string>(statement.GetText(0), statement.GetText(1));
					result.Add(item);
				}
			}

			return result;
		}

		#endregion

		#region Stream collection

		public Task SaveStreamCollectionAsync(StreamItemCollectionState collection)
		{
			if (collection == null) throw new ArgumentNullException("collection");

			return Task.Run(() =>
			{
				using (var connection = new SQLiteConnection(DatabaseFilename))
				{
					EnableForeignKeys(connection);

					BeginTransaction(connection);

					DeleteStreamCollection(connection, collection.StreamId);
					SaveStreamCollection(connection, collection);
					SaveStreamCollectionItems(connection, collection);
					
					CommitTransaction(connection);
				}
			});
		}

		private void DeleteStreamCollection(SQLiteConnection connection, string streamId)
		{
			using (var statement = connection.Prepare("DELETE FROM STREAM_COLLECTION WHERE STREAM_ID = @STREAM_ID;"))
			{
				statement.Bind("@STREAM_ID", streamId);
				statement.Step();
			}
		}

		private void SaveStreamCollection(SQLiteConnection connection, StreamItemCollectionState collection)
		{
			using (var statement = connection.Prepare(@"INSERT INTO STREAM_COLLECTION(STREAM_ID, CONTINUATION, SHOW_NEWEST_FIRST, STREAM_TIMESTAMP, FAULT) 
																			VALUES(@STREAM_ID, @CONTINUATION, @SHOW_NEWEST_FIRST, @STREAM_TIMESTAMP, @FAULT);"))
			{
				statement.Bind("@STREAM_ID", collection.StreamId);
				statement.Bind("@CONTINUATION", collection.Continuation);
				statement.Bind("@SHOW_NEWEST_FIRST", collection.ShowNewestFirst ? 1 : 0);
				statement.Bind("@STREAM_TIMESTAMP", collection.StreamTimestamp);
				statement.Bind("@FAULT", collection.Fault ? 1 : 0);

				statement.Step();
			}
		}

		private void SaveStreamCollectionItems(SQLiteConnection connection, StreamItemCollectionState collection)
		{
			using (var statement = connection.Prepare(@"INSERT INTO STREAM_ITEM(ID, STREAM_ID, PUBLISHED, TITLE, WEB_URI, CONTENT, UNREAD, NEED_SET_READ_EXPLICITLY, IS_SELECTED, STARRED, SAVED) VALUES(@ID, @STREAM_ID, @PUBLISHED, @TITLE, @WEB_URI, @CONTENT, @UNREAD, @NEED_SET_READ_EXPLICITLY, @IS_SELECTED, @STARRED, @SAVED);"))
			{
				foreach (var item in collection.Items)
				{
					if (item is EmptySpaceStreamItem)
						continue;

					statement.Bind("@ID", item.Id);
					statement.Bind("@STREAM_ID", collection.StreamId);
					statement.Bind("@PUBLISHED", item.Title);
					statement.Bind("@TITLE", item.Title);
					statement.Bind("@WEB_URI", item.WebUri);
					statement.Bind("@CONTENT", item.Content);
					statement.Bind("@UNREAD", item.Unread ? 1 : 0);
					statement.Bind("@NEED_SET_READ_EXPLICITLY", item.NeedSetReadExplicitly ? 1 : 0);
					statement.Bind("@IS_SELECTED", item.IsSelected ? 1 : 0);
					statement.Bind("@STARRED", item.Starred ? 1 : 0);
					statement.Bind("@SAVED", item.Saved ? 1 : 0);

					statement.Step();

					// Resets the statement, to that it can be used again (with different parameters).
					statement.Reset();
					statement.ClearBindings();
				}
			}
		}

		#endregion
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