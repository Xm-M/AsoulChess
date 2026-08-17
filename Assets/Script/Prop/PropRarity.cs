/// <summary>道具稀有度四档（与植物 <c>Property.rarity</c> 刷怪权重无关）。</summary>
public enum PropRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Legendary = 3,
}

public static class PropRarityUtil
{
    public static string GetDisplayName(PropRarity rarity)
    {
        switch (rarity)
        {
            case PropRarity.Common: return "普通";
            case PropRarity.Uncommon: return "稀有";
            case PropRarity.Rare: return "史诗";
            case PropRarity.Legendary: return "传说";
            default: return "普通";
        }
    }
}
