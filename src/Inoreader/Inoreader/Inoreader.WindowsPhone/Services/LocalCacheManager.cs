using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Inoreader.Models;

namespace Inoreader.Services
{
	public class LocalCacheManager
	{
		private const string CacheFolderName = "LocalCache";
		private const string HtmlStringContentFileName = "html.data";
		private readonly StorageFolder _rootCacheFolder = ApplicationData.Current.LocalCacheFolder;

		private readonly Dictionary<string, string> _index;
		private readonly List<LocalStreamItem> _items;

		public IReadOnlyCollection<LocalStreamItem> Items
		{
			get
			{
				return new ReadOnlyCollection<LocalStreamItem>(_items);
			}
		}

		public LocalCacheManager(LocalCacheState state)
		{
			if (state == null)
			{
				_index = new Dictionary<string, string>();
				_items = new List<LocalStreamItem>();
			}
			else
			{
				_index = state.Index;
				_items = state.Items;
			}
		}

		public async Task AddAsync(StreamItem item)
		{
			var folderName = Guid.NewGuid().ToString("N");
			_index.Add(item.Id, folderName);
			
			var parser = new HtmlParser();
			var plainText = parser.GetPlainText(item.Content, 200);
			
			_items.Add(new LocalStreamItem
			{
				Id = item.Id,
				Published = item.Published,
				ShortPlainText = plainText,
				Starred = item.Starred,
				Title = item.Title,
				WebUri = item.WebUri
			});

			var cacheFolder = await _rootCacheFolder.CreateFolderAsync(CacheFolderName, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
			var folder = await cacheFolder.CreateFolderAsync(folderName).AsTask().ConfigureAwait(false);

			var newHtml = await SaveImagesAsync(folder, item.Content).ConfigureAwait(false);

			var file = await folder.CreateFileAsync(HtmlStringContentFileName).AsTask().ConfigureAwait(false);
			await FileIO.WriteTextAsync(file, newHtml).AsTask().ConfigureAwait(false);
		}

		private async Task<string> SaveImagesAsync(StorageFolder folder, string html)
		{
			var parser = new HtmlParser();
			var lexemes = parser.Parse(html);

			var fixedImages = new List<string>();
			var localHtml = new StringBuilder(html);

			foreach (var lexeme in lexemes.OfType<HtmlTagLexeme>())
			{
				if (!String.Equals(lexeme.Name, "img", StringComparison.OrdinalIgnoreCase))
					continue;

				var src = lexeme.Attributes["src"];
				if (fixedImages.Any(s => String.Equals(s, src, StringComparison.OrdinalIgnoreCase)))
					continue;

				var fileName = Guid.NewGuid().ToString("N");
				if (!await DownloadImageAsync(src, folder, fileName).ConfigureAwait(false))
					continue;
				
				fixedImages.Add(src);

				var newSrc = String.Format("ms-appdata:///localcache/{0}/{1}/{2}", CacheFolderName, folder.Name, fileName);
				localHtml.Replace(src, newSrc);				
			}

			return localHtml.ToString();
		}

		private async Task<bool> DownloadImageAsync(string src, StorageFolder folder, string fileName)
		{
			var client = new HttpClient();
			byte[] buffer;
			
			try
			{
				buffer = await client.GetByteArrayAsync(src).ConfigureAwait(false);
			}
			catch (Exception)
			{
				return false;
			}

			var file = await folder.CreateFileAsync(fileName).AsTask().ConfigureAwait(false);
			await FileIO.WriteBytesAsync(file, buffer).AsTask().ConfigureAwait(false);

			return true;
		}

		public async Task DeleteAsync(string itemId)
		{
			string folderName;
			if (!_index.TryGetValue(itemId, out folderName))
				return;

			_index.Remove(itemId);
			_items.RemoveAll(a => a.Id == itemId);
			
			var cacheFolder = await _rootCacheFolder.CreateFolderAsync(CacheFolderName, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
			var folder = await cacheFolder.GetFolderAsync(folderName).AsTask().ConfigureAwait(false);
			await folder.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask().ConfigureAwait(false);
		}
	}

	[DataContract]
	public class LocalCacheState
	{
		[DataMember]
		public Dictionary<string, string> Index { get; set; }

		[DataMember]
		public List<LocalStreamItem> Items { get; set; }
	}

	[DataContract]
	public class LocalStreamItem : BindableBaseEx
	{
		[DataMember]
		private bool _starred;

		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public DateTimeOffset Published { get; set; }

		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public string WebUri { get; set; }

		[DataMember]
		public string ShortPlainText { get; set; }

		public bool Starred
		{
			get { return _starred; }
			set { SetProperty(ref _starred, value); }
		}
	}
}