using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;
using handyNews.Domain.Models.Parser;
using handyNews.Domain.Services.Interfaces;
using handyNews.Domain.Strings;
using Microsoft.Practices.ServiceLocation;

//using Microsoft.Practices.Prism.Mvvm.Interfaces;

namespace handyNews.Domain.Services
{
    public class RichTextBlockBuilder
    {
        private const string YoutubePreviewFormat = "http://img.youtube.com/vi/{0}/0.jpg";

        private static readonly string[] YoutubeLinks =
        {
            "http://www.youtube.com/embed/",
            "https://www.youtube.com/embed/",
            "http://youtube.com/embed/",
            "https://youtube.com/embed/"
        };

        private readonly List<Image> _allImages = new List<Image>();
        private readonly ISettingsManager _appSettings;
        private readonly CoreDispatcher _dispatcher;
        private readonly HttpClient _httpClient;
        private readonly double _maxImageWidth;
        private readonly ITelemetryManager _telemetry;

        private readonly List<string> _youtubeAllVideos = new List<string>();

        public RichTextBlockBuilder()
        {
            if (DesignMode.DesignModeEnabled)
                return;

            _appSettings = ServiceLocator.Current.GetInstance<ISettingsManager>();
            _telemetry = ServiceLocator.Current.GetInstance<ITelemetryManager>();

            var displayInformation = DisplayInformation.GetForCurrentView();
            _maxImageWidth = ImageManager.GetMaxImageWidth(displayInformation);
            _httpClient =
                new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    AllowAutoRedirect = true
                });
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        public List<Paragraph> GetParagraphs(string html, out IList<Image> images)
        {
            if (html == null)
            {
                images = new Image[0];
                return new List<Paragraph>();
            }

            var allParagraphs = new List<Paragraph>();

            try
            {
                var parser = new HtmlParser();
                var lexemes = parser.Parse(html);

                var paragraph = new Paragraph {TextAlignment = _appSettings.TextAlignment};
                allParagraphs.Add(paragraph);

                for (var lexemeIndex = 0; lexemeIndex < lexemes.Length; lexemeIndex++)
                {
                    var lexeme = lexemes[lexemeIndex];

                    var literalLexeme = lexeme as LiteralLexeme;
                    if (literalLexeme != null)
                    {
                        Inline i = new Run();
                        //paragraph.Inlines.Add(new);
                        paragraph.Inlines.Add(new Run {Text = literalLexeme.Text, FontSize = _appSettings.FontSize});
                        continue;
                    }

                    var tagLexeme = (HtmlTagLexeme) lexeme;
                    if (string.Equals(tagLexeme.Name, "br", StringComparison.OrdinalIgnoreCase))
                    {
                        AddLineBreak(paragraph.Inlines);
                        continue;
                    }

                    TryAddYoutubeVideo(tagLexeme, paragraph.Inlines);

                    if (string.Equals(tagLexeme.Name, "img", StringComparison.OrdinalIgnoreCase))
                    {
                        AddImage(paragraph.Inlines, tagLexeme, null);
                        continue;
                    }

                    if (tagLexeme.IsOpen && !tagLexeme.IsClose)
                    {
                        //paragraph = new Paragraph { TextAlignment = _appSettings.TextAlignment };
                        //allParagraphs.Add(paragraph);

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
                return allParagraphs;
            }
            catch (Exception e)
            {
                _telemetry.TrackError(e);
                images = _allImages;
                return new List<Paragraph>();
            }
        }

        private void AddImage(InlineCollection inlines, HtmlTagLexeme tagLexeme, string navigationUri)
        {
            var src = tagLexeme.Attributes["src"];
            if (src.StartsWith("https://www.inoreader.com/b/", StringComparison.OrdinalIgnoreCase))
                return;

            AddImage(inlines, src, navigationUri);
        }

        private async void AddImage(InlineCollection inlines, string src, string navigationUri)
        {
            var image = new Image();
            var imgPadding = src.StartsWith("ms-appdata")
                ? ImageManager.SavedPageImageHorizontalPadding
                : ImageManager.StremPageImageHorizontalPadding;
            ImageManager.SetImageHorizontalPadding(image, imgPadding);
            _allImages.Add(image);

            var inlineUiContainer = new InlineUIContainer
            {
                Child = image
            };
            inlines.Add(inlineUiContainer);
            AddLineBreak(inlines);

            image.ImageOpened += (sender, args) =>
            {
                var img = (Image) sender;
                var padding = ImageManager.GetImageHorizontalPadding(img);
                ImageManager.UpdateImageSize(img, _maxImageWidth - padding);
            };

            if (!string.IsNullOrWhiteSpace(navigationUri))
            {
                image.IsTapEnabled = true;
                var navigateUri = new Uri(navigationUri);

                image.Tapped += async (sender, args) =>
                {
                    args.Handled = true;
                    await Launcher.LaunchUriAsync(navigateUri);
                };
            }

            if (src.StartsWith("ms-appdata"))
            {
                image.Source = new BitmapImage(new Uri(src));
            }
            else
            {
                var response = await _httpClient.GetAsync(src);
                if (!response.IsSuccessStatusCode)
                    return;

                var tempFileName = Path.GetRandomFileName();
                var file =
                    await
                        ApplicationData.Current.TemporaryFolder.CreateFileAsync(tempFileName,
                            CreationCollisionOption.GenerateUniqueName);

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    await response.Content.CopyToAsync(stream);
                }

                var localCopyUri = new Uri("ms-appdata:///temp/" + file.Name);
                image.Source = new BitmapImage(localCopyUri);
            }
        }

        private void AddBeginEnd(InlineCollection inlines, ILexeme[] lexemes, int lexemeIndex, int closeIndex,
            StringParameters strParams)
        {
            var startL = (HtmlTagLexeme) lexemes[lexemeIndex];

            if (string.Equals(startL.Name, "p", StringComparison.OrdinalIgnoreCase))
            {
                AddLineBreak(inlines);
            }

            if (string.Equals(startL.Name, "li", StringComparison.OrdinalIgnoreCase))
            {
                inlines.Add(new Run {Text = "• ", FontSize = _appSettings.FontSize});
            }

            if (string.Equals(startL.Name, "div", StringComparison.OrdinalIgnoreCase))
            {
                AddLineBreak(inlines);
            }

            if (string.Equals(startL.Name, "a", StringComparison.OrdinalIgnoreCase))
            {
                string href;
                if (startL.Attributes.TryGetValue("href", out href))
                {
                    // skip Inoreader AD
                    if (href.StartsWith(@"https://www.inoreader.com/b/", StringComparison.OrdinalIgnoreCase))
                        return;

                    Uri tmp;
                    if (Uri.TryCreate(href, UriKind.Absolute, out tmp))
                        strParams.NavigateUri = href;
                }
            }

            if (string.Equals(startL.Name, "strong", StringComparison.OrdinalIgnoreCase))
            {
                strParams.Bold = true;
            }

            SetHeadersValue(strParams, startL.Name, true);
            if (string.Equals(startL.Name, "em", StringComparison.OrdinalIgnoreCase))
            {
                strParams.Italic = true;
            }

            if (string.Equals(startL.Name, "i", StringComparison.OrdinalIgnoreCase))
            {
                strParams.Italic = true;
            }

            TryAddYoutubeVideo(startL, inlines);

            for (var index = lexemeIndex + 1; index < closeIndex; index++)
            {
                var lexeme = lexemes[index];

                var literalLexeme = lexeme as LiteralLexeme;
                if (literalLexeme != null)
                {
                    var fontSize = GetFontSize(strParams);

                    if (string.IsNullOrWhiteSpace(strParams.NavigateUri))
                    {
                        var item = new Run {Text = literalLexeme.Text, FontSize = fontSize};

                        if (IsHtmlHeader(strParams))
                            item.FontWeight = FontWeights.Bold;

                        if (strParams.Italic)
                            item.FontStyle = FontStyle.Italic;

                        inlines.Add(item);
                    }
                    else
                    {
                        var navigateUri = new Uri(strParams.NavigateUri);
                        var hyperlink = new Hyperlink {NavigateUri = navigateUri};
                        hyperlink.Inlines.Add(new Run {Text = literalLexeme.Text, FontSize = _appSettings.FontSize});
                        inlines.Add(hyperlink);
                        inlines.Add(new Run {Text = " ", FontSize = _appSettings.FontSize});
                    }
                    continue;
                }

                var tagLexeme = (HtmlTagLexeme) lexeme;
                if (string.Equals(tagLexeme.Name, "br", StringComparison.OrdinalIgnoreCase))
                {
                    AddLineBreak(inlines);
                    continue;
                }

                if (string.Equals(tagLexeme.Name, "img", StringComparison.OrdinalIgnoreCase)
                    && tagLexeme.IsOpen
                    /*&& tagLexeme.IsClose*/)
                {
                    AddImage(inlines, tagLexeme, strParams.NavigateUri);
                }

                TryAddYoutubeVideo(tagLexeme, inlines);

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
            if (string.Equals(startL.Name, "em", StringComparison.OrdinalIgnoreCase))
            {
                strParams.Italic = false;
            }

            if (string.Equals(startL.Name, "i", StringComparison.OrdinalIgnoreCase))
            {
                strParams.Italic = false;
            }

            if (string.Equals(startL.Name, "p", StringComparison.OrdinalIgnoreCase))
            {
                AddLineBreak(inlines);
            }

            if (string.Equals(startL.Name, "li", StringComparison.OrdinalIgnoreCase))
            {
                AddLineBreak(inlines);
            }

            if (string.Equals(startL.Name, "div", StringComparison.OrdinalIgnoreCase))
            {
                AddLineBreak(inlines);
            }

            if (string.Equals(startL.Name, "a", StringComparison.OrdinalIgnoreCase))
            {
                strParams.NavigateUri = null;
            }

            if (string.Equals(startL.Name, "strong", StringComparison.OrdinalIgnoreCase))
            {
                strParams.Bold = false;
            }
        }

        private void AddLineBreak(InlineCollection inlines)
        {
            if (inlines.Count == 0 /* || inlines[inlines.Count - 1] is LineBreak*/)
                return;

            inlines.Add(new LineBreak());
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
            var startLexeme = (HtmlTagLexeme) lexemes[startLexemeIndex];
            var deep = 1;

            for (var index = startLexemeIndex + 1; index < lexemes.Length; index++)
            {
                var nextLexeme = lexemes[index] as HtmlTagLexeme;
                if (nextLexeme == null)
                    continue;

                if (!string.Equals(startLexeme.Name, nextLexeme.Name, StringComparison.OrdinalIgnoreCase))
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

        private void TryAddYoutubeVideo(HtmlTagLexeme tagLexeme, InlineCollection inlines)
        {
            if (string.Equals(tagLexeme.Name, "iframe", StringComparison.OrdinalIgnoreCase))
            {
                string videoLink;
                if (!tagLexeme.Attributes.TryGetValue("src", out videoLink))
                    return;

                AddYoutubeLink(videoLink, inlines);
                return;
            }

            if (string.Equals(tagLexeme.Name, "object", StringComparison.OrdinalIgnoreCase))
            {
                string videoLink;
                if (!tagLexeme.Attributes.TryGetValue("data", out videoLink))
                    return;

                AddYoutubeLink(videoLink, inlines);
                return;
            }

            if (string.Equals(tagLexeme.Name, "embed", StringComparison.OrdinalIgnoreCase))
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
            if (_youtubeAllVideos.Contains(videoLink))
                return;
            _youtubeAllVideos.Add(videoLink);

            foreach (var testLink in YoutubeLinks)
            {
                if (videoLink.StartsWith(testLink, StringComparison.OrdinalIgnoreCase))
                {
                    var id = videoLink.Substring(testLink.Length).Replace("/", string.Empty);
                    var questionIndex = id.IndexOf('?');
                    if (questionIndex != -1)
                        id = id.Substring(0, questionIndex);

                    AddYoutubeLink(id, videoLink, inlines);
                    return;
                }
            }
        }

        private void AddYoutubeLink(string id, string videoLink, InlineCollection inlines)
        {
            var imageUrl = string.Format(YoutubePreviewFormat, id);

            AddImage(inlines, imageUrl, videoLink);

            var navigateUri = new Uri(videoLink);
            var hyperlink = new Hyperlink {NavigateUri = navigateUri};
            hyperlink.Inlines.Add(new Run
            {
                Text = Resources.YoutubeVideoTitle,
                FontSize = _appSettings.FontSize,
                FontStyle = FontStyle.Italic
            });
            inlines.Add(hyperlink);
            AddLineBreak(inlines);
        }
    }
}