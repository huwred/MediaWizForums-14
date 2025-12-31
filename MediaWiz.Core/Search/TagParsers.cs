using HtmlAgilityPack;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class MessageParser   
{
    private static readonly Regex TagRegex = new Regex("<.*?>",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex SpaceRegex = new Regex(@"\s+",
        RegexOptions.Compiled);
    private static readonly Regex MarkupStartRegex = new Regex(@"({""markup"":"")",
        RegexOptions.Compiled);
    private static readonly Regex MarkupEndRegex = new Regex(@"("",""blocks"":.*)",
        RegexOptions.Compiled);
    private static readonly Regex DecodeEncodedNonAsciiCharacters = new Regex(@"\\u(?<Value>[a-zA-Z0-9]{4})",
        RegexOptions.Compiled);
    public static string StripMarkup(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;
        var text = MarkupStartRegex.Replace(html, "");
        text = MarkupEndRegex.Replace(text, "");        
        text = DecodeEncodedNonAsciiCharacters.Replace(text, m => {
            return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
        });

        return text;
    }
    public static string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Remove tags
        var text = TagRegex.Replace(html, " ");

        // Decode HTML entities (&nbsp;, &amp;, etc.)
        text = System.Net.WebUtility.HtmlDecode(text);


        // Normalize whitespace
        text = SpaceRegex.Replace(text, " ").Trim();

        return text;
    }


    public static string ExtractMerged(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var doc = new HtmlDocument
        {
            OptionFixNestedTags = true
        };
        doc.LoadHtml(html);

        var sb = new StringBuilder();
        AppendNode(doc.DocumentNode, sb);

        // Decode HTML entities (&nbsp;, &amp;, etc.)
        var text = System.Net.WebUtility.HtmlDecode(sb.ToString());

        // Normalize whitespace
        text = Regex.Replace(text, @"\s+", " ").Trim();

        return text;
    }

    private static void AppendNode(HtmlNode node, StringBuilder sb)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = node.InnerText;
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.Append(text);
                sb.Append(' ');
            }
            return;
        }

        // Preserve anchor tags: text + URL
        if (node.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
        {
            var text = node.InnerText.Trim();
            var href = node.GetAttributeValue("href", null);

            if (!string.IsNullOrWhiteSpace(text))
                sb.Append(text);

            if (!string.IsNullOrWhiteSpace(href))
                sb.Append($" ({href}) ");

            return;
        }

        // Add spacing before block elements
        if (IsBlock(node.Name))
            sb.AppendLine();

        foreach (var child in node.ChildNodes)
            AppendNode(child, sb);

        // Add spacing after block elements
        if (IsBlock(node.Name))
            sb.AppendLine();
    }

    private static bool IsBlock(string name)
    {
        switch (name.ToLowerInvariant())
        {
            case "p":
            case "div":
            case "section":
            case "article":
            case "header":
            case "footer":
            case "li":
            case "ul":
            case "ol":
            case "br":
            case "h1":
            case "h2":
            case "h3":
            case "h4":
            case "h5":
            case "h6":
                return true;
            default:
                return false;
        }
    }
}
