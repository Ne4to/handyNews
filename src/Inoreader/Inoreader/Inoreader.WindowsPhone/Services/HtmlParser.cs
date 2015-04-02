using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Data.Html;

namespace Inoreader.Services
{
	public class HtmlParser
	{
		private static readonly Regex RemoveAdRegex;

		static HtmlParser()
		{
			RemoveAdRegex = new Regex("(<center>).*(www.inoreader.com/adv).*(</center>)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		}

		public string GetPlainText(string html, int maxLength)
		{
			if (html == null)
				return String.Empty;

			html = RemoveAdRegex.Replace(html, String.Empty);
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
	
		public ILexeme[] Parse(string html)
		{
			var strings = GetStrings(html);
			return GetLexemes(strings);
		}

		private List<string> GetStrings(string html)
		{
			html = RemoveAdRegex.Replace(html, String.Empty);

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
					if (currentIndex < html.Length - 1)
					{
						tokens.Add(html.Substring(currentIndex));
					}
					break;
				}
			}

			tokens.RemoveAll(String.IsNullOrWhiteSpace);
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
	}
}