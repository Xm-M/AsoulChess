using System;
using UnityEngine;

/// <summary>
/// 集束移动：依赖发射者武器上的 <see cref="IGridFindTarget"/> 与格子几何；
/// 若 <see cref="Bullet.initTarget"/> 非空，集束终点为目标的实时位置，否则为攻击矩形「远沿」上与弹道槽位对应的格中心。
/// 到达集束点后不回收，沿到达瞬间的速度方向以 <see cref="moveSpeed"/> 继续匀速前进。
/// 集束阶段为「初速度 + <see cref="steerForce"/> 拐向集束点」；近沿宽边用世界 (平均x, [minY,maxY] 随机 y)，初向为发射点减该点。
/// 索敌半径内唯一一次锁定：瞬间将方向设为「子弹位置→目标位置」（目标 − 子弹）的单位向量，之后以 <see cref="moveSpeed"/>×<see cref="lockMoveSpeedMultiplier"/> 沿该方向直线飞行，不再读取目标。
/// </summary>
[Serializable]
public class IBulletMove_Bundling : IBulletMove
{
    const int OverlapPoolSize = 256;

    public float moveSpeed = 8f;

    [Tooltip("拐向力：每帧在速度方向上叠加 normalize(desiredDir−currentDir)×steerForce×dt，再归一化")]
    public float steerForce = 6f;

    [Tooltip("集束/滑行阶段若在此距离内发现敌人可触发唯一一次锁定直线")]
    public float detectRadius = 2f;

    [Tooltip("锁定后直线段的移速相对 moveSpeed 的倍率")]
    public float lockMoveSpeedMultiplier = 2f;

    [Tooltip("与集束终点距离小于等于此值时视为到达，转入匀速滑行阶段")]
    public float arriveAtConvergeDistance = 0.18f;

    [Tooltip("弹道槽位；-1 为自动")]
    public int laneSlot = -1;

    [Tooltip("为 true 时：宽边世界 x=近沿各格心 x 平均，y∈[minY,maxY] 随机，初向 normalize(发射点−(x,y))；为 false 时朝向集束终点")]
    public bool initialFaceTowardNearWidth = true;

    enum Phase
    {
        Bundling,
        Coasting,
        /// <summary>锁定瞬间已定方向，与目标无关。</summary>
        LockStraight
    }

    Phase _phase;
    Vector2 _farEndpoint;
    Vector2 _coastDirection;
    bool _lockConsumed;
    Vector2 _lockStraightDir;

    bool _hasGrid;
    Vector2 _nearW;
    Vector2 _gridFarW;
    bool _convergeFollowsTarget;

    Vector2 _bundlingVelocity;

    public void InitMove(Bullet bullet)
    {
        _phase = Phase.Bundling;
        _lockConsumed = false;
        _hasGrid = false;
        _convergeFollowsTarget = false;
        _nearW = _gridFarW = default;

        Chess shooter = bullet.shooter;
        Vector2 start = bullet.startPos;
        _farEndpoint = bullet.targetPos;

        bool hasLivingTarget = bullet.initTarget != null && !bullet.initTarget.IfDeath;

        IGridFindTarget grid = null;
        if (shooter != null && shooter.equipWeapon != null && shooter.equipWeapon.weapon is Weapon_Sample ws)
            grid = ws.findTarget as IGridFindTarget;

        if (grid != null && grid.relativeCells != null && grid.relativeCells.Count > 0 &&
            GridFindTargetGeometry.TryGetBundlingLaneEndpoints(
                shooter, grid.relativeCells, ResolveLaneIndex(bullet, grid), out Vector2 nearW, out Vector2 farW, out _))
        {
            _hasGrid = true;
            _nearW = nearW;
            _gridFarW = farW;
        }

        if (hasLivingTarget)
        {
            _convergeFollowsTarget = true;
            _farEndpoint = bullet.initTarget.transform.position;
        }
        else if (_hasGrid)
        {
            _farEndpoint = _gridFarW;
        }
        else
        {
            AimFallback(bullet, start);
            SyncBundlingVelocityFromTransform(bullet);
            return;
        }

        if (_hasGrid)
        {
            if (initialFaceTowardNearWidth)
            {
                if (GridFindTargetGeometry.TryGetRandomPointOnNearWidth(shooter, grid.relativeCells, out Vector2 pw))
                {
                    Vector2 d = start - pw;
                    if (d.sqrMagnitude > 1e-6f)
                        bullet.transform.right = d.normalized;
                }
                else
                {
                    Vector2 d = _nearW - start;
                    if (d.sqrMagnitude > 1e-6f)
                        bullet.transform.right = d.normalized;
                }
            }
            else
            {
                Vector2 d = _farEndpoint - start;
                if (d.sqrMagnitude > 1e-6f)
                    bullet.transform.right = d.normalized;
            }
        }
        else if (hasLivingTarget)
        {
            Vector2 d = _farEndpoint - start;
            if (d.sqrMagnitude > 1e-6f)
                bullet.transform.right = d.normalized;
        }

        SyncBundlingVelocityFromTransform(bullet);
    }

