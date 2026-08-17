using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 致盲：持续期间，持有者经 <see cref="PropertyController.TakeDamage"/> 打出的非治疗伤害类型变为 <see cref="DamageType.Miss"/>；
/// 身上挂致盲特效，结束时回收。
/// <para>
/// 近战等会复用同一条 <see cref="DamageMessege"/>，因此每次结算后必须把 <c>damageType</c> 还原，
/// 否则 Buff 结束后共享 DM 会一直停在 Miss。
/// </para>
/// </summary>
[Serializable]
public class Buff_Blind : TimeBuff
{
    public const string BuffKey = "致盲";

    [LabelText("致盲特效")]
    public GameObject blindEffect;

    UnityAction<DamageMessege> _onBeforeTakeDamage;
    UnityAction<DamageMessege> _onTakeDamageRestore;
    [NonSerialized] GameObject _spawnedEffect;
    [NonSerialized] DamageType _savedDamageType;
    [NonSerialized] bool _pendingRestore;

    public Buff_Blind()
    {
        buffName = BuffKey;
        continueTime = 2f;
    }

    public override Buff Clone()
    {
        var c = (Buff_Blind)base.Clone();
        c._onBeforeTakeDamage = null;
        c._onTakeDamageRestore = null;
        c._spawnedEffect = null;
        c._pendingRestore = false;
        return c;
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (target?.propertyController == null)
            return;

        target.propertyController.onBeforeTakeDamage ??= new UnityEvent<DamageMessege>();

        _onBeforeTakeDamage = OnBeforeTakeDamage;
        _onTakeDamageRestore = OnTakeDamageRestore;
        target.propertyController.onBeforeTakeDamage.AddListener(_onBeforeTakeDamage);
        target.propertyController.onTakeDamage.AddListener(_onTakeDamageRestore);

        if (blindEffect != null && ObjectPool.instance != null)
        {
            _spawnedEffect = ObjectPool.instance.Create(blindEffect);
            if (_spawnedEffect != null)
            {
                _spawnedEffect.transform.SetParent(target.transform);
                _spawnedEffect.transform.localPosition = Vector3.zero;
            }
        }
    }

    void OnBeforeTakeDamage(DamageMessege mes)
    {
        if (mes == null || target == null)
            return;
        if (mes.damageFrom != target)
            return;
        if (mes.damageType == DamageType.Heal)
            return;

        _savedDamageType = mes.damageType;
        mes.damageType = DamageType.Miss;
        _pendingRestore = true;
    }

    void OnTakeDamageRestore(DamageMessege mes)
    {
        if (!_pendingRestore || mes == null)
            return;
        if (mes.damageFrom != target)
            return;

        mes.damageType = _savedDamageType;
        _pendingRestore = false;
    }

    public override void BuffOver()
    {
        if (target?.propertyController != null)
        {
            if (_onBeforeTakeDamage != null)
                target.propertyController.onBeforeTakeDamage.RemoveListener(_onBeforeTakeDamage);
            if (_onTakeDamageRestore != null)
                target.propertyController.onTakeDamage.RemoveListener(_onTakeDamageRestore);
        }
        _onBeforeTakeDamage = null;
        _onTakeDamageRestore = null;
        _pendingRestore = false;

        if (_spawnedEffect != null)
        {
            if (ObjectPool.instance != null)
                ObjectPool.instance.Recycle(_spawnedEffect);
            else
                UnityEngine.Object.Destroy(_spawnedEffect);
            _spawnedEffect = null;
        }

        base.BuffOver();
    }
}
