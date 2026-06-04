using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 秋山澪溢出转阳：当 <see cref="ContextKeyOverflowHealBuffer"/> ≥ <see cref="overflowThreshold"/> 时产阳并扣除一档溢出量。
/// 需在溢出模式下由 <see cref="MultySkill_Mio"/> + 动画 <c>UseSkill</c> 事件触发。
/// </summary>
public class SkillEffect_OverflowHeal : ISkillEffect
{
    public const string ContextKeyOverflowHealBuffer = "akiOverflowHealBuffer";

    [Tooltip("一档溢出治疗量达到该值时，本次技能结算产阳")]
    [MinValue(0.01f)]
    public float overflowThreshold = 50f;

    [Tooltip("每结算一档产生的阳光数；≤0 时用 SkillConfig.baseDamage[0]")]
    [MinValue(0)]
    public int sunPerPayout = 25;

    public Transform sunLightPos;

    [Tooltip("前方一格友方判定：与 PropertyCreator.chessName 或 fetterMemberId 一致即视为田井中律")]
    public string tainakaRitsuIdentity = "田井中律";

    [Tooltip("对律造成的联动伤害（真实伤害，通常 1）；≤0 不触发")]
    [MinValue(0f)]
    public float pingRitsuDamage = 1f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        TryDealPingDamageToRitsuInFront(user);

        float buffer = 0f;
        user.skillController.context.TryGet(ContextKeyOverflowHealBuffer, out buffer);
        if (buffer < overflowThreshold)
            return;

        int sun = sunPerPayout;
        if (sun <= 0 && config != null && config.baseDamage != null && config.baseDamage.Count > 0)
            sun = (int)config.baseDamage[0];
        if (sun <= 0)
            return;

        user.skillController.context.Set(ContextKeyOverflowHealBuffer, buffer - overflowThreshold);

        SunLight light = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
        Vector3 pos = sunLightPos != null ? sunLightPos.position : user.transform.position;
        light.InitSunLight(user.moveController.standTile, sun, pos);
    }

    static bool IsTainakaRitsu(Chess c, string identity)
    {
        if (c == null || string.IsNullOrEmpty(identity)) return false;
        var creator = c.propertyController?.creator;
        if (creator == null) return false;
        if (creator.chessName == identity) return true;
        return !string.IsNullOrEmpty(creator.fetterMemberId) && creator.fetterMemberId == identity;
    }

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
