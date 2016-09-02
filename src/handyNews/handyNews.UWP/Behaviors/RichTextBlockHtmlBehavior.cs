using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.Domain.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xaml.Interactivity;

namespace handyNews.UWP.Behaviors
{
    public class RichTextBlockHtmlBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty HtmlContentProperty = DependencyProperty.Register(
            "HtmlContent", typeof(object), typeof(RichTextBlockHtmlBehavior),
            new PropertyMetadata(null, OnHtmlContentPropertyChanged));

        private long _callbackToken;
        private bool _created;

        public object HtmlContent
        {
            get { return GetValue(HtmlContentProperty); }
            set { SetValue(HtmlContentProperty, value); }
        }

        public DependencyObject AssociatedObject { get; private set; }

        public void Detach()
        {
            AssociatedObject?.UnregisterPropertyChangedCallback(UIElement.VisibilityProperty, _callbackToken);
            AssociatedObject = null;
        }

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;

            var richTextBlock = (RichTextBlock) AssociatedObject;
            richTextBlock.Loaded += richTextBlock_Loaded;

            _callbackToken = AssociatedObject.RegisterPropertyChangedCallback(UIElement.VisibilityProperty,
                                                                              OnVisibilityPropertyChanged);
        }

        private void OnVisibilityPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            var richTextBlock = (RichTextBlock) AssociatedObject;
            if (!_created && (richTextBlock.Visibility == Visibility.Visible) && HtmlContent is string)
            {
                UpdateTextBlock(richTextBlock);
            }
        }

        private void richTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var richTextBlock = (RichTextBlock) AssociatedObject;
            if ((richTextBlock.Visibility == Visibility.Visible) && HtmlContent is string &&
                (richTextBlock.Blocks.Count == 0))
            {
                UpdateTextBlock(richTextBlock);
            }
        }

        private void UpdateTextBlock(RichTextBlock richTextBlock)
        {
            var builder = new RichTextBlockBuilder();
            IList<Image> images;
            var paragraphs = builder.GetParagraphs((string) HtmlContent, out images);

            foreach (var paragraph in paragraphs)
                richTextBlock.Blocks.Add(paragraph);

            _created = true;

            var imageManager = ServiceLocator.Current.GetInstance<ImageManager>();
            imageManager.RegisterBlock(richTextBlock, images);
        }

        private static void OnHtmlContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RichTextBlockHtmlBehavior) d).OnHtmlContentChanged();
        }

        private void OnHtmlContentChanged()
        {
            var richTextBlock = (RichTextBlock) AssociatedObject;
            if (richTextBlock == null)
            {
                return;
            }

            _created = false;
            richTextBlock.Blocks.Clear();

            if ((richTextBlock.Visibility == Visibility.Visible) && HtmlContent is string)
            {
                UpdateTextBlock(richTextBlock);
            }
        }
    }
}