/// <summary>
/// <see cref="EventName.SnakeEatFood"/> 字符串载荷：去重键 + 分隔符 + UI 展示文案。
/// </summary>
public static class SnakeEatFoodPayload
{
    public const char Separator = '\u001E';

    public static string Build(string chessName, string displayText)
    {
        if (chessName == null) chessName = string.Empty;
        if (displayText == null) displayText = string.Empty;
        return chessName + Separator + displayText;
    }

    /// <param name="dedupeKey">用于「本局每种食物只提示一次」；无棋子名时退回为展示文案。</param>
    /// <param name="displayText">提示正文（棋子描述等）。</param>
    public static void Split(string payload, out string dedupeKey, out string displayText)
    {
        if (string.IsNullOrEmpty(payload))
        {
            dedupeKey = string.Empty;
            displayText = string.Empty;
            return;
        }

        int i = payload.IndexOf(Separator);
        if (i < 0)
        {
            displayText = payload.Trim();
            dedupeKey = displayText;
            return;
        }

        dedupeKey = payload.Substring(0, i).Trim();
        displayText = payload.Substring(i + 1).Trim();
        if (string.IsNullOrEmpty(dedupeKey))
            dedupeKey = displayText;
    }
}
