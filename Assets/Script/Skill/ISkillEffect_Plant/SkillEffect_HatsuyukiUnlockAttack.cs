using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 初雪主动：在 <see cref="ISkill.UseSkill"/> 时把 <see cref="AttackController.AttackAble"/> 设为 true（一次性开关，之后可正常普攻，不会在技能结束时关回）。
/// 挂在主动技能的 <c>effect</c> 上（若需与其它效果并存，请用复合 ISkillEffect）。
/// </summary>
public class SkillEffect_HatsuyukiUnlockAttack : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.equipWeapon == null) return;
        user.equipWeapon.AttackAble = true;
    }
}
