using System;
using System.Collections.Generic;

/// <summary>肉鸽战斗奖励面板中的单条奖励（点击领取后消失）。</summary>
[Serializable]
public class RoguelikeRewardEntry
{
    public RoguelikeRewardEntryKind kind;
    public int goldAmount;
    public bool claimed;

    /// <summary>PlantPick 生成条目时掷好的候选（creator.chessName）。</summary>
    public List<string> plantPickOptions = new List<string>();

    public static RoguelikeRewardEntry CreateGold(int amount) =>
        new RoguelikeRewardEntry { kind = RoguelikeRewardEntryKind.Gold, goldAmount = amount };

    public static RoguelikeRewardEntry CreatePlantPick(List<string> options) =>
        new RoguelikeRewardEntry
        {
            kind = RoguelikeRewardEntryKind.PlantPick,
            plantPickOptions = options != null ? new List<string>(options) : new List<string>(),
        };
}

public enum RoguelikeRewardEntryKind
{
    Gold,
    PlantPick,
    Item,
}
