using Windows.Data.Html;

namespace Inoreader.Domain.Services
{
	public class LiteralLexeme : ILexeme
	{
		public string Text { get; set; }

		public LiteralLexeme(string text)
		{
			Text = HtmlUtilities.ConvertToText(text);
		}
	}
}