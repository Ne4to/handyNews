using Windows.Data.Html;

namespace handyNews.Domain.Models.Parser
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