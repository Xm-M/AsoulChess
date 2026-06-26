using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckReplace_M3 : ICheckReplace
{
    ReplaceSkill replace;
    Chess user;
    public string buffName;

    public void WhenEnter(Chess user, ReplaceSkill replaceSkill)
    {
        this.replace = replaceSkill;
        this.user = user;
        user.skillController.context.AddEvent(CheckCrystal);
        user.skillController.onSkillOver.AddListener(OnSkillOver);
    }

    public void WhenLeave(Chess user, ReplaceSkill replaceSkill)
    {
        user.skillController.context.RemoveEvent(CheckCrystal);
        user.skillController.onSkillOver.RemoveListener(OnSkillOver);
    }

    static bool InSkillState(Chess chess) =>
        chess?.stateController?.currentState?.state?.stateName == StateName.SkillState;

    bool TryGetCrystal(out Chess crystal)
    {
        crystal = null;
        if (user?.skillController?.context == null)
            return false;
        if (!user.skillController.context.TryGet<Chess>(buffName, out crystal))
            return false;
        if (crystal == null || crystal.IfDeath)
        {
            crystal = null;
            return false;
        }
        return true;
    }

    public void CheckCrystal()
    {
        if (user == null || user.IfDeath || replace == null)
            return;

        if (TryGetCrystal(out _))
        {
            if (replace.CurrentSkillIndex != 1)
                ApplySkillIndex(1);
        }
        else if (replace.CurrentSkillIndex != 0)
        {
            // 哈气结束前 context 可能仍在；由 OnSkillOver 协程在 GoBack 后再同步
            if (InSkillState(user))
                return;
            ApplySkillIndex(0);
        }
    }

    void ApplySkillIndex(int skillIndex)
    {
        if (user == null || user.IfDeath || replace == null)
            return;

        user.animatorController.ChangeInt(skillIndex);
        user.StartCoroutine(IEChange(replace, skillIndex));
    }

    void OnSkillOver(Chess chess)
    {
        if (chess != user)
            return;
        user.StartCoroutine(SyncSkillAfterSkillOver());
    }

    /// <summary>
    /// 等本帧所有 onSkillOver（含 GoBack、context 清理）结束后再对齐子技能索引。
    /// </summary>
    IEnumerator SyncSkillAfterSkillOver()
    {
        yield return null;
        if (user == null || user.IfDeath || replace == null)
            yield break;

        if (TryGetCrystal(out _))
        {
            if (replace.CurrentSkillIndex != 1)
                ApplySkillIndex(1);
        }
        else if (replace.CurrentSkillIndex != 0)
        {
            ApplySkillIndex(0);
        }
    }

    IEnumerator IEChange(ReplaceSkill replaceSkill, int skillIndex)
    {
        yield return null;
        replaceSkill.ChangeSkill(skillIndex);
    }
}

public class SkillEffect_Mon3tr : ISkillEffect
{
    [SerializeReference]
    public Buff_Create buff;

    [Tooltip("召唤重构体 PropertyCreator；配置后覆盖 buff 内 creator")]
    public PropertyCreator rebuildCreator;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (buff == null)
            return;
        if (rebuildCreator != null)
            buff.creator = rebuildCreator;
        user.buffController.AddBuff(buff);
    }
}

/// <summary>
/// M3 哈气：与重构体换位 → 技能态强化（生命×10、体型+10、吸血+1）→ 持续圆形真实伤害；
/// 吸血治疗时对周围九宫格友方溅射等量治疗；结束时归位并消耗重构体（重构体已死则 M3 死亡）。
/// </summary>
public class SkillEffect_M3Rebuild : ISkillEffect
{
    public const string TransformBuffName = "M3RebuildMode";

    [SerializeReference]
    public IFindTarget findTarget;

    public string buffName;

    Chess _user;
    Chess _crystal;
    bool _swapped;

