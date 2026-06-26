using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 闪电：累计己方种植 <see cref="plantCountThreshold"/> 次后，随机对 1 名可选敌方造成
/// <see cref="damageMessage"/> 配置的伤害，并在目标处生成 <see cref="strikeEffectPrefab"/>。
/// </summary>
[Serializable]
public class PropEffect_Lightning : PropEffect
{
    [LabelText("种植次数 n")]
    [MinValue(1)]
    public int plantCountThreshold = 3;

    [LabelText("伤害模板")]
    [Tooltip("与 CloseAttack.DM / Fetter.DM 相同；运行时 damageFrom=null，damageTo=目标")]
    public DamageMessege damageMessage = new DamageMessege
    {
        damage = 200f,
        damageType = DamageType.Magic,
        damageElementType = ElementType.None,
    };

    [LabelText("命中特效")]
    [Tooltip("在目标位置通过 ObjectPool 生成；留空则无特效")]
    public GameObject strikeEffectPrefab;

    int plantCount;
    bool listening;

    public override void Apply(PropContext ctx)
    {
        plantCount = 0;
        if (listening)
            return;
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
        listening = true;
    }

    public override void Remove(PropContext ctx)
    {
        if (listening)
        {
            EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
            listening = false;
        }
        plantCount = 0;
    }

    void OnPlantChess(Chess chess)
    {
        if (chess == null || chess.IfDeath || !chess.CompareTag("Player"))
            return;

        plantCount++;
        if (plantCount < plantCountThreshold)
            return;

        plantCount = 0;
        TryStrikeRandomEnemy();
    }

    void TryStrikeRandomEnemy()
    {
        var team = GameManage.instance?.chessTeamManage;
        if (team == null)
            return;

        var enemies = team.GetEnemyTeam("Player");
        if (enemies == null || enemies.Count == 0)
            return;

        var candidates = new List<Chess>();
        for (int i = 0; i < enemies.Count; i++)
        {
            var c = enemies[i];
            if (IsSelectableEnemy(c))
                candidates.Add(c);
        }

        if (candidates.Count == 0)
            return;

        int idx = UnityEngine.Random.Range(0, candidates.Count);
        var target = candidates[idx];
        if (target?.propertyController == null)
            return;

        ApplyDamageFromTemplate(target);
        SpawnStrikeEffectAt(target);
    }

    void ApplyDamageFromTemplate(Chess target)
    {
        var template = damageMessage ?? new DamageMessege();
        var mes = new DamageMessege(
            null,
            target,
            template.damage,
            template.damageType,
            template.damageElementType);
        mes.ifCrit = template.ifCrit;
        mes.suppressFloatingDamage = template.suppressFloatingDamage;
        mes.takeBuff = template.takeBuff;
        target.propertyController.GetDamage(mes);
    }

    void SpawnStrikeEffectAt(Chess target)
    {
        if (strikeEffectPrefab == null || target == null || ObjectPool.instance == null)
            return;

        var go = ObjectPool.instance.Create(strikeEffectPrefab);
        if (go != null)
            go.transform.position = target.transform.position;
    }

    static bool IsSelectableEnemy(Chess chess)
    {
        if (chess == null || chess.IfDeath || chess.IfSelectable)
            return false;
        return chess.CompareTag("Enemy");
    }
}
