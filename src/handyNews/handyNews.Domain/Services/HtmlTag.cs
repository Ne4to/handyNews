namespace handyNews.Domain.Services
{
    public static class HtmlTag
    {
        /// <summary>
        ///     Line break.
        ///     Legitimate line breaks can occur in the likes of addresses or poetry — when they are a meaningful part of the
        ///     content.They should not be used for purely stylistic reasons, such as replicating the look of a paragraph(when a p
        ///     element would be appropriate).
        ///     br has no content and therefore does not warrant a closing tag.
        /// </summary>
        public const string LINE_BREAK = "br";

        /// <summary>
        ///     Image. This could be a photograph or graph or any other meaningful pictorial content. It should not be used purely
        ///     for presentation, where CSS, such as background-image is more appropriate.
        /// </summary>
        public const string IMAGE = "img";

        /// <summary>
        ///     A paragraph.
        /// </summary>
        public const string PARAGRAPH = "p";

        /// <summary>
        ///     List item. Used in conjunction with ul or ol to make an unordered list or ordered list respectively.
        /// </summary>
        public const string LIST_ITEM = "li";

        /// <summary>
        ///     Division — a generic container for a block of HTML. Lending no additional meaning, it is typically used as
        ///     scaffolding to hang CSS on or for JavaScript to reach out to.
        /// </summary>
        public const string DIVISION = "div";

        /// <summary>
        ///     Anchor. Primarily used as a hypertext link. The link can be to another page, a part of a page or any other location
        ///     on the web.
        /// </summary>
        public const string ANCHOR = "a";

        // TODO Add suppport for Preformatted text
        /// <summary>
        ///     Preformatted text. All of the whitespace within this element is regarded as semantically relevant (whereas other
        ///     elements will consider the likes of indentations or multiple consecutive spaces to be meaningless).
        ///     Commonly used to format computer code because it maintains nesting indentations.
        /// </summary>
        public const string PREFORMATTED_TEXT = "pre";

        /// <summary>
        ///     Text in an alternate voice or representing a different quality of text.
        /// </summary>
        public const string ALTERNATE_VOICE = "i";

        /// <summary>
        ///     Strong importance — used to highlight the part of a sentence that matters most, a warning, or something the user
        ///     should act on quickly, etc.
        /// </summary>
        public const string STRONG_IMPORTANCE = "strong";

        /// <summary>
        ///     Emphasis, as in stressing a word or phrase in a sentence.
        /// </summary>
        public const string EMPHASIS = "em";

        /// <summary>
        ///     An inline frame, or nested browsing context — essentially, a web page embedded within a web page.
        /// </summary>
        public const string INLINE_FRAME = "iframe";

        /// <summary>
        ///     External resource, treated as an image, a nested browsing context, or a plugin.
        /// </summary>
        public const string EXTERNAL_RESOURCE = "object";

        /// <summary>
        ///     Plugin point — where an external, typically non-HTML, application or resource can be embedded in a page.
        /// </summary>
        public const string PLUGIN_POINT = "embed";

        /// <summary>
        ///     Ranked headings, h1 being the top-level heading, and h6 being the lowest level heading.
        /// </summary>
        public const string RANKED_HEADINGS1 = "h1";

        /// <summary>
        ///     Ranked headings, h1 being the top-level heading, and h6 being the lowest level heading.
        /// </summary>
        public const string RANKED_HEADINGS2 = "h2";

        /// <summary>
        ///     Ranked headings, h1 being the top-level heading, and h6 being the lowest level heading.
        /// </summary>
        public const string RANKED_HEADINGS3 = "h3";

        /// <summary>
        ///     Ranked headings, h1 being the top-level heading, and h6 being the lowest level heading.
        /// </summary>
        public const string RANKED_HEADINGS4 = "h4";

        /// <summary>
        ///     Ranked headings, h1 being the top-level heading, and h6 being the lowest level heading.
        /// </summary>
        public const string RANKED_HEADINGS5 = "h5";

        /// <summary>
        ///     Ranked headings, h1 being the top-level heading, and h6 being the lowest level heading.
        /// </summary>
        public const string RANKED_HEADINGS6 = "h6";
    }
}