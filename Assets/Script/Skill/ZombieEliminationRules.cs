using System.Collections.Generic;

/// <summary>
/// 「清除 / 消灭僵尸」类效果的公共目标过滤（章鱼噼等）。
/// Boss 判定：<see cref="PropertyCreator.plantTags"/> 含 <see cref="BossPlantTag"/>。
/// </summary>
public static class ZombieEliminationRules
{
    public const string BossPlantTag = "Boss";

    public static bool IsBoss(PropertyCreator creator)
    {
        if (creator?.plantTags == null)
            return false;
        for (int i = 0; i < creator.plantTags.Count; i++)
        {
            if (creator.plantTags[i] == BossPlantTag)
                return true;
        }
        return false;
    }

    public static bool IsEligibleEliminationTarget(Chess chess)
    {
        if (chess == null || chess.IfDeath)
            return false;
        if (!chess.CompareTag("Enemy"))
            return false;
        return !IsBoss(chess.propertyController?.creator);
    }
}
