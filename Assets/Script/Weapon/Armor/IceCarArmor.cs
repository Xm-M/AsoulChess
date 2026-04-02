using UnityEngine;

/// <summary>
/// 冰车防具：二类（溢出不进本体、Buff 不作用于本体）；非 AOE 子弹扣甲后回收；AOE 子弹不回收。
/// 碾压：使用 Inspector 中 <see cref="CarArmor.dm"/> 的 <see cref="DamageMessege.damageType"/> / <see cref="DamageMessege.damageElementType"/>，
/// 伤害数值为 <see cref="PropertyController.GetAttack"/>；对植物走 <see cref="PropertyController.TakeDamage"/>。
/// 车爆：<see cref="carBreakEffect"/> 可选，随后 <see cref="BrokenArmor"/> 并 <see cref="Chess.Death"/>。
/// </summary>
public class IceCarArmor : CarArmor
{
    public float armorMax = 300f;
    [Tooltip("车爆特效（可选）；在 BrokenArmor 时于僵尸位置生成")]
    public GameObject carBreakEffect;

    [SerializeField]
    float armorCurrent;

    bool _broken;

    public override void InitArmor()
    {
        armorCurrent = armorMax;
        user.WhenEnterGame.AddListener(ResetArmor);
    }

    public override void ResetArmor(Chess chess)
    {
        tag = user.tag;
        _broken = false;
        armorCurrent = armorMax;
        user.propertyController.onSetDamage.AddListener(GetDamage);
    }

    /// <summary>二类：Buff 不传递到本体；溢出不进本体；伤害先由车甲承担。</summary>
    public override void GetDamage(DamageMessege dm)
    {
        if (_broken || user == null || dm == null) return;

        dm.takeBuff = null;

        if (armorCurrent >= dm.damage)
        {
            dm.damage *= (1f - user.propertyController.GetExtraDefence());
            UIManage.GetView<DamagePanel>().ShowDamageMes(dm);
            armorCurrent -= dm.damage;

            if ((dm.damageElementType & ElementType.Explode) == 0)
                dm.damage = 0;
            else if (dm.damage > user.propertyController.GetHp())
                BrokenArmorFromDamage();

            player?.RandomPlay();

            if (!_broken && armorCurrent <= 0f)
            {
                if ((dm.damageElementType & ElementType.Explode) == 0)
                    dm.damage = 0;
                BrokenArmorFromDamage();
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
            BrokenArmorFromDamage();
        }
    }

    void BrokenArmorFromDamage()
    {
        if (_broken) return;
        BrokenArmor();
    }

    public override void BrokenArmor()
    {
        if (_broken) return;
        _broken = true;

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
            user.Death();
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
