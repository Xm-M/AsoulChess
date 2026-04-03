using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 近战雪怪被动：普攻结算时若受击方已有 <see cref="FreezyBuff"/>（以 buffName 为 key），
/// 额外对其造成一次最大生命值 100% 的 <see cref="DamageType.Real"/> 伤害（不重复触发于 Real 伤害本身）。
/// 基础普攻仍在 CloseAttack / 武器上配置。
/// </summary>
[Serializable]
public class PassiveSkillEffect_SnowYetiMelee : ISkillEffect
{
    [Tooltip("与 FreezyBuff.buffName 一致，用于 buffDic.ContainsKey；留空则使用「冻结」")]
    public string freezyBuffKey = "冻结";

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        var r = user.GetComponent<SnowYetiMeleeRuntime>();
        if (r == null)
            r = user.gameObject.AddComponent<SnowYetiMeleeRuntime>();
        string key = string.IsNullOrEmpty(freezyBuffKey) ? "冻结" : freezyBuffKey;
        r.Setup(user, key);
    }
}

/// <summary>近战雪怪被动运行时：为敌方单位注册 onSetDamage，延后一帧追加真伤。</summary>
public class SnowYetiMeleeRuntime : MonoBehaviour
{
    Chess _user;
    string _freezyKey;
    readonly List<Chess> _hooked = new List<Chess>();
    UnityAction<Chess> _onPlantChess;

    public void Setup(Chess user, string freezyBuffKey)
    {
        if (_user != null)
            UnhookAll();

        _user = user;
        _freezyKey = freezyBuffKey;
        if (_user == null || string.IsNullOrEmpty(_freezyKey))
            return;

        HookExistingEnemies();
        _onPlantChess = OnPlantChess;
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), _onPlantChess);
        _user.OnRemove.AddListener(OnUserRemove);
    }

    void HookExistingEnemies()
    {
        var team = ChessTeamManage.Instance?.GetEnemyTeam(_user.tag);
        if (team == null) return;
        for (int i = 0; i < team.Count; i++)
            HookVictim(team[i]);
    }

    void OnPlantChess(Chess chess)
    {
        if (_user == null || chess == null) return;
        var enemyTeam = ChessTeamManage.Instance?.GetEnemyTeam(_user.tag);
        if (enemyTeam == null || !enemyTeam.Contains(chess)) return;
        HookVictim(chess);
    }

    void HookVictim(Chess victim)
    {
        if (victim == null || victim.propertyController == null || _hooked.Contains(victim)) return;
        victim.propertyController.onSetDamage.AddListener(OnVictimSetDamage);
        _hooked.Add(victim);
    }

    void UnhookVictim(Chess victim)
    {
        if (victim == null || victim.propertyController == null) return;
        victim.propertyController.onSetDamage.RemoveListener(OnVictimSetDamage);
        _hooked.Remove(victim);
    }

    void OnVictimSetDamage(DamageMessege mes)
    {
        if (mes == null || _user == null) return;
        if (mes.damageFrom != _user) return;
        if (mes.damageType == DamageType.Real) return;
        Chess victim = mes.damageTo;
        if (victim == null || victim.IfDeath) return;
        if (victim.buffController == null || victim.buffController.buffDic == null) return;
        if (!victim.buffController.buffDic.ContainsKey(_freezyKey)) return;

        StartCoroutine(ApplyExtraRealDamageNextFrame(victim));
    }

    IEnumerator ApplyExtraRealDamageNextFrame(Chess victim)
    {
        yield return null;
        if (_user == null || victim == null || victim.IfDeath) yield break;
        if (victim.buffController == null || victim.buffController.buffDic == null) yield break;
        if (!victim.buffController.buffDic.ContainsKey(_freezyKey)) yield break;

        float maxHp = victim.propertyController.GetMaxHp();
        var dm = new DamageMessege(_user, victim, maxHp, DamageType.Real, ElementType.None);
        victim.propertyController.GetDamage(dm);
    }

    void OnUserRemove(Chess _)
    {
        UnhookAll();
        Destroy(this);
    }

    void UnhookAll()
    {
        if (_onPlantChess != null)
        {
            EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), _onPlantChess);
            _onPlantChess = null;
        }

        if (_user != null)
            _user.OnRemove.RemoveListener(OnUserRemove);

        for (int i = _hooked.Count - 1; i >= 0; i--)
            UnhookVictim(_hooked[i]);
        _hooked.Clear();
        _user = null;
    }

    void OnDisable()
    {
        UnhookAll();
    }
}
