using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

namespace Inoreader.Services
{
	public class RichTextBlockExtensions
	{
		public static readonly DependencyProperty HtmlContentProperty = DependencyProperty.RegisterAttached("HtmlContent", typeof(string), typeof(RichTextBlockExtensions), new PropertyMetadata(default(string), new PropertyChangedCallback(HtmlContentChanged)));

		private static void HtmlContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var richTextBlock = d as RichTextBlock;
			if (richTextBlock != null)
			{
				richTextBlock.Blocks.Clear();
				var paragraph = GetParagraph(e.NewValue as String);
				richTextBlock.Blocks.Add(paragraph);
				//var paragrapth = new HtmlToParagraphConvertor().GetParagraps(e.NewValue as string);
				//richTextBlock.Blocks.Clear();
				//richTextBlock.Blocks.Add(paragrapth);
			}
		}

		private static Paragraph GetParagraph(string html)
		{
			if (html == null)
				return new Paragraph();

			var strings = GetStrings(html);
			var lexemes = GetLexemes(strings);

			var paragraph = new Paragraph();

			for (int lexemeIndex = 0; lexemeIndex < lexemes.Length; lexemeIndex++)
			{
				var lexeme = lexemes[lexemeIndex];

				var literalLexeme = lexeme as LiteralLexeme;
				if (literalLexeme != null)
				{
					paragraph.Inlines.Add(new Run { Text = literalLexeme.Text });
					continue;
				}

				var tagLexeme = (HtmlTagLexeme)lexeme;
				if (String.Equals(tagLexeme.Name, "br", StringComparison.OrdinalIgnoreCase))
				{
					paragraph.Inlines.Add(new LineBreak());
					continue;
				}

				if (tagLexeme.IsOpen && !tagLexeme.IsClose)
				{
					var closeIndex = GetCloseIndex(lexemeIndex, lexemes);
					if (closeIndex != -1)
					{
						AddBeginEnd(paragraph.Inlines, lexemes, lexemeIndex, closeIndex);
						lexemeIndex = closeIndex;
					}
				}
			}

			return paragraph;
		}

		private static void AddBeginEnd(InlineCollection inlines, ILexeme[] lexemes, int lexemeIndex, int closeIndex)
		{
			var startL = (HtmlTagLexeme)lexemes[lexemeIndex];

			if (String.Equals(startL.Name, "p", StringComparison.OrdinalIgnoreCase))
			{
				inlines.Add(new LineBreak());
			}

			if (String.Equals(startL.Name, "li", StringComparison.OrdinalIgnoreCase))
			{
				inlines.Add(new Run() { Text = "• " });
			}

			if (String.Equals(startL.Name, "a", StringComparison.OrdinalIgnoreCase))
			{
				string href;
				if (startL.Attributes.TryGetValue("href", out href))
				{
					// Inoreader AD
					if (href.StartsWith(@"https://www.inoreader.com/b/", StringComparison.OrdinalIgnoreCase))
						return;

					var navigateUri = new Uri(href);

					var lexeme = lexemes[lexemeIndex + 1];
					var literalLexeme = lexeme as LiteralLexeme;
					if (literalLexeme != null)
					{
						var hyperlink = new Hyperlink { NavigateUri = navigateUri };
						hyperlink.Inlines.Add(new Run { Text = literalLexeme.Text });
						inlines.Add(hyperlink);
						return;
					}

					var tagLexeme = (HtmlTagLexeme)lexeme;
					if (String.Equals(tagLexeme.Name, "img", StringComparison.OrdinalIgnoreCase))
					{
						var image = new Image
						{
							Source = new BitmapImage(new Uri(tagLexeme.Attributes["src"]))
						};

						image.IsTapEnabled = true;
						image.Tapped += async (sender, args) =>
						{
							// 
							await Launcher.LaunchUriAsync(navigateUri);
						};

						var inlineUiContainer = new InlineUIContainer
						{
							Child = image
						};

						inlines.Add(inlineUiContainer);
						//hyperlink.Inlines.Add(inlineUiContainer);
					}

					return;
				}
			}

			if (String.Equals(startL.Name, "strong", StringComparison.OrdinalIgnoreCase))
			{
				var lexeme = lexemes[lexemeIndex + 1];
				var literalLexeme = lexeme as LiteralLexeme;

				if (literalLexeme != null)
				{
					var item = new Bold();
					item.Inlines.Add(new Run { Text = literalLexeme.Text });
					inlines.Add(item);
				}
				return;
			}

			for (int index = lexemeIndex + 1; index < closeIndex; index++)
			{
				var lexeme = lexemes[index];

				var literalLexeme = lexeme as LiteralLexeme;
				if (literalLexeme != null)
				{
					inlines.Add(new Run() { Text = literalLexeme.Text });
					continue;
				}

				var tagLexeme = (HtmlTagLexeme)lexeme;
				if (String.Equals(tagLexeme.Name, "br", StringComparison.OrdinalIgnoreCase))
				{
					inlines.Add(new LineBreak());
					continue;
				}

				if (tagLexeme.IsOpen && !tagLexeme.IsClose)
				{
					var closeIndex2 = GetCloseIndex(index, lexemes);
					if (closeIndex2 != -1)
					{
						AddBeginEnd(inlines, lexemes, index, closeIndex2);
						index = closeIndex2;
					}
				}
			}

			if (String.Equals(startL.Name, "p", StringComparison.OrdinalIgnoreCase))
			{
				inlines.Add(new LineBreak());
			}

			if (String.Equals(startL.Name, "li", StringComparison.OrdinalIgnoreCase))
			{
				inlines.Add(new LineBreak());
			}
		}

