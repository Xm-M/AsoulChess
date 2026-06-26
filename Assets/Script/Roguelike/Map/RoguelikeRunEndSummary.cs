/// <summary>肉鸽 Run 结束结算快照（在 State 清档前采集）。</summary>
public class RoguelikeRunEndSummary
{
    public RoguelikeRunEndKind kind;
    public int actReached;
    public int layerReached;
    public int runGold;
    public int plantCount;

    public string FormatSummaryText()
    {
        return $"到达 Act {actReached}\n最深层 {layerReached}\n剩余金币 {runGold}\n植物数量 {plantCount}";
    }
}

public enum RoguelikeRunEndKind
{
    Victory,
    Defeat,
}
