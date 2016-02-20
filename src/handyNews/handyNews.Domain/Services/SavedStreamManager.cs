using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using handyNews.Domain.Models;
using handyNews.Domain.Models.Parser;
using JetBrains.Annotations;

namespace handyNews.Domain.Services
{
	public class SavedStreamManager
	{
		private const string CacheFolderName = "SavedItems";
		private readonly StorageFolder _rootCacheFolder = ApplicationData.Current.LocalFolder;
		private readonly LocalStorageManager _storageManager;

		private readonly Lazy<List<SavedStreamItem>> _items;

		public IReadOnlyCollection<SavedStreamItem> Items
		{
			get
			{
				return new ReadOnlyCollection<SavedStreamItem>(_items.Value);
			}
		}

		public SavedStreamManager([NotNull] LocalStorageManager storageManager)
		{
			if (storageManager == null) throw new ArgumentNullException("storageManager");
			_storageManager = storageManager;

			_items = new Lazy<List<SavedStreamItem>>(InitItems);
		}

		private List<SavedStreamItem> InitItems()
		{
			return new List<SavedStreamItem>(_storageManager.LoadSavedStreamItems());
		}

		public async Task AddAsync(StreamItem item)
		{
			if (_items.Value.Any(t => t.Id == item.Id))
				return;

			var folderName = Guid.NewGuid().ToString("N");

			var parser = new HtmlParser();
			var plainText = parser.GetPlainText(item.Content, 200);

			var cacheFolder = await _rootCacheFolder.CreateFolderAsync(CacheFolderName, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
			var folder = await cacheFolder.CreateFolderAsync(folderName).AsTask().ConfigureAwait(false);

			var newHtml = await SaveImagesAsync(folder, item.Content).ConfigureAwait(false);

			var savedItem = new SavedStreamItem
			{
				Id = item.Id,
				Title = item.Title,
				Published = item.Published,
				WebUri = item.WebUri,
				ShortContent = plainText,
				Content = newHtml,
				ImageFolder = folderName
			};

			_items.Value.Add(savedItem);
			_storageManager.Save(savedItem);
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

				var fileName = await DownloadImageAsync(src, folder).ConfigureAwait(false);
				if (fileName == null)
					continue;

				fixedImages.Add(src);

				var newSrc = String.Format("ms-appdata:///local/{0}/{1}/{2}", CacheFolderName, folder.Name, fileName);
				localHtml.Replace(src, newSrc);
			}

			return localHtml.ToString();
		}

		private async Task<string> DownloadImageAsync(string src, StorageFolder folder)
		{
			var client = new HttpClient();
			HttpResponseMessage response;

			try
			{
				response = await client.GetAsync(src).ConfigureAwait(false);
			}
			catch (Exception)
			{
				return null;
			}

			if (!response.IsSuccessStatusCode)
				return null;

			var fileName = Guid.NewGuid().ToString("N");
			
			var file = await folder.CreateFileAsync(fileName).AsTask().ConfigureAwait(false);
			using (var stream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
			{
				await response.Content.CopyToAsync(stream).ConfigureAwait(false);
			}

			return fileName;
		}

		public async Task DeleteAsync([NotNull] string itemId)
		{
			if (itemId == null) throw new ArgumentNullException("itemId");

			var item = _items.Value.FirstOrDefault(a => a.Id == itemId);
			if (item == null)
				return;

			_items.Value.Remove(item);
			_storageManager.DeleteSavedStreamItem(item.Id);

			var cacheFolder = await _rootCacheFolder.CreateFolderAsync(CacheFolderName, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);

			var folder = await cacheFolder.GetFolderAsync(item.ImageFolder).AsTask().ConfigureAwait(false);
			await folder.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask().ConfigureAwait(false);
		}
	}
}