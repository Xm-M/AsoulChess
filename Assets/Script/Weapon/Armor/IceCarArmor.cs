using System;
using UnityEngine;

/// <summary>
/// 冰车防具：二类（溢出不进本体、Buff 不作用于本体）；非 AOE 子弹扣甲后回收；AOE 子弹不回收。
/// 碾压：使用 Inspector 中 <see cref="CarArmor.dm"/> 的 <see cref="DamageMessege.damageType"/> / <see cref="DamageMessege.damageElementType"/>，
/// 伤害数值为 <see cref="PropertyController.GetAttack"/>；对植物走 <see cref="PropertyController.TakeDamage"/>。
/// 车爆：<see cref="carBreakEffect"/> 可选，随后 <see cref="BrokenArmor"/> 并进入 <see cref="StateName.DeathState"/>（<see cref="DeathState"/> 内播死亡动画与亡语）。
/// </summary>
public class IceCarArmor : CarArmor
{
    public float armorMax = 300f;
    [Tooltip("车爆特效（可选）；在 BrokenArmor 时于僵尸位置生成")]
    public GameObject carBreakEffect;
    public AudioPlayer au;

    [Tooltip("护甲当前值/最大值低于该比例时，开始按间隔自动扣护甲（相对 armorMax）")]
    [Range(0f, 1f)]
    public float lowArmorBleedThreshold = 0.15f;

    [Tooltip("进入低护甲后，每隔多少秒扣一次护甲")]
    public float armorBleedIntervalSeconds = 1f;

    [Tooltip("每次扣除的护甲数值（每秒一次即等价于每秒扣这么多）")]
    public float armorBleedPerTick = 20f;

    [SerializeField]
    float armorCurrent;

    bool _broken;
    Timer _armorBleedTimer;

    /// <summary>破甲时若最后一次致命链来自爆炸伤害，供 <see cref="AnimatorController_SampleZombie.PlayDeath"/> 使用 DeathVariant。</summary>
    bool _explodeDeathVariant;

    public bool ExplodeDeathVariant => _explodeDeathVariant;

    public override void InitArmor()
    {
        armorCurrent = armorMax;
        user.WhenEnterGame.AddListener(ResetArmor);
    }

    public override void ResetArmor(Chess chess)
    {
        StopArmorBleedTimer();
        tag = user.tag;
        _broken = false;
        _explodeDeathVariant = false;
        armorCurrent = armorMax;
        user.propertyController.onSetDamage.AddListener(GetDamage);
        SyncArmorVisualToUserAnimator();
    }

    /// <summary>二类：Buff 不传递到本体；溢出不进本体；伤害先由车甲承担。</summary>
    public override void GetDamage(DamageMessege dm)
    {
        if (_broken || user == null || dm == null) return;

        dm.takeBuff = null;
        bool explode = (dm.damageElementType & ElementType.Explode) != 0;
        au?.RandomPlay();
        if (armorCurrent >= dm.damage)
        {
            dm.damage *= (1f - user.propertyController.GetExtraDefence());
            if (!dm.suppressFloatingDamage)
            {
                if (dm.damageTo == null)
                    dm.damageTo = user;
                var damagePanel = UIManage.GetView<DamagePanel>();
                if (damagePanel != null)
                    damagePanel.ShowDamageMes(dm);
            }
            armorCurrent -= dm.damage;
            //Debug.Log("受到" + dm.damage + "伤害");
            if ((dm.damageElementType & ElementType.Explode) == 0)
                dm.damage = 0;
            else if (dm.damage > user.propertyController.GetHp())
                BrokenArmorFromDamage(explode);

            player?.RandomPlay();

            if (!_broken && armorCurrent <= 0f)
            {
                if ((dm.damageElementType & ElementType.Explode) == 0)
                    dm.damage = 0;
                BrokenArmorFromDamage(explode);
            }
        }
        else
        {
            armorCurrent = 0f;
            if ((dm.damageElementType & ElementType.Explode) == 0)
            {
                // 二类：多余伤害不进本体
            }
            dm.damage = 0f;
            BrokenArmorFromDamage(explode);
        }

        if (!_broken)
        {
            SyncArmorVisualToUserAnimator();
            TryStartArmorBleedTimer();
        }
    }

