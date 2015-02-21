using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using Windows.Data.Html;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

namespace Inoreader.Services
{
	public class RichTextBlockExtensions
	{
		public static readonly DependencyProperty HtmlContentProperty = DependencyProperty.RegisterAttached("HtmlContent", typeof(string), typeof(RichTextBlockExtensions), new PropertyMetadata(default(string), new PropertyChangedCallback(HtmlContentChanged)));
		
		public static string GetHtmlContent(RichTextBlock element)
		{
			return (string)element.GetValue(HtmlContentProperty);
		}

		public static void SetHtmlContent(RichTextBlock element, string value)
		{
			element.SetValue(HtmlContentProperty, value);
		}

		private static void HtmlContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var richTextBlock = d as RichTextBlock;
			if (richTextBlock != null)
			{
				richTextBlock.Blocks.Clear();
				var paragraph = HtmlParser.GetParagraph(e.NewValue as String);
				richTextBlock.Blocks.Add(paragraph);
				//var paragrapth = new HtmlToParagraphConvertor().GetParagraps(e.NewValue as string);
				//richTextBlock.Blocks.Clear();
				//richTextBlock.Blocks.Add(paragrapth);
			}
		}
	}
}