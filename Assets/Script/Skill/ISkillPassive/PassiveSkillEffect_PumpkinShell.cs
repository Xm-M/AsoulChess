using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 南瓜罩类 Support 被动：同格存在 <see cref="PlantType.MainPlant"/>（<see cref="Tile.stander"/>）时，
/// 在 <see cref="PropertyController.onSetDamage"/> 将本次对 Main 的**可转移伤害**改为由自身结算（仅数值，不带 <see cref="DamageMessege.takeBuff"/>）。
/// 不转移：<see cref="DamageType.Heal"/> / <see cref="DamageType.Real"/> / <see cref="DamageType.Miss"/>。
/// <see cref="ElementType.Grind"/>：Main 与南瓜各自按 <see cref="PropertyController.GetDamage"/> 中碾压逻辑做体型判定。
/// </summary>
[Serializable]
public class PassiveSkillEffect_PumpkinShell : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        PumpkinShellRedirectRuntime r = user.GetComponent<PumpkinShellRedirectRuntime>();
        if (r == null)
            r = user.gameObject.AddComponent<PumpkinShellRedirectRuntime>();
        r.Apply(user);
    }
}

/// <summary>由 <see cref="PassiveSkillEffect_PumpkinShell"/> 添加；勿在同一预制体上重复挂多个。</summary>
public class PumpkinShellRedirectRuntime : MonoBehaviour
{
    Chess _pumpkin;
    Chess _protectee;
    bool _applyingRedirect;

    public void Apply(Chess pumpkin)
    {
        if (pumpkin == null) return;
        if (_pumpkin == pumpkin) return;
        if (_pumpkin != null)
            _pumpkin.OnRemove.RemoveListener(OnPumpkinRemove);
        UnbindProtectee();
        _pumpkin = pumpkin;
        pumpkin.OnRemove.AddListener(OnPumpkinRemove);
    }

    void LateUpdate()
    {
        if (_pumpkin == null || _pumpkin.IfDeath) return;
        TryBindProtectee();
    }

    void OnPumpkinRemove(Chess _)
    {
        UnbindProtectee();
        if (_pumpkin != null)
            _pumpkin.OnRemove.RemoveListener(OnPumpkinRemove);
        _pumpkin = null;
    }

    void OnDestroy()
    {
        UnbindProtectee();
        if (_pumpkin != null)
            _pumpkin.OnRemove.RemoveListener(OnPumpkinRemove);
    }

    void TryBindProtectee()
    {
        Tile t = _pumpkin.moveController?.standTile;
        if (t == null)
        {
            UnbindProtectee();
            return;
        }

        Chess stander = t.stander;
        Chess main = null;
        if (stander != null && stander != _pumpkin &&
            stander.propertyController != null &&
            stander.propertyController.creator != null &&
            stander.propertyController.creator.plantType == PlantType.MainPlant &&
            !stander.IfDeath)
            main = stander;

        if (main == _protectee) return;

        UnbindProtectee();
        if (main == null) return;

        _protectee = main;
        _protectee.propertyController.onSetDamage.AddListener(OnProtecteeSetDamage);
    }

    void UnbindProtectee()
    {
        if (_protectee != null && _protectee.propertyController != null)
            _protectee.propertyController.onSetDamage.RemoveListener(OnProtecteeSetDamage);
        _protectee = null;
    }

    void OnProtecteeSetDamage(DamageMessege mes)
    {
        if (_applyingRedirect || _pumpkin == null || _pumpkin.IfDeath || _protectee == null || _protectee.IfDeath)
            return;
        if (mes == null || mes.damageTo != _protectee)
            return;
        if (!ShouldRedirectDamageType(mes.damageType))
            return;

        float amount = mes.damage;
        mes.damage = 0f;

        var redirect = new DamageMessege(
            mes.damageFrom,
            _pumpkin,
            amount,
            mes.damageType,
            mes.damageElementType)
        {
            ifCrit = mes.ifCrit,
            suppressFloatingDamage = mes.suppressFloatingDamage,
            takeBuff = null
        };

        _applyingRedirect = true;
        try
        {
            _pumpkin.propertyController.GetDamage(redirect);
        }
        finally
        {
            _applyingRedirect = false;
        }

        // Main 碾压不可在 onSetDamage 同步 Death（会打断 GetDamage）；南瓜侧已在 GetDamage(redirect) 内判定
        if ((mes.damageElementType & ElementType.Grind) != 0)
            StartCoroutine(CoDeferredMainGrind(_protectee, mes));
    }

    IEnumerator CoDeferredMainGrind(Chess main, DamageMessege mes)
    {
        yield return null;
        TryGrindDeath(main, mes);
    }

    static bool ShouldRedirectDamageType(DamageType t)
    {
        return t != DamageType.Heal && t != DamageType.Real && t != DamageType.Miss;
    }

    /// <summary>与 <see cref="PropertyController.GetDamage"/> 中碾压块一致；在 Main 已转移伤害后仍判定 Main 是否被碾压。</summary>
    static void TryGrindDeath(Chess victim, DamageMessege mes)
    {
        if (victim == null || victim.IfDeath || mes.damageFrom == null || mes.damageFrom.IfDeath)
            return;
        if (victim.propertyController == null || mes.damageFrom.propertyController == null)
            return;
        if (mes.damageFrom.propertyController.GetSize() <= victim.propertyController.GetSize())
            return;
        UIManage.GetView<DamagePanel>().ShowText(mes, "GRIND!", Color.white);
        victim.Death();
    }
}
