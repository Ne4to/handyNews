using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Inoreader.Services;
using Microsoft.Xaml.Interactivity;

namespace Inoreader.Behaviors
{
	public class RichTextBlockHtmlBehavior : DependencyObject, IBehavior
	{
		private DependencyPropertyChangedHelper _helper;
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
			AssociatedObject = associatedObject;
			var richTextBlock = (RichTextBlock)AssociatedObject;
			_helper = new DependencyPropertyChangedHelper(richTextBlock, "Visibility");
			_helper.PropertyChanged += RichTextBlock_VisibilityPropertyChanged;
			richTextBlock.Loaded += richTextBlock_Loaded;
		}

		void richTextBlock_Loaded(object sender, RoutedEventArgs e)
		{
			var richTextBlock = (RichTextBlock)AssociatedObject;
			if (richTextBlock.Visibility == Visibility.Visible && HtmlContent is string && richTextBlock.Blocks.Count == 0)
			{
				var paragraph = HtmlParser.GetParagraph((string)HtmlContent);
				richTextBlock.Blocks.Add(paragraph);
				_created = true;
			}
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
				var paragraph = HtmlParser.GetParagraph((string)HtmlContent);
				richTextBlock.Blocks.Add(paragraph);
				_created = true;
			}
		}

		void RichTextBlock_VisibilityPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var richTextBlock = (RichTextBlock)AssociatedObject;
			if (!_created && richTextBlock.Visibility == Visibility.Visible && HtmlContent is string)
			{
				var paragraph = HtmlParser.GetParagraph((string)HtmlContent);
				richTextBlock.Blocks.Add(paragraph);
				_created = true;
			}
		}

		public void Detach()
		{
			AssociatedObject = null;
			_helper.PropertyChanged -= RichTextBlock_VisibilityPropertyChanged;
		}

		public DependencyObject AssociatedObject
		{
			get;
			private set;
		}
	}
}