    readonly List<Tile> _tileBuffer = new List<Tile>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        _user = user;
        if (!_swapped)
        {
            if (!TryResolveCrystal(user))
                return;
            if (!TrySwap(user))
                return;

            user.buffController.AddBuff(new Buff_M3RebuildMode());
            RegisterListeners(user);
            _swapped = true;
        }
        else
        {
            ApplyCircleDamage(user, config, targets);
        }
    }

    bool TryResolveCrystal(Chess user)
    {
        _crystal = null;
        if (user.skillController.context.TryGet<Chess>(buffName, out Chess ctxCrystal)
            && ctxCrystal != null
            && !ctxCrystal.IfDeath)
        {
            _crystal = ctxCrystal;
            return true;
        }

        return false;
    }

    bool TrySwap(Chess user)
    {
        if (_crystal?.moveController?.standTile == null || user.moveController?.standTile == null)
            return false;

        Tile targetTile = _crystal.moveController.standTile;
        Tile selfTile = user.moveController.standTile;
        targetTile.ChessLeave(_crystal);
        selfTile.ChessLeave(user);
        targetTile.ChessEnter(user);
        targetTile.PlantChess(user);
        selfTile.ChessEnter(_crystal);
        selfTile.PlantChess(_crystal);
        return true;
    }

    void RegisterListeners(Chess user)
    {
        user.skillController.onSkillOver.RemoveListener(GoBack);
        user.skillController.onSkillOver.AddListener(GoBack);
        user.propertyController.onTakeDamage.RemoveListener(OnTakeDamage);
        user.propertyController.onTakeDamage.AddListener(OnTakeDamage);
        user.propertyController.onHealDamage.RemoveListener(OnSelfHeal);
        user.propertyController.onHealDamage.AddListener(OnSelfHeal);
        user.OnRemove.RemoveListener(OnUserRemove);
        user.OnRemove.AddListener(OnUserRemove);
        _crystal.OnRemove.RemoveListener(OnCrystalRemove);
        _crystal.OnRemove.AddListener(OnCrystalRemove);
    }

    void ApplyCircleDamage(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (findTarget == null || config?.baseDamage == null || config.baseDamage.Count == 0)
            return;

        findTarget.FindTarget(user, targets);
        float mult = config.baseDamage[0];
        for (int i = 0; i < targets.Count; i++)
        {
            Chess enemy = targets[i];
            if (enemy == null || enemy.IfDeath)
                continue;

            user.skillController.DM.damageFrom = user;
            user.skillController.DM.damageTo = enemy;
            user.skillController.DM.damageElementType = ElementType.CloseAttack;
            user.skillController.DM.damageType = DamageType.Real;
            user.skillController.DM.damage = user.propertyController.GetAttack() * mult;
            user.propertyController.TakeDamage(user.skillController.DM);
        }
    }

    /// <summary>吸血：按造成伤害量溅射九宫格友方（与自身吸血量同源数值）。</summary>
    void OnTakeDamage(DamageMessege dm)
    {
        if (!_swapped || _user == null || dm == null || dm.damageFrom != _user)
            return;
        if (dm.damageType == DamageType.Heal || dm.damageType == DamageType.Miss)
            return;

        float ls = _user.propertyController.GetLifeStealing();
        if (ls <= 0f)
            return;

        float healAmount = dm.damage * ls;
        if (healAmount <= 0f)
            return;

        SplashHealAllies(healAmount);
    }

    /// <summary>直接治疗自身时（如治愈波命中自己），九宫格友方获得相同治疗量。</summary>
    void OnSelfHeal(DamageMessege dm)
    {
        if (!_swapped || _user == null || dm == null)
            return;
        if (dm.damageFrom != _user || dm.damageType != DamageType.Heal || dm.damageTo != _user)
            return;

        SplashHealAllies(dm.damage);
    }

    void SplashHealAllies(float healAmount)
    {
        if (_user == null || healAmount <= 0f)
            return;
        if (!WisadelGridHelper.TryGetStandOrEstimatedTile(_user, out Tile centerTile))
            return;

        WisadelGridHelper.CollectNineGridTiles(centerTile, _tileBuffer);
        for (int i = 0; i < _tileBuffer.Count; i++)
        {
            Tile tile = _tileBuffer[i];
            Chess ally = tile?.stander;
            if (ally == null || ally.IfDeath || ally == _user)
                continue;
            if (!ally.CompareTag(_user.tag))
                continue;

            var splash = new DamageMessege(_user, ally, healAmount, DamageType.Heal, ElementType.AOE);
            _user.propertyController.TakeDamage(splash);
        }
    }

    void OnCrystalRemove(Chess chess)
    {
        if (chess != _crystal)
            return;
        _crystal = null;
    }

    void OnUserRemove(Chess chess)
    {
        if (chess != _user)
            return;
        RemoveTransformBuff();
        UnregisterListeners();
        ResetState();
    }

    public void GoBack(Chess chess)
    {
        if (chess != _user)
            return;

        RemoveTransformBuff();
        UnregisterListeners();

        if (_user == null || _user.IfDeath)
        {
            ResetState();
            return;
        }

        Tile standTile = _user.moveController?.standTile;
        if (standTile != null)
            standTile.ChessLeave(_user);

        if (_crystal == null || _crystal.IfDeath || _crystal.moveController?.standTile == null)
        {
            _user.Death();
            ResetState();
            return;
        }

        Tile targetTile = _crystal.moveController.standTile;
        _crystal.Death();
        targetTile.ChessEnter(_user);
        targetTile.PlantChess(_user);
        ResetState();
    }

    void RemoveTransformBuff()
    {
        if (_user?.buffController?.buffDic != null
            && _user.buffController.buffDic.TryGetValue(TransformBuffName, out Buff buff))
            buff.BuffOver();
    }

    void UnregisterListeners()
    {
        if (_user != null)
        {
            _user.skillController.onSkillOver.RemoveListener(GoBack);
            _user.propertyController.onTakeDamage.RemoveListener(OnTakeDamage);
            _user.propertyController.onHealDamage.RemoveListener(OnSelfHeal);
            _user.OnRemove.RemoveListener(OnUserRemove);
        }

        if (_crystal != null)
            _crystal.OnRemove.RemoveListener(OnCrystalRemove);
    }

    void ResetState()
    {
        _swapped = false;
        _crystal = null;
        _user = null;
    }
}

