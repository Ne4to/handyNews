namespace handyNews.Domain.Models.Parser
{
    public class LiteralLexeme : ILexeme
    {
        public LiteralLexeme(string text)
        {
            // TODO implement fast version of HtmlUtilities.ConvertToText(text);
            Text = text;
        }

        public string Text { get; set; }
    }
}