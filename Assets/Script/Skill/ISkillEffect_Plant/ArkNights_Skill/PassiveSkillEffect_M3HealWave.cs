using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// M3 被动：治愈波主目标命中后，九宫格友方获得等量治疗；所有受 M3 治疗的单位获得攻速增益。
/// 仅响应 <see cref="ElementType.Bullet"/> 治疗（溅射使用 AOE，避免递归触发）。
/// </summary>
public class PassiveSkillEffect_M3HealWave : ISkillEffect
{
    [LabelText("攻速增益比例"), Tooltip("ChangeAcceleRate 增量，0.2 = +20%")]
    public float attackSpeedBonus = 0.2f;

    [LabelText("攻速 Buff 持续(秒)")]
    public float hasteDuration = 5f;

    Chess _user;
    readonly List<Tile> _tileBuffer = new List<Tile>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        _user = user;
        user.propertyController.onHealDamage.AddListener(OnHealDamage);
        user.OnRemove.AddListener(OnChessRemove);
    }

    void OnHealDamage(DamageMessege dm)
    {
        if (_user == null || dm == null)
            return;
        if (dm.damageFrom != _user || dm.damageType != DamageType.Heal)
            return;
        if ((dm.damageElementType & ElementType.Bullet) == 0)
            return;

        Chess main = dm.damageTo;
        if (main == null || main.IfDeath)
            return;

        float healAmount = dm.damage;
        ApplyHaste(main);

        if (!WisadelGridHelper.TryGetStandOrEstimatedTile(main, out Tile mainTile))
            return;

        WisadelGridHelper.CollectNineGridTiles(mainTile, _tileBuffer);
        for (int i = 0; i < _tileBuffer.Count; i++)
        {
            Tile tile = _tileBuffer[i];
            Chess ally = tile?.stander;
            if (ally == null || ally.IfDeath || ally == main)
                continue;
            if (!ally.CompareTag(_user.tag))
                continue;

            ApplyHaste(ally);
            var splash = new DamageMessege(_user, ally, healAmount, DamageType.Heal, ElementType.AOE);
            _user.propertyController.TakeDamage(splash);
        }
    }

    void ApplyHaste(Chess ally)
    {
        if (ally == null || ally.IfDeath)
            return;

        var buff = new Buff_M3HealHaste
        {
            continueTime = hasteDuration,
            attackSpeedBuff = new Buff_BaseValueBuff_AttackSpeed { speed = attackSpeedBonus }
        };
        ally.buffController.AddBuff(buff);
    }

    void OnChessRemove(Chess c)
    {
        if (_user?.propertyController != null)
            _user.propertyController.onHealDamage.RemoveListener(OnHealDamage);
        if (_user != null)
            _user.OnRemove.RemoveListener(OnChessRemove);
        _user = null;
    }
}

/// <summary>M3 治疗攻速增益：<see cref="TimeBuff"/> + <see cref="Buff_BaseValueBuff_AttackSpeed"/>。</summary>
public class Buff_M3HealHaste : TimeBuff
{
    public const string DefaultBuffName = "M3HealHaste";

    [SerializeReference]
    public Buff_BaseValueBuff_AttackSpeed attackSpeedBuff;

    public Buff_M3HealHaste()
    {
        buffName = DefaultBuffName;
    }

    public override Buff Clone()
    {
        var c = (Buff_M3HealHaste)base.Clone();
        c.attackSpeedBuff = attackSpeedBuff != null
            ? (Buff_BaseValueBuff_AttackSpeed)attackSpeedBuff.Clone()
            : null;
        return c;
    }

    void EnsureBuff()
    {
        if (attackSpeedBuff == null)
            attackSpeedBuff = new Buff_BaseValueBuff_AttackSpeed { speed = 0.2f };
    }

    protected override void PrepareForRestore() => EnsureBuff();

    public override void BuffEffect(Chess target)
    {
        EnsureBuff();
        base.BuffEffect(target);
        attackSpeedBuff.target = target;
        attackSpeedBuff.BuffEffect(target);
    }

    public override void BuffOver()
    {
        if (attackSpeedBuff != null)
            attackSpeedBuff.BuffOver();
        base.BuffOver();
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as Buff_M3HealHaste;
        if (other?.attackSpeedBuff != null && attackSpeedBuff != null)
            attackSpeedBuff.BuffReset(other.attackSpeedBuff);
    }
}
