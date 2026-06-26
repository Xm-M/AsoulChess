using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>维什戴尔三技能「爆裂黎明」：召影 + 仅换爆裂弹 + 动画/Buff（6 发 Bullet 命中后结束）。</summary>
public class SkillEffect_WisadelBurst : ISkillEffect
{
    [LabelText("魂灵之影 Creator（可空，后续配置）")]
    public PropertyCreator shadowCreator;

    [LabelText("技能召唤影数量")]
    public int burstSummonCount = 2;

    [FoldoutGroup("爆裂武器")]
    public GameObject burstBullet;

    [FoldoutGroup("爆裂武器"), SerializeField]
    Buff_WisadelBurst burstBuff;

    [FoldoutGroup("爆裂武器")]
    public int burstAmmo = 6;

    [LabelText("召唤特效")]
    public GameObject summonEffect;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        WisadelShadowPlacer.TrySummon(user, shadowCreator, burstSummonCount, summonEffect);

        WisadelBurstMode.EnterBurst(user, new WisadelBurstMode.BurstConfig
        {
            burstBullet = burstBullet,
            burstBuff = burstBuff,
            burstAmmo = burstAmmo
        });
    }
}
