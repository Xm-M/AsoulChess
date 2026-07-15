using System.Collections.Generic;
using UnityEngine;

/// <summary>老仓育常驻公式弹：无限命中次数，按目标独立触碰 CD。</summary>
public class Bullet_OkuwakiOrbit : Bullet
{
    [Min(0.05f)]
    public float touchCooldown = 0.3f;

    readonly Dictionary<Chess, float> _nextHitTime = new Dictionary<Chess, float>();

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (shooter == null || shooter.IfDeath)
        {
            RecycleBullet();
            return;
        }

        if (CompareTag(collision.tag))
            return;

        Chess c = collision.GetComponent<Chess>();
        if (c == null || c.IfDeath)
            return;

        if (Time.time < GetNextHitTime(c))
            return;

        Dm.damage = damage * rate;
        Dm.damageTo = c;
        shooter.propertyController.TakeDamage(Dm);
        WhenBulletHit?.Invoke(this);
        effect?.OnBulletHit(this);
        _nextHitTime[c] = Time.time + touchCooldown;
    }

    float GetNextHitTime(Chess c)
    {
        return _nextHitTime.TryGetValue(c, out float t) ? t : 0f;
    }

    public override void RecycleBullet()
    {
        _nextHitTime.Clear();
        base.RecycleBullet();
    }
}
