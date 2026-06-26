using System.Collections.Generic;
using UnityEngine;

/// <summary>魂灵之影被动：武器命中后挂残影标记（伤害/眩晕由武器配置）。</summary>
public class PassiveSkillEffect_WisadelShadow : ISkillEffect
{
    Chess shadow;
    Chess master;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        shadow = user;
        if (!user.skillController.context.TryGet(WisadelKeys.Master, out master) || master == null)
            return;

        if (user.equipWeapon != null)
            user.equipWeapon.OnAttack.AddListener(OnShadowAttack);
        user.OnRemove.AddListener(OnShadowRemove);
    }

    void OnShadowAttack(Chess attacker)
    {
        if (shadow == null || shadow.IfDeath || master == null || master.IfDeath)
            return;

        var weapon = shadow.equipWeapon?.weapon as Weapon_Sample;
        if (weapon?.enemys == null)
            return;

        for (int i = 0; i < weapon.enemys.Count; i++)
        {
            Chess target = weapon.enemys[i];
            if (target != null && !target.IfDeath)
                Buff_WisadelMark.ApplyOrRefresh(target, master);
        }
    }

    void OnShadowRemove(Chess c)
    {
        if (shadow?.equipWeapon != null)
            shadow.equipWeapon.OnAttack.RemoveListener(OnShadowAttack);
    }
}
