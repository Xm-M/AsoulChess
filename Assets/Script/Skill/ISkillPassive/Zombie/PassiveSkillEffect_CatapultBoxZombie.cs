using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 投小丑盒僵尸被动：投掷 <see cref="maxThrows"/> 次后，或本行已无可攻击目标时，
/// 关闭 <see cref="AttackController.AttackAble"/> 并停止攻击，由 <see cref="OutRangeTransition"/> 切入移动且不再回攻击。
/// </summary>
[Serializable]
public class PassiveSkillEffect_CatapultBoxZombie : ISkillEffect
{
    [Tooltip("最大投掷次数，达到后永久停止攻击")]
    public int maxThrows = 20;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.equipWeapon == null)
            return;

        int throwCount = 0;
        UnityAction<Chess> onAttack = null;
        UnityAction<Chess> onRemove = null;

        void Cleanup()
        {
            if (user?.equipWeapon != null && onAttack != null)
                user.equipWeapon.OnAttack.RemoveListener(onAttack);
            if (user != null && onRemove != null)
                user.OnRemove.RemoveListener(onRemove);
        }

        onRemove = _ => Cleanup();

        onAttack = _ =>
        {
            if (user == null || user.IfDeath)
                return;

            throwCount++;
            bool noTarget = user.equipWeapon.weapon == null
                || user.equipWeapon.weapon.FindEnemy(user) <= 0;
            if (throwCount >= maxThrows || noTarget)
                EndAttackPhase(user);
        };

        user.equipWeapon.OnAttack.AddListener(onAttack);
        user.OnRemove.AddListener(onRemove);
    }

    static void EndAttackPhase(Chess user)
    {
        if (user?.equipWeapon == null)
            return;
        user.equipWeapon.AttackAble = false;
        user.equipWeapon.StopAttack();
    }
}
