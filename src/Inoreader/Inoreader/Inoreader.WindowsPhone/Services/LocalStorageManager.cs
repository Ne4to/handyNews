using System;
using System.Collections.Generic;
using Inoreader.Annotations;
using SQLitePCL;

namespace Inoreader.Services
{
	public class LocalStorageManager
	{
		private const string DatabaseFilename = "data.db";

		//INTEGER = 1,
		//FLOAT = 2,
		//TEXT = 3,
		//BLOB = 4,
		//NULL = 5,

		public void Init()
		{
			using (var connection = new SQLiteConnection(DatabaseFilename))
			{
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

				using (var statement = connection.Prepare(@"CREATE TABLE IN NOT EXISTS TAG_ACTION (ID INTEGER PRIMARY KEY NOT NULL AUTOINCREMENT,
							ITEM_ID TEXT,
							TAG TEXT,
							ACTION_KIND INTEGER);"))
				{
					statement.Step();
				}
			}
		}

		#region SavedStreamItem

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

		public void AddTagAction(string itemId, string tag, TagActionKind kind)
		{
			using (var connection = new SQLiteConnection(DatabaseFilename))
			{
				using (var statement = connection.Prepare(@"INSERT INTO TAG_ACTION(ITEM_ID, TAG, ACTION_KIND) VALUES(@ITEM_ID, @TAG, @ACTION_KIND);"))
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
				using (var statement = connection.Prepare(@"SELECT ID, ITEM_ID, TAG, ACTION_KIND FROM TAG_ACTION ORDER BY ID LIMIT 1;"))
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