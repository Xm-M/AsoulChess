using UnityEngine;

/// <summary>
/// 治疗弹：对「仅自己」生效。普通 Bullet 只打异 tag，无法命中同队；
/// 此类在 Init 后立刻对 shooter 结算一次 Heal，并回收。
/// </summary>
public class Bullet_HealSelf : Bullet
{
    public override void InitBullet(Chess shooter, Vector3 position, Chess target, Vector2 moveDir, float damage = -1, float rate = 1)
    {
        if (Dm == null) Dm = new DamageMessege();
        Dm.damageType = DamageType.Heal;
        Dm.damageElementType = ElementType.Bullet;
        base.InitBullet(shooter, position, target, moveDir, damage, rate);
        if (target != null && target == shooter)
            ApplyHealAndRecycle();
    }

    void ApplyHealAndRecycle()
    {
        Dm.damage = damage * rate;
        Dm.damageTo = shooter;
        shooter.propertyController.TakeDamage(Dm);
        WhenBulletHit?.Invoke(this);
        effect?.OnBulletHit(this);
        RecycleBullet();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 即时治疗路径已在 InitBullet 处理；若未回收则不打异 tag，避免误伤。
    }
}
