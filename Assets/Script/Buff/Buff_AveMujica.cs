using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AveMujica 成员羁绊 Buff：按角色名分支上报 fever；全员绑定技能释放以在 fever 满时由 <see cref="AveMujica.OnAveMujicaMemberUsedSkill"/> 触发 Fever。
/// </summary>
[Serializable]
public class Buff_AveMujica : Buff
{
    const string NameSaki = "丰川祥子";
    const string NameUika = "三角初华";
    const string NameMutsumi = "若叶睦";
    const string NameNyamu = "祐天寺若麦";

    Timer _nyamuTimer;
    UnityAction<DamageMessege> _onTakeDamage;
    UnityAction<DamageMessege> _onHealDamage;
    UnityAction<DamageMessege> _onSetDamage;
    UnityAction<Chess> _onUseSkillFever;

    [System.NonSerialized]
    FeverFXPresenter _feverFx;

    public Buff_AveMujica()
    {
        buffName = "Buff_AveMujica";
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);

        _feverFx = FeverFXPresenter.TrySpawnUnderChess(target);

        _onUseSkillFever = OnUseSkillFever;
        target.skillController.onUseSkill.AddListener(_onUseSkillFever);

        string name = target.propertyController?.creator?.chessName ?? "";

        if (name.Contains(NameSaki))
        {
            _onTakeDamage = dm =>
            {
                if (dm == null || dm.damageFrom != target)
                    return;
                if (dm.damageType == DamageType.Heal)
                    return;
                AveMujica.Instance?.TryAddFever(1);
            };
            target.propertyController.onTakeDamage.AddListener(_onTakeDamage);
        }
        else if (name.Contains(NameUika))
        {
            _onHealDamage = dm =>
            {
                if (dm == null || dm.damageTo == null)
                    return;
                if (!IsAveMujicaTagged(dm.damageTo))
                    return;
                AveMujica.Instance?.TryAddFever(1);
            };
            target.propertyController.onHealDamage.AddListener(_onHealDamage);
        }
        else if (name.Contains(NameMutsumi))
        {
            _onSetDamage = dm =>
            {
                if (dm == null || dm.damageType == DamageType.Heal)
                    return;
                AveMujica.Instance?.TryAddFever(1);
            };
            target.propertyController.onSetDamage.AddListener(_onSetDamage);
        }
        else if (name.Contains(NameNyamu))
        {
            if (GameManage.instance?.timerManage != null)
                _nyamuTimer = GameManage.instance.timerManage.AddTimer(OnNyamuTick, 1f, true);
        }
    }

    void OnUseSkillFever(Chess user)
    {
        if (user != target)
            return;
        AveMujica.Instance?.OnAveMujicaMemberUsedSkill(user);
    }

    void OnNyamuTick()
    {
        AveMujica.Instance?.TryAddFever(1);
    }

    static bool IsAveMujicaTagged(Chess c)
    {
        return c.propertyController?.creator?.plantTags != null &&
               c.propertyController.creator.plantTags.Contains(AveMujica.AveMujicaPlantTag);
    }

    public override void BuffOver()
    {
        if (target != null && target.propertyController != null)
        {
            if (_onTakeDamage != null)
                target.propertyController.onTakeDamage.RemoveListener(_onTakeDamage);
            if (_onHealDamage != null)
                target.propertyController.onHealDamage.RemoveListener(_onHealDamage);
            if (_onSetDamage != null)
                target.propertyController.onSetDamage.RemoveListener(_onSetDamage);
        }
        if (target != null && target.skillController != null && _onUseSkillFever != null)
            target.skillController.onUseSkill.RemoveListener(_onUseSkillFever);

        if (_nyamuTimer != null)
        {
            _nyamuTimer.Stop();
            _nyamuTimer = null;
        }

        if (_feverFx != null)
        {
            _feverFx.DestroySafe();
            _feverFx = null;
        }

        _onTakeDamage = null;
        _onHealDamage = null;
        _onSetDamage = null;
        _onUseSkillFever = null;

        base.BuffOver();
    }
}
