using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace RClaude.Telegram;

public partial class MessageFormatter
{
    private const int MaxMessageLength = 4000;

    public async Task SendResponseAsync(
        ITelegramBotClient bot, long chatId, string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            await bot.SendMessage(chatId, "(bo'sh javob)", cancellationToken: ct);
            return;
        }

        var htmlText = ConvertMarkdownToHtml(text);
        var chunks = SplitMessage(htmlText);

        foreach (var chunk in chunks)
        {
            try
            {
                await bot.SendMessage(chatId, chunk,
                    parseMode: ParseMode.Html, cancellationToken: ct);
            }
            catch
            {
                // HTML parse xato bo'lsa, plain text yuboramiz
                var plain = StripHtmlTags(chunk);
                if (plain.Length > 4096)
                    plain = plain[..4090] + "\n...";
                await bot.SendMessage(chatId, plain, cancellationToken: ct);
            }

            if (chunks.Count > 1)
                await Task.Delay(300, ct);
        }
    }

    public static string ConvertMarkdownToHtml(string markdown)
    {
        // Code blocks: ```lang\ncode\n``` → <pre><code>
        var result = CodeBlockRegex().Replace(markdown, m =>
        {
            var code = EscapeHtml(m.Groups[2].Value.TrimEnd());
            return $"<pre><code>{code}</code></pre>";
        });

        // Inline code: `code` → <code>code</code>
        result = InlineCodeRegex().Replace(result, m =>
            $"<code>{EscapeHtml(m.Groups[1].Value)}</code>");

        // Bold: **text** → <b>text</b>
        result = BoldRegex().Replace(result, "<b>$1</b>");

        // Italic: *text* (but not inside bold) → <i>text</i>
        result = ItalicRegex().Replace(result, "<i>$1</i>");

        return result;
    }

    private static List<string> SplitMessage(string text)
    {
        if (text.Length <= MaxMessageLength)
            return [text];

        var chunks = new List<string>();
        var remaining = text;

        while (remaining.Length > 0)
        {
            if (remaining.Length <= MaxMessageLength)
            {
                chunks.Add(remaining);
                break;
            }

            var splitAt = FindSplitPoint(remaining, MaxMessageLength);
            var chunk = remaining[..splitAt];
            chunk = BalanceHtmlTags(chunk);

            chunks.Add(chunk);
            remaining = remaining[splitAt..].TrimStart('\n', '\r');
        }

        return chunks;
    }

    private static int FindSplitPoint(string text, int maxLen)
    {
        // Try to split at double newline (paragraph break)
        var doubleNewline = text.LastIndexOf("\n\n", maxLen, StringComparison.Ordinal);
        if (doubleNewline > maxLen / 3)
            return doubleNewline;

        // Try to split at newline
        var lastNewline = text.LastIndexOf('\n', maxLen);
        if (lastNewline > maxLen / 3)
            return lastNewline;

        // Try to split at space
        var lastSpace = text.LastIndexOf(' ', maxLen);
        if (lastSpace > maxLen / 3)
            return lastSpace;

        return maxLen;
    }

    private static string BalanceHtmlTags(string html)
    {
        var sb = new StringBuilder(html);

        var openPre = PreOpenRegex().Matches(html).Count
                    - PreCloseRegex().Matches(html).Count;
        var openCode = CodeOpenRegex().Matches(html).Count
                     - CodeCloseRegex().Matches(html).Count;
        var openB = BOpenRegex().Matches(html).Count
                  - BCloseRegex().Matches(html).Count;

        for (int i = 0; i < openCode; i++) sb.Append("</code>");
        for (int i = 0; i < openPre; i++) sb.Append("</pre>");
        for (int i = 0; i < openB; i++) sb.Append("</b>");

        return sb.ToString();
    }

    private static string EscapeHtml(string text) =>
        text.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");

    public static string StripHtmlTags(string html) =>
        HtmlTagRegex().Replace(html, "")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&amp;", "&");

    [GeneratedRegex(@"```(\w*)\n([\s\S]*?)```")]
    private static partial Regex CodeBlockRegex();

    [GeneratedRegex(@"`([^`]+)`")]
    private static partial Regex InlineCodeRegex();

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex BoldRegex();

    [GeneratedRegex(@"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)")]
    private static partial Regex ItalicRegex();

    [GeneratedRegex(@"<pre>")]
    private static partial Regex PreOpenRegex();

    [GeneratedRegex(@"</pre>")]
    private static partial Regex PreCloseRegex();

    [GeneratedRegex(@"<code[^>]*>")]
    private static partial Regex CodeOpenRegex();

    [GeneratedRegex(@"</code>")]
    private static partial Regex CodeCloseRegex();

    [GeneratedRegex(@"<b>")]
    private static partial Regex BOpenRegex();

    [GeneratedRegex(@"</b>")]
    private static partial Regex BCloseRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex HtmlTagRegex();
}
