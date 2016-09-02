using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using handyNews.Domain.Services.Interfaces;
using handyNews.Domain.Views.Controls;
using JetBrains.Annotations;
using NotificationsExtensions.Tiles;

namespace handyNews.Domain.Services
{
    public class TileManager : ITileManager
    {
        private const string DrawCanvasName = "DrawCanvas";
        private const string WideFileName = "WideTile.png";
        private const string SquareFileName = "SquareTile.png";
        private const string SquareSmallFileName = "SquareSmallTile.png";

        private readonly ITelemetryManager _telemetryManager;

        public TileManager([NotNull] ITelemetryManager telemetryManager)
        {
            if (telemetryManager == null)
            {
                throw new ArgumentNullException(nameof(telemetryManager));
            }
            _telemetryManager = telemetryManager;
        }

        public async void UpdatePrimaryTile(long count)
        {
            try
            {
                var wideImage = await RenderAsync(new StartTileWide(count), 310, 150);
                if (wideImage == null)
                {
                    return;
                }
                var wideName = await SaveFileAsync(wideImage, WideFileName, 310U, 150U);

                var squareImage = await RenderAsync(new StartTileSquare(count), 150, 150);
                if (squareImage == null)
                {
                    return;
                }
                var squareName = await SaveFileAsync(squareImage, SquareFileName, 150U, 150U);

                var squareSmallImage = await RenderAsync(new StartTileSquareSmall(count), 71, 71);
                if (squareSmallImage == null)
                {
                    return;
                }
                var squareSmallName = await SaveFileAsync(squareSmallImage, SquareSmallFileName, 71U, 71U);


                var tile = new TileContent
                           {
                               Visual = new TileVisual
                                        {
                                            TileWide = new TileBinding
                                                       {
                                                           Branding = TileBranding.Name,
                                                           Content = new TileBindingContentAdaptive
                                                                     {
                                                                         BackgroundImage = new TileBackgroundImage
                                                                                           {
                                                                                               Source = new TileImageSource("ms-appdata:///local/" + wideName)
                                                                                           }
                                                                     }
                                                       },
                                            TileMedium = new TileBinding
                                                         {
                                                             Branding = TileBranding.Name,
                                                             Content = new TileBindingContentAdaptive
                                                                       {
                                                                           BackgroundImage = new TileBackgroundImage
                                                                                             {
                                                                                                 Source = new TileImageSource("ms-appdata:///local/" + squareName)
                                                                                             }
                                                                       }
                                                         },
                                            TileSmall = new TileBinding
                                                        {
                                                            Branding = TileBranding.None,
                                                            Content = new TileBindingContentAdaptive
                                                                      {
                                                                          BackgroundImage = new TileBackgroundImage
                                                                                            {
                                                                                                Source = new TileImageSource("ms-appdata:///local/" + squareSmallName)
                                                                                            }
                                                                      }
                                                        }
                                        }
                           };

                var doc = tile.GetXml();
                var tileNotification = new TileNotification(doc);
                tileNotification.ExpirationTime = DateTimeOffset.Now.AddDays(1D);

                var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
                tileUpdater.Clear();
                tileUpdater.Update(tileNotification);
            }
            catch (Exception ex)
            {
                _telemetryManager.TrackError(ex);
            }
        }

        private static async Task<string> SaveFileAsync(RenderTargetBitmap image, string fileName, uint imageWidth,
                                                        uint imageHeight)
        {
            var file =
                await
                    ApplicationData.Current.LocalFolder.CreateFileAsync(fileName,
                                                                        CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                var pixelsBuffer = await image.GetPixelsAsync();
                var bytes = pixelsBuffer.ToArray();

                var dpi = DisplayInformation.GetForCurrentView()
                                            .LogicalDpi;

                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                     BitmapAlphaMode.Straight,
                                     (uint) image.PixelWidth, (uint) image.PixelHeight,
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
            {
                return null;
            }

            var page = frame.Content as Page;
            if (page == null)
            {
                return null;
            }

            var grid = page.Content as Grid;
            if (grid == null)
            {
                return null;
            }

            var canvas = grid.FindName(DrawCanvasName) as Canvas;
            if (canvas == null)
            {
                canvas = new Canvas();
                canvas.Name = DrawCanvasName;
                canvas.Opacity = 0;
                canvas.Background = new SolidColorBrush(Colors.Transparent);
                canvas.IsHitTestVisible = false;

                if (grid.ColumnDefinitions.Count > 1)
                {
                    Grid.SetColumnSpan(canvas, grid.ColumnDefinitions.Count);
                }

                if (grid.RowDefinitions.Count > 1)
                {
                    Grid.SetRowSpan(canvas, grid.RowDefinitions.Count);
                }

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

            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(ctrl, width, height);

            var result = rtb;

            canvas.Children.Clear();

            return result;
        }
    }
}