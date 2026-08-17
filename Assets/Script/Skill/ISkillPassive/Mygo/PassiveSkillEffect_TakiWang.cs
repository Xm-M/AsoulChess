using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 立希汪被动：每次造成伤害时对目标叠加「压力」+<see cref="stressPerAttack"/>（默认 1）。
/// 近战由 Prefab 上 Weapon_Sample + CloseAttack + IGridFindTarget 配置。
/// </summary>
[Serializable]
public class PassiveSkillEffect_TakiWang : ISkillEffect
{
    [SerializeReference]
    [Tooltip("目标尚无「压力」时挂上的模板")]
    public Buff_StressBuff_Death guestStressBuff;

    [Min(1)]
    [Tooltip("每次命中通过 BuffReset 增加的压力")]
    public int stressPerAttack = 1;

    Chess _user;
    UnityAction<DamageMessege> _onTakeDamage;
    UnityAction<Chess> _onRemove;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null)
            return;

        Cleanup();
        _user = user;

        _onTakeDamage = OnTakeDamage;
        if (user.propertyController != null)
            user.propertyController.onTakeDamage.AddListener(_onTakeDamage);

        _onRemove = OnRemove;
        user.OnRemove.AddListener(_onRemove);
    }

    void OnTakeDamage(DamageMessege dm)
    {
        if (_user == null || _user.IfDeath || dm == null)
            return;
        if (dm.damageFrom != _user)
            return;
        if (dm.damageType == DamageType.Heal || dm.damageType == DamageType.Miss)
            return;

        Chess victim = dm.damageTo;
        if (victim == null || victim.IfDeath)
            return;
        if (victim.CompareTag(_user.tag))
            return;

        OkuwakiStressSpread.ApplyStressDelta(
            victim,
            Mathf.Max(1, stressPerAttack),
            guestStressBuff);
    }

    void OnRemove(Chess chess)
    {
        Cleanup();
    }

    void Cleanup()
    {
        if (_user != null)
        {
            if (_onTakeDamage != null && _user.propertyController != null)
                _user.propertyController.onTakeDamage.RemoveListener(_onTakeDamage);
            if (_onRemove != null)
                _user.OnRemove.RemoveListener(_onRemove);
        }

        _onTakeDamage = null;
        _onRemove = null;
        _user = null;
    }
}
