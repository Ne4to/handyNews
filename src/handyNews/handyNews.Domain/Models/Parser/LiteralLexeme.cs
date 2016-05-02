namespace handyNews.Domain.Models.Parser
{
    public class LiteralLexeme : ILexeme
    {
        public string Text { get; set; }

        public LiteralLexeme(string text)
        {
            // TODO implement fast version of HtmlUtilities.ConvertToText(text);
            Text = text;
        }
    }
}