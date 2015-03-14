using System;
using System.Collections.Generic;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Inoreader.Services
{
	public class ImageManager
	{
		private readonly Dictionary<RichTextBlock, IList<Image>> _allBlocks = new Dictionary<RichTextBlock, IList<Image>>();

		public ImageManager()
		{
			DisplayInformation.GetForCurrentView().OrientationChanged += DisplayInformation_OrientationChanged;
		}

		public void RegisterBlock(RichTextBlock textBlock, IList<Image> images)
		{
			IList<Image> oldImages;
			if (_allBlocks.TryGetValue(textBlock, out oldImages))
			{
				foreach (var oldImage in oldImages)
				{
					oldImage.Unloaded -= textBlock_Unloaded;
				}
			}

			_allBlocks[textBlock] = images;
			textBlock.Unloaded += textBlock_Unloaded;
		}

		void textBlock_Unloaded(object sender, RoutedEventArgs e)
		{
			var textBlock = (RichTextBlock)sender;
			textBlock.Unloaded -= textBlock_Unloaded;
			_allBlocks.Remove(textBlock);
		}

		public static double GetMaxImageWidth(DisplayInformation display)
		{
			// 36 = 6 (unread rectangle) + 5*2 (margin 5,0) + 20 (star control)
			var maxImageWidth = Window.Current.Bounds.Width - 36D;

			if (display.CurrentOrientation == DisplayOrientations.Landscape
			    || display.CurrentOrientation == DisplayOrientations.LandscapeFlipped)
			{
				var frame = Window.Current.Content as Frame;
				if (frame != null)
				{
					var page = frame.Content as Page;
					if (page != null)
					{
						var bottomAppBar = page.BottomAppBar;

						if (bottomAppBar != null)
							maxImageWidth -= bottomAppBar.ActualHeight;
					}
				}
			}
			return maxImageWidth;
		}

		public static void UpdateImageSize(Image image, double maxImageWidth)
		{
			var imgSource = (BitmapImage)image.Source;
			var width = Math.Min(imgSource.PixelWidth, maxImageWidth);

			if (!(Math.Abs(width) > 0.1D))
				return;

			var k = width / imgSource.PixelWidth;

			image.Width = width;
			image.Height = imgSource.PixelHeight * k;
		}

		private void DisplayInformation_OrientationChanged(DisplayInformation display, object args)
		{
			var maxImageWidth = GetMaxImageWidth(display);

			foreach (var list in _allBlocks.Values)
			{
				foreach (var image in list)
				{
					UpdateImageSize(image, maxImageWidth);
				}
			}
		}

	}
}