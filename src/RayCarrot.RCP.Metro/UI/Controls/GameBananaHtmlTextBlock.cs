using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using HtmlAgilityPack;

namespace RayCarrot.RCP.Metro;

public class GameBananaHtmlTextBlock : TextBlock
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private int _currentOrderedListIndex;

    public string? HtmlText
    {
        get => (string)GetValue(HtmlTextProperty);
        set => SetValue(HtmlTextProperty, value);
    }

    public static readonly DependencyProperty HtmlTextProperty = DependencyProperty.Register(
        name: nameof(HtmlText), 
        propertyType: typeof(string), 
        ownerType: typeof(GameBananaHtmlTextBlock), 
        typeMetadata: new FrameworkPropertyMetadata(OnHtmlTextChanged));

    private bool HasParentWithElementName(HtmlNode node, string name)
    {
        HtmlNode? parentNode = node;
        while ((parentNode = parentNode.ParentNode) != null)
        {
            if (parentNode.NodeType == HtmlNodeType.Element && parentNode.Name == name)
                return true;
        }

        return false;
    }

    private Hyperlink CreateHyperlink(Inline child, Uri uri)
    {
        return new Hyperlink(child)
        {
            Command = new AsyncRelayCommand(async () =>
            {
                // TODO-LOC
                if (await Services.MessageUI.DisplayMessageAsync($"You are about to open \"{uri}\".\nOnly continue if you trust the website.", "External link warning", MessageType.Question, true))
                    Services.App.OpenUrl(uri.AbsoluteUri);
            }),
            ToolTip = uri,
        };
    }

    // NOTE: This is not a perfect implementation of converting html to textblock inlines, but it works well enough
    private void ProcessNode(HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            // Decode the text in order to convert html character entities to normal text
            string text = WebUtility.HtmlDecode(node.InnerText);

            // Format spaces and linebreaks if not in a preformatted text element
            if (!HasParentWithElementName(node, "pre"))
            {
                // Trim whitespace
                if (Inlines.LastInline is LineBreak)
                    text = text.TrimStart(' ');

                // Replace first linebreak instance with a space, assuming previous text didn't end with a space or linebreak
                if (!(Inlines.LastInline is Run run && run.Text.EndsWith(" ")) && Inlines.LastInline is not LineBreak)
                {
                    int newLineIndex = text.IndexOf('\n');
                    if (newLineIndex != -1)
                        text = text.Substring(0, newLineIndex) + " " + text.Substring(newLineIndex + 1);
                }

                // Remove linebreaks
                text = text.Replace(['\r', '\n'], String.Empty);
            }

            // Ignore if empty
            if (text == String.Empty)
                return;

            // Create an inline run element
            Run runInline = new(text);
            Inline inline = runInline;

            // Process parent nodes
            bool separateLine = false;
            bool hasSetForeground = false;
            HtmlNode? parentNode = node;
            while ((parentNode = parentNode.ParentNode) != null)
            {
                if (parentNode.NodeType == HtmlNodeType.Element)
                {
                    switch (parentNode.Name)
                    {
                        // Bold
                        case "b":
                            inline.FontWeight = FontWeights.Bold;
                            break;

                        // Italic
                        case "i":
                            inline.FontStyle = FontStyles.Italic;
                            break;

                        // Underline
                        case "u":
                            inline.TextDecorations.Add(System.Windows.TextDecorations.Underline);
                            break;

                        // Span with custom class
                        case "span":
                            foreach (HtmlAttribute attr in parentNode.Attributes)
                            {
                                if (attr.Name == "class")
                                {
                                    foreach (string @class in attr.Value.Split(' '))
                                    {
                                        switch (@class)
                                        {
                                            case "GreenColor":
                                                if (!hasSetForeground)
                                                {
                                                    inline.Foreground = new SolidColorBrush(Color.FromRgb(0x4c, 0xaf, 0x50)); // GreenPrimary500
                                                    hasSetForeground = true;
                                                }
                                                break;

                                            case "RedColor":
                                                if (!hasSetForeground)
                                                {
                                                    inline.Foreground = new SolidColorBrush(Color.FromRgb(0xf4, 0x43, 0x36)); // RedPrimary500
                                                    hasSetForeground = true;
                                                }
                                                break;

                                            case "Spoiler":
                                                // NOTE: Not implemented
                                                break;
                                        }
                                    }
                                }
                            }
                            break;

                        // Deleted (strikethrough)
                        case "del":
                            inline.TextDecorations.Add(System.Windows.TextDecorations.Strikethrough);
                            break;

                        // Inline code
                        case "code":
                            inline.FontFamily = new FontFamily("consolas");
                            break;

                        // Heading 1
                        case "h1":
                            separateLine = true;
                            inline.FontSize = 22;
                            break;

                        // Heading 2
                        case "h2":
                            separateLine = true;
                            inline.FontSize = 18;
                            break;

                        // Heading 3
                        case "h3":
                            separateLine = true;
                            inline.FontSize = 14;
                            break;

                        // Hyperlink (anchor)
                        case "a":
                            foreach (HtmlAttribute attr in parentNode.Attributes)
                            {
                                if (attr.Name == "href")
                                {
                                    if (inline is not Hyperlink && Uri.TryCreate(attr.Value, UriKind.Absolute, out Uri uri))
                                        inline = CreateHyperlink(inline, uri);
                                
                                    break;
                                }
                            }
                            break;
                    }
                }
            }

            if (separateLine && Inlines.LastInline is not LineBreak)
                Inlines.Add(new LineBreak());

            Inlines.Add(inline);

            if (separateLine)
                Inlines.Add(new LineBreak());
        }
        else if (node.NodeType is HtmlNodeType.Document or HtmlNodeType.Element)
        {
            foreach (HtmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    // Linebreak
                    case "br":
                        Inlines.Add(new LineBreak());
                        break;

                    // Ordered list
                    case "ol":
                        _currentOrderedListIndex = 0;
                        break;

                    // List item
                    case "li":
                        if (Inlines.LastInline is not LineBreak)
                            Inlines.Add(new LineBreak());

                        // Ordered list
                        if (HasParentWithElementName(childNode, "ol"))
                        {
                            Inlines.Add($"    {_currentOrderedListIndex + 1}. ");
                            _currentOrderedListIndex++;
                        }
                        // Unordered list
                        else if (HasParentWithElementName(childNode, "ul"))
                        {
                            Inlines.Add("    • ");
                        }
                        break;

                    // Separator (horizontal rule)
                    case "hr":
                        if (Inlines.LastInline is not LineBreak)
                            Inlines.Add(new LineBreak());

                        Inlines.Add(new InlineUIContainer(new Separator()
                        {
                            Width = 200
                        }));

                        Inlines.Add(new LineBreak());
                        break;

                    // Image
                    case "img":
                        foreach (HtmlAttribute attr in childNode.Attributes)
                        {
                            if (attr.Name == "src")
                            {
                                if (Uri.TryCreate(attr.Value, UriKind.Absolute, out Uri uri))
                                    Inlines.Add(CreateHyperlink(new Run("Image"), uri));

                                break;
                            }
                        }
                        break;

                    // YouTube
                    case "iframe":
                        foreach (HtmlAttribute attr in childNode.Attributes)
                        {
                            if (attr.Name == "src")
                            {
                                if (Uri.TryCreate(attr.Value.Replace("/embed/", "/watch/"), UriKind.Absolute, out Uri uri))
                                    Inlines.Add(CreateHyperlink(new Run("YouTube"), uri));

                                break;
                            }
                        }
                        break;
                }

                ProcessNode(childNode);
            }
        }
    }

    private static void OnHtmlTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not GameBananaHtmlTextBlock textBlock)
            return;

        if (textBlock.HtmlText == null)
        {
            textBlock.Inlines.Clear();
            return;
        }

        try
        {
            HtmlDocument doc = new();
            doc.LoadHtml(textBlock.HtmlText);

            textBlock.ProcessNode(doc.DocumentNode);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Parsing html text");

            textBlock.Inlines.Clear();
            textBlock.Text = textBlock.HtmlText;
        }
    }
}