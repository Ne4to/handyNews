using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel;
using Windows.Data.Html;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.ServiceLocation;

namespace Inoreader.Services
{
	public class HtmlParser
	{
		private readonly TelemetryClient _telemetry;
		private readonly AppSettingsService _appSettings;
		private readonly List<Image> _allImages = new List<Image>();
		private readonly double _maxImageWidth;

		public HtmlParser()
		{
			if (DesignMode.DesignModeEnabled)
				return;

			_appSettings = ServiceLocator.Current.GetInstance<AppSettingsService>();
			_telemetry = ServiceLocator.Current.GetInstance<TelemetryClient>();

			var displayInformation = DisplayInformation.GetForCurrentView();
			_maxImageWidth = ImageManager.GetMaxImageWidth(displayInformation);
		}

		public Paragraph GetParagraph(string html, out IList<Image> images)
		{
			if (html == null)
			{
				images = new Image[0];
				return new Paragraph();
			}

			try
			{
				var strings = GetStrings(html);
				var lexemes = GetLexemes(strings);

				var paragraph = new Paragraph();
				paragraph.TextAlignment = _appSettings.TextAlignment;

				for (int lexemeIndex = 0; lexemeIndex < lexemes.Length; lexemeIndex++)
				{
					var lexeme = lexemes[lexemeIndex];

					var literalLexeme = lexeme as LiteralLexeme;
					if (literalLexeme != null)
					{
						paragraph.Inlines.Add(new Run { Text = literalLexeme.Text, FontSize = _appSettings.FontSize });
						continue;
					}

					var tagLexeme = (HtmlTagLexeme)lexeme;
					if (String.Equals(tagLexeme.Name, "br", StringComparison.OrdinalIgnoreCase))
					{
						paragraph.Inlines.Add(new LineBreak());
						continue;
					}

					TryAddYoutubeVideo(tagLexeme, paragraph.Inlines);

					if (String.Equals(tagLexeme.Name, "img", StringComparison.OrdinalIgnoreCase))
					{
						var image = CreateImage(tagLexeme);
						if (image == null)
							continue;

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

				images = _allImages;
				return paragraph;
			}
			catch (Exception e)
			{
				_telemetry.TrackException(e);
				images = _allImages;
				return new Paragraph();
			}
		}

		private Image CreateImage(HtmlTagLexeme tagLexeme)
		{
			var src = tagLexeme.Attributes["src"];
			if (src.StartsWith("https://www.inoreader.com/b/", StringComparison.OrdinalIgnoreCase))
				return null;

			return CreateImage(src);
		}

		private Image CreateImage(string src)
		{
			var image = new Image
			{
				Source = new BitmapImage(new Uri(src)),
			};

			_allImages.Add(image);

			image.ImageOpened += (sender, args) =>
			{
				var img = (Image)sender;
				ImageManager.UpdateImageSize(img, _maxImageWidth);
			};
			return image;
		}

		private void AddBeginEnd(InlineCollection inlines, ILexeme[] lexemes, int lexemeIndex, int closeIndex, StringParameters strParams)
		{
			var startL = (HtmlTagLexeme)lexemes[lexemeIndex];

			if (String.Equals(startL.Name, "p", StringComparison.OrdinalIgnoreCase))
			{
				inlines.Add(new LineBreak());
			}

			if (String.Equals(startL.Name, "li", StringComparison.OrdinalIgnoreCase))
			{
				inlines.Add(new Run() { Text = "• ", FontSize = _appSettings.FontSize });
			}

			if (String.Equals(startL.Name, "div", StringComparison.OrdinalIgnoreCase))
			{
				inlines.Add(new LineBreak());
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
						hyperlink.Inlines.Add(new Run { Text = literalLexeme.Text, FontSize = _appSettings.FontSize });
						inlines.Add(hyperlink);
						inlines.Add(new Run { Text = " ", FontSize = _appSettings.FontSize });
						return;
					}

					var tagLexeme = (HtmlTagLexeme)lexeme;
					if (String.Equals(tagLexeme.Name, "img", StringComparison.OrdinalIgnoreCase))
					{
						var image = CreateImage(tagLexeme);
						if (image == null)
							return;

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
						inlines.Add(new LineBreak());
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
					item.Inlines.Add(new Run { Text = literalLexeme.Text, FontSize = _appSettings.FontSize });
					inlines.Add(item);
				}
				return;
			}

			SetHeadersValue(strParams, startL.Name, true);
			if (String.Equals(startL.Name, "em", StringComparison.OrdinalIgnoreCase))
			{
				strParams.Italic = true;
			}

			if (String.Equals(startL.Name, "i", StringComparison.OrdinalIgnoreCase))
			{
				strParams.Italic = true;
			}

			for (int index = lexemeIndex + 1; index < closeIndex; index++)
			{
				var lexeme = lexemes[index];

				var literalLexeme = lexeme as LiteralLexeme;
				if (literalLexeme != null)
				{
					var item = new Run { Text = literalLexeme.Text, FontSize = _appSettings.FontSize };

					item.FontSize = GetFontSize(strParams);
					if (IsHtmlHeader(strParams))
						item.FontWeight = FontWeights.Bold;
					
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

				if (String.Equals(tagLexeme.Name, "img", StringComparison.OrdinalIgnoreCase)
					&& tagLexeme.IsOpen
					&& tagLexeme.IsClose)
				{
					var image = CreateImage(tagLexeme);
					if (image != null)
					{
						var inlineUiContainer = new InlineUIContainer
						{
							Child = image
						};

						inlines.Add(inlineUiContainer);
						inlines.Add(new LineBreak());
					}
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

			if (String.Equals(startL.Name, "i", StringComparison.OrdinalIgnoreCase))
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

			if (String.Equals(startL.Name, "div", StringComparison.OrdinalIgnoreCase))
			{
				inlines.Add(new LineBreak());
			}
		}

		private void SetHeadersValue(StringParameters strParams, string tagName, bool value)
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

		private double GetFontSize(StringParameters strParams)
		{
			if (strParams.H1)
				return _appSettings.FontSizeH1;

			if (strParams.H2)
				return _appSettings.FontSizeH2;

			if (strParams.H3)
				return _appSettings.FontSizeH3;

			if (strParams.H4)
				return _appSettings.FontSizeH4;

			if (strParams.H5)
				return _appSettings.FontSizeH5;

			if (strParams.H6)
				return _appSettings.FontSizeH6;

			return _appSettings.FontSize;
		}

		private bool IsHtmlHeader(StringParameters strParams)
		{
			return strParams.H1
			       || strParams.H2
			       || strParams.H3
			       || strParams.H4
			       || strParams.H5
			       || strParams.H6;
		}

		private int GetCloseIndex(int startLexemeIndex, ILexeme[] lexemes)
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

		private List<string> GetStrings(string html)
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

		private ILexeme[] GetLexemes(List<string> lexemes)
		{
			var q = from l in lexemes
					let isTag = l[0] == '<' && l[l.Length - 1] == '>'
					select isTag ? (ILexeme)GetHtmlTag(l) : (ILexeme)(new LiteralLexeme(l));

			return q.ToArray();
		}

		private HtmlTagLexeme GetHtmlTag(string token)
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

		public string GetPlainText(string html, int maxLength)
		{
			if (html == null)
				return String.Empty;

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
				return String.Empty;
			}
		}

		private static readonly string[] YoutubeLinks = new[]
		{
			"http://www.youtube.com/embed/",
			"https://www.youtube.com/embed/",
			"http://youtube.com/embed/",
			"https://youtube.com/embed/"
		};

		private const string YoutubePreviewFormat = "http://img.youtube.com/vi/{0}/0.jpg";

		private void TryAddYoutubeVideo(HtmlTagLexeme tagLexeme, InlineCollection inlines)
		{
			if (String.Equals(tagLexeme.Name, "iframe", StringComparison.OrdinalIgnoreCase))
			{
				string videoLink;
				if (!tagLexeme.Attributes.TryGetValue("src", out videoLink))
					return;

				AddYoutubeLink(videoLink, inlines);
				return;
			}

			if (String.Equals(tagLexeme.Name, "object", StringComparison.OrdinalIgnoreCase))
			{
				string videoLink;
				if (!tagLexeme.Attributes.TryGetValue("data", out videoLink))
					return;

				AddYoutubeLink(videoLink, inlines);
				return;
			}

			if (String.Equals(tagLexeme.Name, "embed", StringComparison.OrdinalIgnoreCase))
			{
				string videoLink;
				if (!tagLexeme.Attributes.TryGetValue("src", out videoLink))
					return;

				AddYoutubeLink(videoLink, inlines);
			}
		}

		private void AddYoutubeLink(string videoLink, InlineCollection inlines)
		{
			videoLink = videoLink.Trim();
			foreach (var testLink in YoutubeLinks)
			{
				if (videoLink.StartsWith(testLink, StringComparison.OrdinalIgnoreCase))
				{
					var id = videoLink.Substring(testLink.Length).Replace("/", string.Empty);
					AddYoutubeLink(id, videoLink, inlines);
				}
			}
		}

		private void AddYoutubeLink(string id, string videoLink, InlineCollection inlines)
		{
			var imageUrl = String.Format(YoutubePreviewFormat, id);
			
			var navigateUri = new Uri(videoLink);
			var image = CreateImage(imageUrl);
			image.IsTapEnabled = true;
			image.Tapped += async (sender, args) =>
			{
				await Launcher.LaunchUriAsync(navigateUri);
			};

			var inlineUiContainer = new InlineUIContainer
			{
				Child = image
			};

			inlines.Add(new Run
			{
				Text = Strings.Resources.YoutubeVideoTitle,
				FontSize = _appSettings.FontSize,
				FontStyle = FontStyle.Italic,				
			});


			inlines.Add(new LineBreak());
			inlines.Add(inlineUiContainer);
			inlines.Add(new LineBreak());
		}
	}
}