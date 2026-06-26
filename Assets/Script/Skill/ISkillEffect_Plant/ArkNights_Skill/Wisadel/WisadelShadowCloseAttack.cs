using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>魂灵之影近战法术：伤害 = 主人 ATK × <see cref="magicRatio"/>，伤害来源记为主人。</summary>
[Serializable]
public class WisadelShadowCloseAttack : IAttackFunction
{
    public float magicRatio = 0.5f;
    public DamageMessege DM = new DamageMessege
    {
        damageType = DamageType.Magic,
        damageElementType = ElementType.AOE,
    };

    public void Attack(Chess user, List<Chess> targets)
    {
        if (user?.skillController?.context == null || targets == null || targets.Count == 0)
            return;
        if (!user.skillController.context.TryGet(WisadelKeys.Master, out Chess master)
            || master == null
            || master.IfDeath)
            return;

        float damage = WisadelDamageHelper.GetAttack(master) * magicRatio;
        Buff stun = DM?.takeBuff;

        for (int i = 0; i < targets.Count; i++)
        {
            Chess target = targets[i];
            if (target == null || target.IfDeath)
                continue;

            var mes = new DamageMessege(
                master,
                target,
                damage,
                DM != null ? DM.damageType : DamageType.Magic,
                DM != null ? DM.damageElementType : ElementType.AOE);
            if (stun != null)
                mes.takeBuff = stun;
            master.propertyController.TakeDamage(mes);
        }
    }
}
