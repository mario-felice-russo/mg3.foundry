using System.Text.RegularExpressions;

namespace mg3.foundry.Features.Chat.Services
{
    public static class MarkdownParser
    {
        public static string ParseToHtml(string markdownText)
        {
            if (string.IsNullOrWhiteSpace(markdownText))
                return markdownText;

            var html = markdownText;

            // Bold: **text** or __text__
            html = Regex.Replace(html, @"\*\*(.*?)\*\*", "<strong>$1</strong>");
            html = Regex.Replace(html, @"__(.*?)__", "<strong>$1</strong>");

            // Italic: *text* or _text_
            html = Regex.Replace(html, @"\*(.*?)\*", "<em>$1</em>");
            html = Regex.Replace(html, @"_(.*?)_", "<em>$1</em>");

            // Strikethrough: ~~text~~
            html = Regex.Replace(html, @"~~(.*?)~~", "<s>$1</s>");

            // Headers: #, ##, ###
            html = Regex.Replace(html, @"^#\s+(.*?)$", "<h1>$1</h1>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^##\s+(.*?)$", "<h2>$1</h2>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^###\s+(.*?)$", "<h3>$1</h3>", RegexOptions.Multiline);

            // Links: [text](url)
            html = Regex.Replace(html, @"\[(.*?)\]\((.*?)\)", "<a href='$2'>$1</a>");

            // Images: ![alt](url)
            html = Regex.Replace(html, @"!\[(.*?)\]\((.*?)\)", "<img src='$2' alt='$1' />");

            // Code blocks: ```code``` or `code`
            html = Regex.Replace(html, @"```(.*?)```", "<pre><code>$1</code></pre>", RegexOptions.Singleline);
            html = Regex.Replace(html, @"`(.*?)`", "<code>$1</code>");

            // Lists: - item or * item
            html = Regex.Replace(html, @"^(-|\*|\+)\s+(.*?)$", "<li>$2</li>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"(?<=\n)(<li>.*?</li>)(?=\n|$)", "<ul>$1</ul>", RegexOptions.Singleline);

            // Paragraphs: separate lines
            html = Regex.Replace(html, @"\n\n", "</p><p>");
            html = Regex.Replace(html, @"^(.*)$", "<p>$1</p>", RegexOptions.Multiline);

            // Remove empty paragraphs
            html = Regex.Replace(html, @"<p>\s*</p>", "");

            return html;
        }

        public static string ParseToFormattedString(string markdownText)
        {
            if (string.IsNullOrWhiteSpace(markdownText))
                return markdownText;

            var result = markdownText;

            // Bold
            result = Regex.Replace(result, @"\*\*(.*?)\*\*", "**$1**");
            result = Regex.Replace(result, @"__(.*?)__", "**$1**");

            // Italic
            result = Regex.Replace(result, @"\*(.*?)\*", "*$1*");
            result = Regex.Replace(result, @"_(.*?)_", "*$1*");

            // Strikethrough
            result = Regex.Replace(result, @"~~(.*?)~~", "~~$1~~");

            // Headers
            result = Regex.Replace(result, @"^#\s+(.*?)$", "📌 $1", RegexOptions.Multiline);
            result = Regex.Replace(result, @"^##\s+(.*?)$", "📍 $1", RegexOptions.Multiline);
            result = Regex.Replace(result, @"^###\s+(.*?)$", "📎 $1", RegexOptions.Multiline);

            // Code blocks
            result = Regex.Replace(result, @"```(.*?)```", "\n💻 Code:\n$1\n", RegexOptions.Singleline);
            result = Regex.Replace(result, @"`(.*?)`", "`$1`");

            // Links
            result = Regex.Replace(result, @"\[(.*?)\]\((.*?)\)", "🔗 $1: $2");

            return result;
        }
    }
}