/// <summary>哈气技能态：MaxHp×10、体型+10、生命偷取+1。</summary>
public class Buff_M3RebuildMode : Buff
{
    Buff_BaseValueBuff_HPmax _hpBuff;
    Buff_BaseValueBuff_Size _sizeBuff;
    Buff_BaseValueBuff_LifeSteal _lifeStealBuff;

    public Buff_M3RebuildMode()
    {
        buffName = SkillEffect_M3Rebuild.TransformBuffName;
    }

    public override Buff Clone()
    {
        var c = (Buff_M3RebuildMode)base.Clone();
        c._hpBuff = _hpBuff != null ? (Buff_BaseValueBuff_HPmax)_hpBuff.Clone() : null;
        c._sizeBuff = _sizeBuff != null ? (Buff_BaseValueBuff_Size)_sizeBuff.Clone() : null;
        c._lifeStealBuff = _lifeStealBuff != null ? (Buff_BaseValueBuff_LifeSteal)_lifeStealBuff.Clone() : null;
        return c;
    }

    void EnsureSubBuffs(Chess target)
    {
        if (_hpBuff == null)
            _hpBuff = new Buff_BaseValueBuff_HPmax();
        if (_sizeBuff == null)
            _sizeBuff = new Buff_BaseValueBuff_Size { size = 10 };
        if (_lifeStealBuff == null)
            _lifeStealBuff = new Buff_BaseValueBuff_LifeSteal { lifeSteal = 1f };

        float maxHp = target.propertyController.GetMaxHp();
        _hpBuff.hpmax = maxHp * 9f;
    }

    protected override void PrepareForRestore()
    {
        if (_hpBuff == null)
            _hpBuff = new Buff_BaseValueBuff_HPmax();
        if (_sizeBuff == null)
            _sizeBuff = new Buff_BaseValueBuff_Size { size = 10 };
        if (_lifeStealBuff == null)
            _lifeStealBuff = new Buff_BaseValueBuff_LifeSteal { lifeSteal = 1f };
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        EnsureSubBuffs(target);
        _hpBuff.target = target;
        _hpBuff.BuffEffect(target);
        _sizeBuff.target = target;
        _sizeBuff.BuffEffect(target);
        _lifeStealBuff.target = target;
        _lifeStealBuff.BuffEffect(target);
    }

    public override void BuffOver()
    {
        if (_lifeStealBuff != null)
            _lifeStealBuff.BuffOver();
        if (_sizeBuff != null)
            _sizeBuff.BuffOver();
        if (_hpBuff != null)
            _hpBuff.BuffOver();
        base.BuffOver();
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as Buff_M3RebuildMode;
        if (other?._hpBuff != null && _hpBuff != null)
            _hpBuff.BuffReset(other._hpBuff);
        if (other?._sizeBuff != null && _sizeBuff != null)
            _sizeBuff.BuffReset(other._sizeBuff);
        if (other?._lifeStealBuff != null && _lifeStealBuff != null)
            _lifeStealBuff.BuffReset(other._lifeStealBuff);
    }
}
