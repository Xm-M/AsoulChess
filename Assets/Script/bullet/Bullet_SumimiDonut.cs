using UnityEngine;

/// <summary>
/// sumimi 羁绊甜甜圈：追踪 <see cref="initTarget"/> 友军，命中时不造成伤害，由 <see cref="IBulletEffect"/> 处理效果。
/// </summary>
public class Bullet_SumimiDonut : Bullet
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (initTarget == null || initTarget.IfDeath)
        {
            RecycleBullet();
            return;
        }

        Chess c = collision.GetComponent<Chess>();
        if (c == null || c != initTarget || c.IfDeath || !c.CompareTag(tag))
            return;

        if (Dm == null)
            Dm = new DamageMessege();
        Dm.damageFrom = shooter;
        Dm.damageTo = c;
        Dm.damage = 0;
        Dm.damageType = DamageType.Miss;
        WhenBulletHit?.Invoke(this);
        effect?.OnBulletHit(this);
        RecycleBullet();
    }
}
