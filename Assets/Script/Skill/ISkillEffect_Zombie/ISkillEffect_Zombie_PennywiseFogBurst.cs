using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 潘妮怀斯僵尸主动：攻击范围内植物全部消灭；以自身为中心 3×3 驱散雾气；
/// 3×3 内存活植物恐惧；自身恢复 10% 最大生命 + 每名被恐惧者额外 10% 最大生命。
/// 触发条件由 <see cref="SkillReady_Multy"/>（雾中 + 范围内有植物）与 <see cref="StraightLaser"/> 配合。
/// </summary>
public class ISkillEffect_Zombie_PennywiseFogBurst : ISkillEffect
{
    [SerializeReference]
    [LabelText("恐惧 Buff 模板")]
    public Buff_Fear fearBuff;

    [LabelText("恐惧时长（秒）")]
    public float fearDuration = 10f;

    [LabelText("单次回复最大生命比例")]
    public float healPercentOfMaxHp = 0.1f;

    [LabelText("驱散雾气区域边长")]
    public int fogClearSize = 3;

    [LabelText("雾气隐藏时长")]
    public float fogHideTime = 99999f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || user.IfDeath)
            return;

        KillTargetsInRange(targets);
        ClearFogAround(user);
        int fearedCount = FearPlantsInArea(user);
        HealSelf(user, fearedCount);
    }

    static void KillTargetsInRange(List<Chess> targets)
    {
        if (targets == null) return;
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (t != null && !t.IfDeath)
                t.Death();
        }
    }

    void ClearFogAround(Chess user)
    {
        var tile = user.moveController?.standTile;
        if (tile == null || Effect_Smoke.Instance == null)
            return;
        Effect_Smoke.Instance.HideSmokeInRange(tile.mapPos, fogClearSize, fogHideTime);
    }

    int FearPlantsInArea(Chess user)
    {
        if (fearBuff == null || user.moveController?.standTile == null || MapManage.instance == null)
            return 0;

        int feared = 0;
        int half = (fogClearSize - 1) / 2;
        var center = user.moveController.standTile.mapPos;
        int minX = Mathf.Max(0, center.x - half);
        int maxX = Mathf.Min(MapManage.instance.mapSize.x - 1, center.x + half);
        int minY = Mathf.Max(0, center.y - half);
        int maxY = Mathf.Min(MapManage.instance.mapSize.y - 1, center.y + half);

        for (int x = minX; x <= maxX; x++)
        for (int y = minY; y <= maxY; y++)
        {
            var tile = MapManage.instance.tiles[x, y];
            var plant = tile?.stander;
            if (plant == null || plant.IfDeath || !plant.CompareTag("Player"))
                continue;
            if (plant.buffController == null)
                continue;

            var buff = fearBuff.Clone() as Buff_Fear;
            if (buff == null)
                continue;
            buff.continueTime = fearDuration;
            plant.buffController.AddBuff(buff);
            feared++;
        }

        return feared;
    }

    void HealSelf(Chess user, int fearedCount)
    {
        if (user.propertyController == null || healPercentOfMaxHp <= 0f)
            return;
        int stacks = 1 + Mathf.Max(0, fearedCount);
        float heal = user.propertyController.GetMaxHp() * healPercentOfMaxHp * stacks;
        if (heal > 0f)
            user.propertyController.Heal(heal);
    }
}