    void SyncBundlingVelocityFromTransform(Bullet bullet)
    {
        Vector2 dir = (Vector2)bullet.transform.right;
        if (dir.sqrMagnitude < 1e-6f)
            dir = Vector2.right;
        _bundlingVelocity = dir.normalized * moveSpeed;
    }

    int ResolveLaneIndex(Bullet bullet, IGridFindTarget grid)
    {
        if (bullet == null || grid.relativeCells == null)
            return 0;

        int minX = int.MaxValue, maxX = int.MinValue;
        for (int i = 0; i < grid.relativeCells.Count; i++)
        {
            Vector2Int r = grid.relativeCells[i];
            if (r.x < minX) minX = r.x;
            if (r.x > maxX) maxX = r.x;
        }

        int laneCount = 1;
        if (minX < maxX)
        {
            var nearY = new System.Collections.Generic.HashSet<int>();
            var farY = new System.Collections.Generic.HashSet<int>();
            foreach (Vector2Int rel in grid.relativeCells)
            {
                if (rel.x == minX) nearY.Add(rel.y);
                if (rel.x == maxX) farY.Add(rel.y);
            }
            int common = 0;
            foreach (int y in nearY)
            {
                if (farY.Contains(y))
                    common++;
            }
            if (common > 0)
                laneCount = common;
        }

        if (laneCount < 1)
            laneCount = 1;

        if (laneSlot >= 0)
            return laneSlot % laneCount;

        return Mathf.Abs(bullet.GetInstanceID()) % laneCount;
    }

    void AimFallback(Bullet bullet, Vector2 start)
    {
        Vector2 d = (Vector2)bullet.targetPos - start;
        if (d.sqrMagnitude > 1e-6f)
            bullet.transform.right = d.normalized;
    }

    void UpdateBundlingGoal(Bullet bullet)
    {
        if (!_convergeFollowsTarget)
            return;

        if (bullet.initTarget == null || bullet.initTarget.IfDeath)
        {
            _convergeFollowsTarget = false;
            if (_hasGrid)
                _farEndpoint = _gridFarW;
            return;
        }

        _farEndpoint = bullet.initTarget.transform.position;
    }

    public void MoveBullet(Bullet bullet)
    {
        if (_phase == Phase.LockStraight)
        {
            StepLockStraight(bullet);
            return;
        }

        TryEnterLockStraight(bullet);
        if (_phase == Phase.LockStraight)
        {
            StepLockStraight(bullet);
            return;
        }

        if (_phase == Phase.Coasting)
        {
            bullet.transform.position += (Vector3)(_coastDirection * moveSpeed * Time.deltaTime);
            bullet.transform.right = (Vector3)_coastDirection;
            return;
        }

        UpdateBundlingGoal(bullet);
        Vector2 goal = _farEndpoint;
        float dist = Vector2.Distance(bullet.transform.position, goal);
        if (dist <= arriveAtConvergeDistance)
        {
            _coastDirection = _bundlingVelocity.sqrMagnitude > 1e-6f
                ? _bundlingVelocity.normalized
                : (Vector2)bullet.transform.right;
            if (_coastDirection.sqrMagnitude < 1e-6f)
            {
                Vector2 dr = goal - (Vector2)bullet.transform.position;
                _coastDirection = dr.sqrMagnitude > 1e-6f ? dr.normalized : Vector2.right;
            }
            bullet.transform.right = (Vector3)_coastDirection;
            _phase = Phase.Coasting;
            return;
        }

        StepBundlingSteer(bullet, goal);
    }

