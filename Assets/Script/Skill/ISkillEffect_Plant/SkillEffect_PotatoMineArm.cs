using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 土豆地雷主动（冷却）：恢复普攻；<see cref="visualTierArmed"/> 写入 Animator VisualTier（通常为 1=起身/武装）；
/// <see cref="Chess.UnSelectable"/> 使铲子无法选中（与 PVZ 武装后不可铲一致）。
/// </summary>
[Serializable]
public class SkillEffect_PotatoMineArm : ISkillEffect
{
    [Tooltip("武装态 VisualTier，需与 Animator 中 Blend Tree 一致")]
    public float visualTierArmed = 1f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        if (user.equipWeapon != null)
            user.equipWeapon.AttackAble = true;
        if (user.animatorController != null)
            user.animatorController.SetVisualTierPublic(visualTierArmed);
        user.UnSelectable();
    }
}