    float ArmorRatio() => armorMax > 0f ? armorCurrent / armorMax : 0f;

    void TryStartArmorBleedTimer()
    {
        if (_broken || user == null || user.IfDeath) return;
        if (armorMax <= 0f) return;
        if (ArmorRatio() >= lowArmorBleedThreshold) return;
        if (_armorBleedTimer != null && !_armorBleedTimer.IsFinish) return;
        if (GameManage.instance == null || GameManage.instance.timerManage == null) return;

        float interval = Mathf.Max(0.05f, armorBleedIntervalSeconds);
        _armorBleedTimer = GameManage.instance.timerManage.AddTimer(OnArmorBleedTick, interval, true);
    }

    void OnArmorBleedTick()
    {
        if (_broken || user == null || user.IfDeath)
        {
            StopArmorBleedTimer();
            return;
        }
        if (armorMax <= 0f)
        {
            StopArmorBleedTimer();
            return;
        }
        if (ArmorRatio() >= lowArmorBleedThreshold)
        {
            StopArmorBleedTimer();
            return;
        }

        armorCurrent -= armorBleedPerTick;
        if (armorCurrent <= 0f)
        {
            armorCurrent = 0f;
            SyncArmorVisualToUserAnimator();
            BrokenArmorFromDamage(false);
            StopArmorBleedTimer();
            return;
        }

        SyncArmorVisualToUserAnimator();
    }

    void StopArmorBleedTimer()
    {
        if (_armorBleedTimer != null)
        {
            _armorBleedTimer.Stop();
            _armorBleedTimer = null;
        }
    }

    void BrokenArmorFromDamage(bool fromExplodeDamage)
    {
        if (_broken) return;
        if (fromExplodeDamage)
            _explodeDeathVariant = true;
        BrokenArmor();
    }

    /// <summary>
    /// 与 <see cref="AnimatorController_SampleZombie.SyncVisualTierToAnimator"/> 相同档位规则，比例改为护甲 <see cref="armorCurrent"/> / <see cref="armorMax"/>。
    /// </summary>
    void SyncArmorVisualToUserAnimator()
    {
        if (user == null || user.animatorController == null) return;
        float ratio = armorMax > 0f ? armorCurrent / armorMax : 0f;
        if (user.animatorController is AnimatorController_SampleZombie acz)
            acz.ApplyVisualTierFromRatio(ratio);
        else
            user.animatorController.SetVisualTierPublic(AnimatorController_SampleZombie.TierFromDamagePhaseRatio(ratio));
    }

    public override void BrokenArmor()
    {
        if (_broken) return;
        _broken = true;
        StopArmorBleedTimer();

        if (user != null)
            user.propertyController.onSetDamage.RemoveListener(GetDamage);

        if (carBreakEffect != null && user != null)
        {
            GameObject fx = ObjectPool.instance.Create(carBreakEffect);
            fx.transform.SetParent(user.transform, false);
            fx.transform.localPosition = Vector3.zero;
        }

        base.BrokenArmor();

        if (user != null && !user.IfDeath)
            user.stateController.ChangeState(StateName.DeathState);
    }

    /// <summary>
    /// 先处理子弹（二类 + 非 AOE 回收），再处理碾压。
    /// </summary>
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (_broken || user == null || user.IfDeath) return;
        if (collision.CompareTag(user.tag)) return;

        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null && bullet.Dm != null)
        {
            ElementType elementSave = bullet.Dm.damageElementType;
            GetDamage(bullet.Dm);
            if ((elementSave & ElementType.AOE) == 0)
                bullet.RecycleBullet();
            return;
        }

        Chess target = collision.GetComponent<Chess>();
        if (target == null) return;

        DamageMessege mes = BuildCrushMessage(target);
        user.propertyController.TakeDamage(mes);
        onHit?.Invoke();
        player?.RandomPlay();
    }

    DamageMessege BuildCrushMessage(Chess target)
    {
        var mes = new DamageMessege();
        if (dm != null)
        {
            mes.damageType = dm.damageType;
            mes.damageElementType = dm.damageElementType;
        }
        mes.damageFrom = user;
        mes.damageTo = target;
        mes.damage = user.propertyController.GetAttack();
        mes.takeBuff = null;
        return mes;
    }
}
