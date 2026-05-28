using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 魅惑菇被动：受到带 <see cref="ElementType.CloseAttack"/> 且不含 <see cref="ElementType.Grind"/> 的伤害时，
/// 对攻击来源施加 <see cref="Buff_Charm"/>，并令自身 <see cref="Chess.Death"/>。
/// </summary>
[Serializable]
public class PassiveSkillEffect_MushRoom : ISkillEffect
{
    [SerializeReference, LabelText("魅惑 Buff")]
    public Buff_Charm charmBuff;

    Chess _user;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        _user = user;
        if (charmBuff == null)
        {
            Debug.LogWarning("[PassiveSkillEffect_MushRoom] 未配置 Buff_Charm");
            return;
        }

        user.propertyController.onGetDamage.AddListener(OnGetDamage);
        user.OnRemove.AddListener(OnChessRemove);
    }

    void OnGetDamage(DamageMessege dm)
    {
        if (_user == null || _user.IfDeath || dm.damageTo != _user) return;
        if (dm.damageFrom == null || dm.damageFrom.IfDeath) return;

        if ((dm.damageElementType & ElementType.CloseAttack) == 0) return;
        if ((dm.damageElementType & ElementType.Grind) != 0) return;

        dm.damageFrom.buffController.AddBuff(charmBuff);
        _user.Death();
    }

    void OnChessRemove(Chess chess)
    {
        if (_user != null)
        {
            _user.propertyController.onGetDamage.RemoveListener(OnGetDamage);
            _user.OnRemove.RemoveListener(OnChessRemove);
        }
        _user = null;
    }
}