		private static int GetCloseIndex(int startLexemeIndex, ILexeme[] lexemes)
		{
			var startLexeme = (HtmlTagLexeme)lexemes[startLexemeIndex];
			int deep = 1;

			for (int index = startLexemeIndex + 1; index < lexemes.Length; index++)
			{
				var nextLexeme = lexemes[index] as HtmlTagLexeme;
				if (nextLexeme == null)
					continue;

				if (!String.Equals(startLexeme.Name, nextLexeme.Name, StringComparison.OrdinalIgnoreCase))
					continue;

				if (nextLexeme.IsOpen && !nextLexeme.IsClose)
				{
					deep++;
					continue;
				}

				if (!nextLexeme.IsOpen && nextLexeme.IsClose)
				{
					deep--;
				}

				if (deep == 0)
					return index;
			}

			return -1;
		}

		private static List<string> GetStrings(string html)
		{
			List<string> tokens = new List<string>(20);
			var currentIndex = 0;

			while (true)
			{
				var index = html.IndexOf('<', currentIndex);
				if (index != -1)
				{
					if (index != currentIndex)
					{
						var zzz = html.Substring(currentIndex, index - currentIndex);
						tokens.Add(zzz);
					}

					var index2 = html.IndexOf('>', index + 1);
					if (index2 != -1)
					{
						var str = html.Substring(index, index2 - index + 1);
						tokens.Add(str);
						currentIndex = index2 + 1;
					}
					else
					{
						throw new Exception("bad format");
					}
				}
				else
				{
					break;
				}
			}

			tokens.RemoveAll(String.IsNullOrEmpty);
			return tokens;
		}

		private static ILexeme[] GetLexemes(List<string> lexemes)
		{
			var q = from l in lexemes
					let isTag = l[0] == '<' && l[l.Length - 1] == '>'
					select isTag ? (ILexeme)GetHtmlTag(l) : (ILexeme)(new LiteralLexeme(l));

			return q.ToArray();
		}

		private static HtmlTagLexeme GetHtmlTag(string token)
		{
			var tag = new HtmlTagLexeme();

			tag.IsOpen = token[1] != '/';
			tag.IsClose = token[1] == '/' || token[token.Length - 2] == '/';

			var spacePos = token.IndexOf(' ');
			if (spacePos != -1)
			{
				tag.Name = token.Substring(1, spacePos - 1);

				var searchAttrStartPos = spacePos;

				//<a href="http://channel9.msdn.com/Shows/This+Week+On+Channel+9/TWC9-NET-Core-OSS-Update-CoreCLR-on-GitHub-Windows-10-for-Raspberry-Pi-2-Super-Bowl-Stories-and-more#time=1m06s">
				//<img src="https://www.inoreader.com/b/1438160281/1008166777" style="position: absolute; visibility: hidden">

				while (true)
				{
					var eqPos = token.IndexOf('=', searchAttrStartPos + 1);
					if (eqPos != -1)
					{
						var attrName = token.Substring(searchAttrStartPos + 1, eqPos - searchAttrStartPos - 1).Trim();
						var quoteSymb = token[eqPos + 1];

						var endQuotePos = token.IndexOf(quoteSymb, eqPos + 2);
						if (endQuotePos != -1)
						{
							var attrValue = token.Substring(eqPos + 2, endQuotePos - eqPos - 2);
							tag.Attributes[attrName] = attrValue;

							searchAttrStartPos = endQuotePos + 1;
						}
						else
						{
							break;
						}
					}
					else
					{
						break;
					}
				}
			}
			else
			{
				if (tag.IsOpen && !tag.IsClose)
				{
					tag.Name = token.Substring(1, token.Length - 2);
				}
				else
				{
					if (!tag.IsOpen && tag.IsClose)
					{
						tag.Name = token.Substring(2, token.Length - 3);
					}
				}
			}

			return tag;
		}

		public static string GetHtmlContent(RichTextBlock element)
		{
			return (string)element.GetValue(HtmlContentProperty);
		}

		public static void SetHtmlContent(RichTextBlock element, string value)
		{
			element.SetValue(HtmlContentProperty, value);
		}
	}

	public interface ILexeme
	{
	}

	public class LiteralLexeme : ILexeme
	{
		public string Text { get; set; }

		public LiteralLexeme(string text)
		{
			Text = text;
		}
	}

	public class HtmlTagLexeme : ILexeme
	{
		public string Name { get; set; }
		public bool IsOpen { get; set; }
		public bool IsClose { get; set; }
		public Dictionary<string, string> Attributes { get; set; }

		public HtmlTagLexeme()
		{
			Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}
	}
}