    void StepLockStraight(Bullet bullet)
    {
        float sp = moveSpeed * Mathf.Max(0.01f, lockMoveSpeedMultiplier);
        bullet.transform.position += (Vector3)(_lockStraightDir * sp * Time.deltaTime);
        bullet.transform.right = new Vector3(_lockStraightDir.x, _lockStraightDir.y, 0f);
    }

    void StepBundlingSteer(Bullet bullet, Vector2 goal)
    {
        Vector2 pos = bullet.transform.position;
        Vector2 toGoal = goal - pos;
        Vector2 desiredDir = toGoal.sqrMagnitude > 1e-6f ? toGoal.normalized : _bundlingVelocity.normalized;

        Vector2 currentDir = _bundlingVelocity.sqrMagnitude > 1e-6f ? _bundlingVelocity.normalized : desiredDir;

        Vector2 delta = desiredDir - currentDir;
        float dMag = delta.magnitude;
        Vector2 newDir;
        if (dMag < 1e-6f)
            newDir = desiredDir;
        else
        {
            Vector2 step = (delta / dMag) * (steerForce * Time.deltaTime);
            if (step.sqrMagnitude > dMag * dMag)
                step = delta;
            Vector2 combined = currentDir + step;
            newDir = combined.sqrMagnitude > 1e-8f ? combined.normalized : desiredDir;
        }

        _bundlingVelocity = newDir * moveSpeed;
        bullet.transform.position = pos + _bundlingVelocity * Time.deltaTime;
        bullet.transform.right = new Vector3(newDir.x, newDir.y, 0f);
    }

    /// <summary>锁定：方向 = normalize(目标位置 − 子弹位置)，即沿锁定瞬间从子弹指向目标；之后不再读目标。</summary>
    void TryEnterLockStraight(Bullet bullet)
    {
        if (_lockConsumed)
            return;
        if (_phase != Phase.Bundling && _phase != Phase.Coasting)
            return;

        Chess shooter = bullet.shooter;
        if (shooter == null)
            return;

        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(shooter.gameObject);
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(OverlapPoolSize);
        int n = Physics2D.OverlapCircleNonAlloc(bullet.transform.position, detectRadius, cols, enemyLayer);

        Vector2 p = bullet.transform.position;
        float bestD2 = detectRadius * detectRadius + 1f;
        Chess best = null;

        for (int i = 0; i < n; i++)
        {
            if (cols[i] == null)
                continue;
            Chess c = cols[i].GetComponent<Chess>();
            if (c == null || c.IfDeath)
                continue;
            float d2 = ((Vector2)c.transform.position - p).sqrMagnitude;
            if (d2 < bestD2)
            {
                bestD2 = d2;
                best = c;
            }
        }

        CheckObjectPoolManage.ReleaseColArray(OverlapPoolSize, cols);

        if (best == null)
            return;

        _lockConsumed = true;
        Vector2 towardTarget = (Vector2)best.transform.position - p;
        if (towardTarget.sqrMagnitude < 1e-8f)
            towardTarget = (Vector2)bullet.transform.right;
        _lockStraightDir = towardTarget.normalized;

        _phase = Phase.LockStraight;
        bullet.transform.right = new Vector3(_lockStraightDir.x, _lockStraightDir.y, 0f);
        bullet.Dm.damageTo = best;
    }
}
