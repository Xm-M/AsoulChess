using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 上原绯玛丽 AAO：按概率对场上所有友方单位施加攻速 Buff（同名不叠加）。
/// </summary>
public class SkillEffect_HimariAAO : ISkillEffect
{
    [SerializeReference, LabelText("AAO Buff 模板")]
    public Buff_AAO aaoBuff;

    [LabelText("攻速加成（acceleRated +）")]
    public float attackSpeedBonus = 0.25f;

    [LabelText("生效概率"), Range(0f, 1f)]
    public float procChance = 0.5f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (Random.value >= procChance)
            return;

        if (ChessTeamManage.Instance == null)
            return;

        var team = ChessTeamManage.Instance.GetTeam(user.tag);
        if (team == null)
            return;

        Buff_AAO buff = CreateBuffInstance();
        for (int i = 0; i < team.Count; i++)
        {
            Chess ally = team[i];
            if (ally == null || ally.IfDeath || ally.buffController == null)
                continue;
            ally.buffController.AddBuff(buff);
        }
    }

    Buff_AAO CreateBuffInstance()
    {
        if (aaoBuff != null)
        {
            var clone = aaoBuff.Clone() as Buff_AAO;
            if (clone != null)
                return clone;
        }

        return new Buff_AAO { speed = attackSpeedBonus };
    }
}

/// <summary>AAO 攻速增益；<see cref="BuffController"/> 按 buffName 去重，不可叠加。</summary>
[System.Serializable]
public class Buff_AAO : Buff_BaseValueBuff_AttackSpeed
{
    public const string BuffKey = "AAO";

    public Buff_AAO()
    {
        buffName = BuffKey;
        speed = 0.25f;
    }

    public override Buff Clone()
    {
        var c = (Buff_AAO)MemberwiseClone();
        c.buffName = BuffKey;
        return c;
    }
}
