using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Data.Html;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.ApplicationInsights;

namespace Inoreader.Services
{
	public class HtmlParser
	{
		private static readonly TelemetryClient _telemetry = new TelemetryClient();

		private const double FontSizeH1 = 28D;
		private const double FontSizeH2 = 24D;
		private const double FontSizeH3 = 20D;
		private const double FontSizeH4 = 17D;
		private const double FontSizeH5 = 14D;
		private const double FontSizeH6 = 12D;

		public static Paragraph GetParagraph(string html)
		{
			if (html == null)
				return new Paragraph();

			try
			{
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

					if (String.Equals(tagLexeme.Name, "img", StringComparison.OrdinalIgnoreCase))
					{
						var src = tagLexeme.Attributes["src"];
						if (src.StartsWith("https://www.inoreader.com/b/", StringComparison.OrdinalIgnoreCase))
							continue;

						string widthStr;
						int width = 0;
						if (tagLexeme.Attributes.TryGetValue("width", out widthStr))
						{
							Int32.TryParse(widthStr, out width);
						}

						string heightStr;
						int height = 0;
						if (tagLexeme.Attributes.TryGetValue("height", out heightStr))
						{
							Int32.TryParse(heightStr, out height);
						}

						var image = new Image
						{
							Source = new BitmapImage(new Uri(src))
						};

						if (width != 0)
							image.Width = width;

						if (height != 0)
							image.Height = height;

						var inlineUiContainer = new InlineUIContainer
						{
							Child = image
						};

						paragraph.Inlines.Add(inlineUiContainer);
						paragraph.Inlines.Add(new LineBreak());
						continue;
					}

					if (tagLexeme.IsOpen && !tagLexeme.IsClose)
					{
						var closeIndex = GetCloseIndex(lexemeIndex, lexemes);
						if (closeIndex != -1)
						{
							var strParams = new StringParameters();
							AddBeginEnd(paragraph.Inlines, lexemes, lexemeIndex, closeIndex, strParams);
							lexemeIndex = closeIndex;
						}
					}
				}

				return paragraph;
			}
			catch (Exception e)
			{
				_telemetry.TrackException(e);
				return new Paragraph();
			}
		}

		private static void AddBeginEnd(InlineCollection inlines, ILexeme[] lexemes, int lexemeIndex, int closeIndex, StringParameters strParams)
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
						var src = tagLexeme.Attributes["src"];
						if (src.StartsWith("https://www.inoreader.com/b/", StringComparison.OrdinalIgnoreCase))
							return;

						var image = new Image
						{
							Source = new BitmapImage(new Uri(src))
						};

						image.IsTapEnabled = true;
						image.Tapped += async (sender, args) =>
						{
							await Launcher.LaunchUriAsync(navigateUri);
						};

						var inlineUiContainer = new InlineUIContainer
						{
							Child = image
						};

						inlines.Add(inlineUiContainer);
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

			SetHeadersValue(strParams, startL.Name, true);
			if (String.Equals(startL.Name, "em", StringComparison.OrdinalIgnoreCase))
			{
				strParams.Italic = true;
			}

			for (int index = lexemeIndex + 1; index < closeIndex; index++)
			{
				var lexeme = lexemes[index];

				var literalLexeme = lexeme as LiteralLexeme;
				if (literalLexeme != null)
				{
					var item = new Run { Text = literalLexeme.Text };

					double? customFontSize = GetFontSize(strParams);
					if (customFontSize.HasValue)
						item.FontSize = customFontSize.Value;

					if (strParams.Italic)
						item.FontStyle = FontStyle.Italic;

					inlines.Add(item);
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
						AddBeginEnd(inlines, lexemes, index, closeIndex2, strParams);
						index = closeIndex2;
					}
				}
			}

			SetHeadersValue(strParams, startL.Name, false);
			if (String.Equals(startL.Name, "em", StringComparison.OrdinalIgnoreCase))
			{
				strParams.Italic = false;
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

		private static void SetHeadersValue(StringParameters strParams, string tagName, bool value)
		{
			switch (tagName.ToLower())
			{
				case "h1":
					strParams.H1 = value;
					return;

				case "h2":
					strParams.H2 = value;
					return;

				case "h3":
					strParams.H3 = value;
					return;

				case "h4":
					strParams.H4 = value;
					return;

				case "h5":
					strParams.H5 = value;
					return;

				case "h6":
					strParams.H6 = value;
					return;
			}
		}

		private static double? GetFontSize(StringParameters strParams)
		{
			if (strParams.H1)
				return FontSizeH1;

			if (strParams.H2)
				return FontSizeH2;

			if (strParams.H3)
				return FontSizeH3;

			if (strParams.H4)
				return FontSizeH4;

			if (strParams.H5)
				return FontSizeH5;

			if (strParams.H6)
				return FontSizeH6;

			return null;
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

		public static string GetPlainText(string html, int maxLength)
		{
			if (html == null)
				return null;

			try
			{
				var x = HtmlUtilities.ConvertToText(html);
				var builder = new StringBuilder(x);
				builder.Replace('\r', ' ');
				builder.Replace('\n', ' ');
				builder.Replace('\t', ' ');

				int currentLength;
				int newLength;

				do
				{
					currentLength = builder.Length;
					builder.Replace("  ", " ");
					newLength = builder.Length;
				} while (currentLength != newLength);

				if (newLength < maxLength)
					return builder.ToString().Trim();

				return builder.ToString().Substring(0, maxLength).Trim();
			}
			catch (Exception e)
			{
				_telemetry.TrackException(e);
				return null;
			}
		}
	}


	class StringParameters
	{
		public bool Bold { get; set; }
		public bool Italic { get; set; }
		public bool H1 { get; set; }
		public bool H2 { get; set; }
		public bool H3 { get; set; }
		public bool H4 { get; set; }
		public bool H5 { get; set; }
		public bool H6 { get; set; }
	}

	public interface ILexeme
	{
	}

	public class LiteralLexeme : ILexeme
	{
		public string Text { get; set; }

		public LiteralLexeme(string text)
		{
			Text = HtmlUtilities.ConvertToText(text);
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