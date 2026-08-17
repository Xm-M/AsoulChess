using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 粉色奶龙被动：常态关闭普攻（动物形态，对齐天素罗/芭菲猫）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_PinkNailong : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.equipWeapon != null)
            user.equipWeapon.AttackAble = false;
    }
}
