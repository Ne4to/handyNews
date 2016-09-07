using handyNews.Domain.Utils;

namespace handyNews.Domain.Models.Parser
{
    public class LiteralLexeme : ILexeme
    {
        public string Text { get; }

        public LiteralLexeme(string text)
        {
            Text = text;
        }
    }
}