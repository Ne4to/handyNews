using System;
using System.Collections.Generic;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace handyNews.Domain.Services
{
    public class ImageManager
    {
        // 36 = 6 (unread rectangle) + 5*2 (margin 5,0) + 20 (star control)
        public const double StremPageImageHorizontalPadding = 36D;
        // 10 = 5*2 (margin 5,0)
        public const double SavedPageImageHorizontalPadding = 10D;

        public static readonly DependencyProperty ImageHorizontalPaddingProperty = DependencyProperty.RegisterAttached(
            "ImageHorizontalPadding", typeof(double), typeof(ImageManager), new PropertyMetadata(default(double)));

        private readonly Dictionary<RichTextBlock, IList<Image>> _allBlocks =
            new Dictionary<RichTextBlock, IList<Image>>();

        public ImageManager()
        {
            DisplayInformation.GetForCurrentView()
                              .OrientationChanged += DisplayInformation_OrientationChanged;
        }

        public static void SetImageHorizontalPadding(DependencyObject element, double value)
        {
            element.SetValue(ImageHorizontalPaddingProperty, value);
        }

        public static double GetImageHorizontalPadding(DependencyObject element)
        {
            return (double) element.GetValue(ImageHorizontalPaddingProperty);
        }

        public void RegisterBlock(RichTextBlock textBlock, IList<Image> images)
        {
            IList<Image> oldImages;
            if (_allBlocks.TryGetValue(textBlock, out oldImages))
            {
                foreach (var oldImage in oldImages)
                    oldImage.Unloaded -= textBlock_Unloaded;
            }

            _allBlocks[textBlock] = images;
            textBlock.Unloaded += textBlock_Unloaded;
        }

        private void textBlock_Unloaded(object sender, RoutedEventArgs e)
        {
            var textBlock = (RichTextBlock) sender;
            textBlock.Unloaded -= textBlock_Unloaded;
            _allBlocks.Remove(textBlock);
        }

        public static double GetMaxImageWidth(DisplayInformation display)
        {
            var maxImageWidth = Window.Current.Bounds.Width;

            if ((display.CurrentOrientation == DisplayOrientations.Landscape)
                || (display.CurrentOrientation == DisplayOrientations.LandscapeFlipped))
            {
                var frame = Window.Current.Content as Frame;
                if (frame != null)
                {
                    var page = frame.Content as Page;
                    if (page != null)
                    {
                        var bottomAppBar = page.BottomAppBar;

                        if (bottomAppBar != null)
                        {
                            maxImageWidth -= bottomAppBar.ActualHeight;
                        }
                    }
                }
            }
            return maxImageWidth;
        }

        public static void UpdateImageSize(Image image, double maxImageWidth)
        {
            var imgSource = image.Source as BitmapImage;
            if (imgSource == null)
            {
                return;
            }

            var width = Math.Min(imgSource.PixelWidth, maxImageWidth);

            if (!(Math.Abs(width) > 0.1D))
            {
                return;
            }

            var k = width/imgSource.PixelWidth;

            image.Width = width;
            image.Height = imgSource.PixelHeight*k;
        }

        private void DisplayInformation_OrientationChanged(DisplayInformation display, object args)
        {
            var maxImageWidth = GetMaxImageWidth(display) - StremPageImageHorizontalPadding;

            foreach (var list in _allBlocks.Values)
                foreach (var image in list)
                    UpdateImageSize(image, maxImageWidth);
        }
    }
}