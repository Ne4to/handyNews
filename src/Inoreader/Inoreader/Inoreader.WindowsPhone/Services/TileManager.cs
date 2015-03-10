using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Inoreader.Annotations;
using Inoreader.Views.Controls;
using Microsoft.ApplicationInsights;
using NotificationsExtensions.TileContent;

namespace Inoreader.Services
{
	public class TileManager
	{
		private const string DrawCanvasName = "DrawCanvas";
		private const string WideFileName = "WideTile.png";
		private const string SquareFileName = "SquareTile.png";
		private const string SquareSmallFileName = "SquareSmallTile.png";

		private readonly TelemetryClient _telemetryClient;

		public TileManager([NotNull] TelemetryClient telemetryClient)
		{
			if (telemetryClient == null) throw new ArgumentNullException("telemetryClient");
			_telemetryClient = telemetryClient;
		}

		public async void UpdateAsync(int count)
		{
			try
			{
				var wideImage = await RenderAsync(new StartTileWide(count), 310, 150);
				if (wideImage == null)
					return;
				var wideName = await SaveFileAsync(wideImage, WideFileName, 310U, 150U);

				var squareImage = await RenderAsync(new StartTileSquare(count), 150, 150);
				if (squareImage == null)
					return;
				var squareName =await SaveFileAsync(squareImage, SquareFileName, 150U, 150U);

				var squareSmallImage = await RenderAsync(new StartTileSquareSmall(count), 71, 71);
				if (squareSmallImage == null)
					return;
				var squareSmallName = await SaveFileAsync(squareSmallImage, SquareSmallFileName, 71U, 71U);

				var tileWide = TileContentFactory.CreateTileWide310x150Image();
				tileWide.StrictValidation = true;
			
				tileWide.Branding = TileBranding.Name;
				tileWide.Image.Src = "ms-appdata:///local/" + wideName;
			
				var tileSquare = TileContentFactory.CreateTileSquare150x150Image();
				tileSquare.Branding = TileBranding.Name;
				tileSquare.Image.Src = "ms-appdata:///local/" + squareName;
				tileWide.Square150x150Content = tileSquare;

				var tileSquareSmall = TileContentFactory.CreateTileSquare71x71Image();
				tileSquareSmall.Branding = TileBranding.None;
				tileSquareSmall.Image.Src = "ms-appdata:///local/" + squareSmallName;
				tileSquare.Square71x71Content = tileSquareSmall;

				var tileNotification = tileWide.CreateNotification();
				tileNotification.ExpirationTime = DateTimeOffset.Now.AddDays(1D);

				var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();			
				tileUpdater.Update(tileNotification);
			}
			catch (Exception ex)
			{
				_telemetryClient.TrackException(ex);
			}
		}

		private static async Task<string> SaveFileAsync(RenderTargetBitmap image, string fileName, uint imageWidth, uint imageHeight)
		{
			var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
			using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
			{
				var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
				var pixelsBuffer = await image.GetPixelsAsync();
				byte[] bytes = pixelsBuffer.ToArray();

				var dpi = DisplayInformation.GetForCurrentView().LogicalDpi;

				encoder.SetPixelData(BitmapPixelFormat.Bgra8,
					BitmapAlphaMode.Ignore,
					(uint)image.PixelWidth, (uint)image.PixelHeight,
					dpi, dpi, bytes);
				
				await encoder.FlushAsync();
				stream.Dispose();
			}

			return file.Name;
		}

		private async Task<RenderTargetBitmap> RenderAsync(FrameworkElement drawControl, int width, int height)
		{
			var frame = Window.Current.Content as Frame;
			if (frame == null)
				return null;

			var page = frame.Content as Page;
			if (page == null)
				return null;

			var grid = page.Content as Grid;
			if (grid == null)
				return null;

			var canvas = grid.FindName(DrawCanvasName) as Canvas;
			if (canvas == null)
			{
				canvas = new Canvas();
				canvas.Name = DrawCanvasName;
				canvas.Opacity = 0;
				canvas.Background = new SolidColorBrush(Colors.White);
				canvas.IsHitTestVisible = false;

				if (grid.ColumnDefinitions.Count > 1)
					Grid.SetColumnSpan(canvas, grid.ColumnDefinitions.Count);

				if (grid.RowDefinitions.Count > 1)
					Grid.SetRowSpan(canvas, grid.RowDefinitions.Count);

				grid.Children.Add(canvas);
			}

			var ctrl = drawControl;
			ctrl.Width = width;
			ctrl.Height = height;

			canvas.Children.Add(ctrl);
			canvas.InvalidateMeasure();
			canvas.UpdateLayout();

			var tile = ctrl as ITile;
			if (tile != null)
			{
				await tile.LoadComplete;
			}

			RenderTargetBitmap rtb = new RenderTargetBitmap();
			await rtb.RenderAsync(ctrl, width, height);

			var result = rtb;

			canvas.Children.Clear();

			return result;
		}
	}
}
