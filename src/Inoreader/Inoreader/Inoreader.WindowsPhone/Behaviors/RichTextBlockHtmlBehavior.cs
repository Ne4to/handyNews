using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.Domain.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xaml.Interactivity;

namespace Inoreader.Behaviors
{
    public class RichTextBlockHtmlBehavior : DependencyObject, IBehavior
    {
#if WINDOWS_UWP
        private long _callbackToken;
#else
        private DependencyPropertyChangedHelper _helper;
#endif
        private bool _created;

        public static readonly DependencyProperty HtmlContentProperty = DependencyProperty.Register(
            "HtmlContent", typeof(object), typeof(RichTextBlockHtmlBehavior), new PropertyMetadata(null, OnHtmlContentPropertyChanged));

        public object HtmlContent
        {
            get { return GetValue(HtmlContentProperty); }
            set { SetValue(HtmlContentProperty, value); }
        }

        public void Attach(DependencyObject associatedObject)
        {
            //DependencyObject o;
            //o.RegisterPropertyChangedCallback()

            AssociatedObject = associatedObject;

            var richTextBlock = (RichTextBlock)AssociatedObject;
            richTextBlock.Loaded += richTextBlock_Loaded;

#if WINDOWS_UWP
            _callbackToken = AssociatedObject.RegisterPropertyChangedCallback(UIElement.VisibilityProperty, OnVisibilityPropertyChanged);
#else
            _helper = new DependencyPropertyChangedHelper(richTextBlock, "Visibility");
            _helper.PropertyChanged += RichTextBlock_VisibilityPropertyChanged;            
#endif
        }

#if WINDOWS_UWP
        private void OnVisibilityPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            var richTextBlock = (RichTextBlock)AssociatedObject;
            if (!_created && richTextBlock.Visibility == Visibility.Visible && HtmlContent is string)
            {
                UpdateTextBlock(richTextBlock);
            }
        }
#endif

        void richTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var richTextBlock = (RichTextBlock)AssociatedObject;
            if (richTextBlock.Visibility == Visibility.Visible && HtmlContent is string && richTextBlock.Blocks.Count == 0)
            {
                UpdateTextBlock(richTextBlock);
            }
        }

        private void UpdateTextBlock(RichTextBlock richTextBlock)
        {
            var builder = new RichTextBlockBuilder();
            IList<Image> images;
            var paragraphs = builder.GetParagraphs((string)HtmlContent, out images);

            foreach (var paragraph in paragraphs)
            {
                richTextBlock.Blocks.Add(paragraph);
            }

            _created = true;

            var imageManager = ServiceLocator.Current.GetInstance<ImageManager>();
            imageManager.RegisterBlock(richTextBlock, images);
        }

        private static void OnHtmlContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RichTextBlockHtmlBehavior)d).OnHtmlContentChanged();
        }

        private void OnHtmlContentChanged()
        {
            var richTextBlock = (RichTextBlock)AssociatedObject;
            if (richTextBlock == null)
                return;

            _created = false;
            richTextBlock.Blocks.Clear();

            if (richTextBlock.Visibility == Visibility.Visible && HtmlContent is string)
            {
                UpdateTextBlock(richTextBlock);
            }
        }

#if !WINDOWS_UWP
        void RichTextBlock_VisibilityPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var richTextBlock = (RichTextBlock)AssociatedObject;
            if (!_created && richTextBlock.Visibility == Visibility.Visible && HtmlContent is string)
            {
                UpdateTextBlock(richTextBlock);
            }
        }
#endif

        public void Detach()
        {
#if WINDOWS_UWP
            AssociatedObject?.UnregisterPropertyChangedCallback(UIElement.VisibilityProperty, _callbackToken);
#else
            _helper.PropertyChanged -= RichTextBlock_VisibilityPropertyChanged;
#endif

            AssociatedObject = null;
        }

        public DependencyObject AssociatedObject
        {
            get;
            private set;
        }
    }
}