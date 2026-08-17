using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 章鱼祥被动：对敌方造成伤害后，有概率施加 <see cref="Buff_Blind"/>（默认 25%、2 秒，可刷新）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_ZhangyuXiang : ISkillEffect
{
    [SerializeReference]
    [Tooltip("致盲模板；为空则运行时 new Buff_Blind()")]
    public Buff_Blind blindBuff;

    [Range(0f, 1f)]
    [Tooltip("命中后施加致盲的概率")]
    public float blindChance = 0.25f;

    [Min(0.01f)]
    [Tooltip("致盲持续时间（秒）")]
    public float blindDuration = 2f;

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
        if (dm.damageType == DamageType.Heal)
            return;

        Chess victim = dm.damageTo;
        if (victim == null || victim.IfDeath)
            return;
        if (victim.CompareTag(_user.tag))
            return;
        if (UnityEngine.Random.value >= Mathf.Clamp01(blindChance))
            return;

        Buff_Blind template = blindBuff != null ? blindBuff : new Buff_Blind();
        Buff_Blind apply = (Buff_Blind)template.Clone();
        apply.buffName = Buff_Blind.BuffKey;
        apply.continueTime = Mathf.Max(0.01f, blindDuration);
        victim.buffController.AddBuff(apply);
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
