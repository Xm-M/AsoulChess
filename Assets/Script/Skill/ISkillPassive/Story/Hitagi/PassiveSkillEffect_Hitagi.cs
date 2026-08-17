using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 战场原黑仪被动：入场挂压力；stress 变化时同步体型 Size = 1 - floor(stress/20)（可负）。
/// 额外弹数由攻击侧按同一公式读取。
/// </summary>
[Serializable]
public class PassiveSkillEffect_Hitagi : ISkillEffect
{
    [SerializeReference]
    [Tooltip("自身压力 Buff 模板")]
    public Buff_StressBuff_Death selfStressBuff;

    [Min(1)]
    public int stressPerTier = 20;

    public int baseSize = 1;

    Chess _user;
    UnityAction _onStress;
    UnityAction<Chess> _onRemove;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        _user = user;

        EnsureSelfStress(user);
        SyncSizeFromStress();

        _onStress = SyncSizeFromStress;
        user.skillController?.context?.AddEvent(_onStress);

        _onRemove = OnRemove;
        user.OnRemove.AddListener(_onRemove);
    }

    void EnsureSelfStress(Chess user)
    {
        if (user.buffController == null) return;
        if (user.buffController.buffDic != null && user.buffController.buffDic.ContainsKey("压力"))
            return;

        Buff_StressBuff_Death buff = selfStressBuff != null
            ? (Buff_StressBuff_Death)selfStressBuff.Clone()
            : new Buff_StressBuff_Death { stressLimit = 999 };
        user.buffController.AddBuff(buff);
    }

    void SyncSizeFromStress()
    {
        if (_user?.propertyController == null) return;

        int stress = 0;
        _user.skillController?.context?.TryGet("stress", out stress);
        int tier = Mathf.Max(0, stress) / Mathf.Max(1, stressPerTier);
        _user.propertyController.SetSizeRaw(baseSize - tier);
    }

    void OnRemove(Chess chess)
    {
        if (_user != null)
        {
            if (_onStress != null)
                _user.skillController?.context?.RemoveEvent(_onStress);
            if (_onRemove != null)
                _user.OnRemove.RemoveListener(_onRemove);

            var ctx = _user.skillController?.context;
            if (ctx != null && ctx.TryGet("hitagi_stationery_salvo", out Coroutine salvo) && salvo != null)
            {
                _user.StopCoroutine(salvo);
                ctx.Remove("hitagi_stationery_salvo");
            }
        }
        _user = null;
        _onStress = null;
        _onRemove = null;
    }
}
