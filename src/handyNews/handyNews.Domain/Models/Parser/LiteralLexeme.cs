using handyNews.Domain.Utils;

namespace handyNews.Domain.Models.Parser
{
    public class LiteralLexeme : ILexeme
    {
        public LiteralLexeme(string text)
        {
            Text = text.ConvertHtmlToText();
        }

        public string Text { get; }
    }
}