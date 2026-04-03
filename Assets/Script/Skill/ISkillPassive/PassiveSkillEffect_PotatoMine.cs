using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 土豆地雷被动：进场后 <see cref="AttackController.AttackAble"/> 为 false（埋地状态不能普攻）。
/// 主动技能挂 <see cref="SkillEffect_PotatoMineArm"/>：起身后可攻击并切换外观、无法被铲子选中。
/// </summary>
[Serializable]
public class PassiveSkillEffect_PotatoMine : ISkillEffect
{
    [Tooltip("进场时写入 Animator VisualTier（通常为 0=埋地）；若预制体默认已是 0 可保持 0")]
    public float visualTierBuried = 0f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        if (user.equipWeapon != null)
            user.equipWeapon.AttackAble = false;
        if (user.animatorController != null)
            user.animatorController.SetVisualTierPublic(visualTierBuried);
    }
}
