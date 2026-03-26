using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 秋山澪主动（或产阳光帧）：基础阳光来自 <see cref="SkillConfig.baseDamage"/>[0]，
/// 额外阳光 = 缓存溢出治疗量 × <see cref="overflowHealToSunRatio"/> × (1 + <see cref="PropertyController.GetLifeStealing"/> )，然后清空缓存。
/// 联动：释放时若面朝方向前一格有友方「田井中律」，对其造成 1 点真实伤害（供受击充能等）。
/// </summary>
public class SkillEffect_AkiyamaMioSun : ISkillEffect
{
    public const string ContextKeyOverflowHealBuffer = "akiOverflowHealBuffer";

    [Tooltip("溢出治疗量 → 阳光换算系数（再乘 1+生命偷取）")]
    [Range(0f, 2f)]
    public float overflowHealToSunRatio = 0.15f;
    public Transform sunLightPos;

    [Tooltip("前方一格友方判定：与 PropertyCreator.chessName 或 fetterMemberId 一致即视为田井中律")]
    public string tainakaRitsuIdentity = "田井中律";

    [Tooltip("对律造成的联动伤害（真实伤害，通常 1）")]
    [MinValue(0f)]
    public float pingRitsuDamage = 1f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        TryDealPingDamageToRitsuInFront(user);

        int baseSun = 0;
        if (config != null && config.baseDamage != null && config.baseDamage.Count > 0)
            baseSun = (int)config.baseDamage[0];

        float buffer = 0f;
        user.skillController.context.TryGet(ContextKeyOverflowHealBuffer, out buffer);
        float ls = user.propertyController.GetLifeStealing();
        int bonusSun = Mathf.RoundToInt(buffer * overflowHealToSunRatio * (1f + ls));
        user.skillController.context.Set(ContextKeyOverflowHealBuffer, 0f);
        
        int total = Mathf.Max(0, baseSun + bonusSun);
        if (total <= 0) return;

        SunLight light = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
        Vector3 pos = sunLightPos != null ? sunLightPos.position : user.transform.position;
        light.InitSunLight(user.moveController.standTile, total, pos);
    }

    static bool IsTainakaRitsu(Chess c, string identity)
    {
        if (c == null || string.IsNullOrEmpty(identity)) return false;
        var creator = c.propertyController?.creator;
        if (creator == null) return false;
        if (creator.chessName == identity) return true;
        return !string.IsNullOrEmpty(creator.fetterMemberId) && creator.fetterMemberId == identity;
    }

    /// <summary>面朝方向相邻格（PVZ：transform.right 的 X 符号决定 +1 / -1 列）。</summary>
    void TryDealPingDamageToRitsuInFront(Chess mio)
    {
        if (mio == null || pingRitsuDamage <= 0f) return;
        var stand = mio.moveController?.standTile;
        var map = MapManage.instance;
        if (stand == null || map == null) return;

        int dx = mio.transform.right.x >= -0.01f ? 1 : -1;
        int fx = stand.mapPos.x + dx;
        int fy = stand.mapPos.y;
        if (!map.IfInMapRange(fx, fy)) return;

        var tile = map.tiles[fx, fy];
        var ally = tile?.stander;
        if (ally == null || ally.IfDeath || !ally.CompareTag(mio.tag)) return;
        if (!IsTainakaRitsu(ally, tainakaRitsuIdentity)) return;

        var dm = new DamageMessege(mio, ally, pingRitsuDamage, DamageType.Real, ElementType.CloseAttack);
        ally.propertyController.GetDamage(dm);
    }
}
