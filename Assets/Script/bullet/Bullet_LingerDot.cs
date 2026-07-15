using UnityEngine;

/// <summary>
/// 压力希驻留弹：直线飞至命中 → 立即造成一次伤害并停下 →
/// 每秒对当前锁定目标造成 ATK×rate 伤害；目标无效时等待下一碰撞敌人接锁；
/// <see cref="lingerDuration"/> 秒后回收。
/// </summary>
public class Bullet_LingerDot : Bullet
{
    [Min(0.1f)]
    [Tooltip("驻留总时长（含命中当刻起算）")]
    public float lingerDuration = 3f;

    [Min(0.05f)]
    [Tooltip("跳伤间隔")]
    public float tickInterval = 1f;

    bool _lingering;
    Chess _lockTarget;
    float _lingerEndTime;
    float _nextTickTime;
    IBulletMove _flightMove;

    public override void InitBullet(Chess shooter, Vector3 position, Chess target, Vector2 moveDir, float damage = -1, float rate = 1)
    {
        StopLingerState();
        _lingering = false;
        _lockTarget = null;
        _flightMove = bulletMove;
        base.InitBullet(shooter, position, target, moveDir, damage, rate);
        // 飞行阶段可多次探测，驻留后由逻辑接管，不再因 current==0 立刻回收
        current = Mathf.Max(1, MaxHitNum);
    }

    protected override void Update()
    {
        if (_lingering)
        {
            TickLinger();
            return;
        }

        if (bulletMove != null)
            bulletMove.MoveBullet(this);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (shooter == null) return;
        if (CompareTag(collision.tag)) return;

        Chess c = collision.GetComponent<Chess>();
        if (c == null)
            c = collision.GetComponentInParent<Chess>();
        if (c == null || c.IfDeath) return;

        if (!_lingering)
        {
            EnterLinger(c);
            return;
        }

        // 已驻留且当前锁失效：碰到新敌人则接锁并立即跳一次
        if (!IsLockValid())
        {
            _lockTarget = c;
            ApplyTickDamage(c);
            _nextTickTime = Time.time + tickInterval;
        }
    }

    void EnterLinger(Chess firstHit)
    {
        _lingering = true;
        _lockTarget = firstHit;
        // 停移：不再调用飞行弹道
        bulletMove = null;

        ApplyTickDamage(firstHit);
        WhenBulletHit?.Invoke(this);
        effect?.OnBulletHit(this);

        float now = Time.time;
        _lingerEndTime = now + Mathf.Max(0.1f, lingerDuration);
        _nextTickTime = now + tickInterval;
        current = 0; // 禁用基类「命中次数用尽回收」路径
    }

    void TickLinger()
    {
        float now = Time.time;
        if (now >= _lingerEndTime)
        {
            RecycleBullet();
            return;
        }

        if (!IsLockValid())
            _lockTarget = null;

        if (_lockTarget != null && now >= _nextTickTime)
        {
            ApplyTickDamage(_lockTarget);
            _nextTickTime = now + tickInterval;
            if (!IsLockValid())
                _lockTarget = null;
        }
    }

    bool IsLockValid()
    {
        return _lockTarget != null && !_lockTarget.IfDeath && _lockTarget.gameObject != null && _lockTarget.gameObject.activeInHierarchy;
    }

    void ApplyTickDamage(Chess target)
    {
        if (shooter == null || target == null || target.IfDeath) return;

        float atk = shooter.propertyController != null
            ? shooter.propertyController.GetAttack() * rate
            : damage;
        Dm.damageFrom = shooter;
        Dm.damageTo = target;
        Dm.damage = atk;
        shooter.propertyController.TakeDamage(Dm);
    }

    public override void RecycleBullet()
    {
        StopLingerState();
        base.RecycleBullet();
    }

    void StopLingerState()
    {
        _lingering = false;
        _lockTarget = null;
        if (_flightMove != null)
            bulletMove = _flightMove;
    }
}
