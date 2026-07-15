using UnityEngine;

/// <summary>喂球弹：可命中同 tag 挥棒手，零伤害，触发站位连招。</summary>
public class Bullet_PitchBall : Bullet
{
    const float DeliverProximity = 0.55f;
    bool _delivered;

    protected override void Update()
    {
        base.Update();
        TryDeliverOnProximity();
    }

    void TryDeliverOnProximity()
    {
        if (_delivered || current <= 0 || initTarget == null || shooter == null)
            return;
        if (!BaseballRelayHelper.IsBatter(initTarget))
            return;
        if (!BaseballRelayHelper.IsSameRelayRow(shooter, initTarget))
            return;

        float dist = Vector2.Distance(transform.position, initTarget.transform.position);
        if (dist > DeliverProximity)
            return;

        DeliverToBatter(initTarget);
    }

    void DeliverToBatter(Chess batter)
    {
        if (_delivered || batter == null || shooter == null)
            return;

        _delivered = true;
        current = 0;
        BaseballRelayHelper.DeliverPitch(batter, shooter);
        RecycleBullet();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (CompareTag(collision.tag))
        {
            Chess c = collision.GetComponentInParent<Chess>();
            if (c != null && current > 0 && c == initTarget && BaseballRelayHelper.IsBatter(c)
                && BaseballRelayHelper.IsSameRelayRow(shooter, c))
                DeliverToBatter(c);
            else if (current == 0)
                RecycleBullet();
            return;
        }

        base.OnTriggerEnter2D(collision);
    }

    public override void InitBullet(Chess shooter, Vector3 position, Chess target, Vector2 moveDir, float damage = -1, float rate = 1)
    {
        _delivered = false;
        base.InitBullet(shooter, position, target, moveDir, 0f, rate);
        Dm.damageType = DamageType.Miss;
        Dm.damage = 0f;
    }
}
