using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多首攻击：本体从 weaponPos 直线射本行；每个分身从分身（或子物体 shootPos）发射，
/// 目标行按行序；跨行弹依赖子弹上的 <see cref="BulletMove_DiagonalThenLane"/>（targetPos.y = 行 Y）。
/// </summary>
public class MultiHeadShootAttack : IAttackFunction
{
    public GameObject bullet;
    [Tooltip("跨行专用弹（含 DiagonalThenLane）；为空则复用 bullet")]
    public GameObject crossRowBullet;

    const string ShootPosChildName = "shootPos";

    static readonly List<int> RowScratch = new List<int>(16);

    public void Attack(Chess user, List<Chess> targets)
    {
        if (user == null || bullet == null)
            return;
        if (MultiHeadMutsumiKeys.IsFinaleLocked(user))
            return;

        if (!MultiHeadMutsumiKeys.TryGetHomeRow(user, out int homeY))
            homeY = 0;

        Vector3 bodyMuzzle = user.equipWeapon != null && user.equipWeapon.weaponPos != null
            ? user.equipWeapon.weaponPos.position
            : user.transform.position;

        // 本体：只射本行（即使 FindTarget 因分身锁到了其他行）
        Chess homeTarget = MultiHeadRowFind.FindEnemyOnRow(user, homeY, bodyMuzzle);
        if (homeTarget != null)
            FireOne(user, bodyMuzzle, homeTarget, useCrossRowMove: false);

        int cloneCount = MultiHeadMutsumiKeys.GetCloneCount(user);
        if (cloneCount <= 0)
            return;

        List<Transform> heads = MultiHeadMutsumiKeys.GetOrCreateHeadList(user);
        MapManage map = MapManage.instance;
        int mapH = map != null ? map.mapSize.y : 1;

        for (int i = 0; i < cloneCount; i++)
        {
            Transform head = null;
            if (heads != null && i < heads.Count)
                head = heads[i];
            if (head == null)
                continue;

            Vector3 firePos = ResolveFirePos(head);
            int rowY = MultiHeadMutsumiKeys.GetAssignedRow(homeY, mapH, i, RowScratch);
            bool crossRow = rowY != homeY;

            Chess laneTarget = MultiHeadRowFind.FindEnemyOnRow(user, rowY, firePos);
            if (laneTarget == null)
                continue;

            if (!MultiHeadMutsumiKeys.TryGetRowWorldY(user, rowY, out float laneWorldY))
                laneWorldY = laneTarget.transform.position.y;

            FireFromHead(user, firePos, laneTarget, laneWorldY, crossRow);
        }
    }

    /// <summary>优先分身子物体 shootPos（含深层），否则分身自身位置。</summary>
    public static Vector3 ResolveFirePos(Transform head)
    {
        if (head == null)
            return Vector3.zero;
        Transform shoot = FindChildRecursive(head, ShootPosChildName);
        return shoot != null ? shoot.position : head.position;
    }

    static Transform FindChildRecursive(Transform root, string name)
    {
        Transform direct = root.Find(name);
        if (direct != null)
            return direct;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursive(root.GetChild(i), name);
            if (found != null)
                return found;
        }
        return null;
    }

    void FireOne(Chess user, Vector3 start, Chess target, bool useCrossRowMove)
    {
        GameObject prefab = useCrossRowMove && crossRowBullet != null ? crossRowBullet : bullet;
        if (prefab == null || ObjectPool.instance == null)
            return;
        GameObject go = ObjectPool.instance.Create(prefab);
        if (go == null)
            return;
        Bullet b = go.GetComponent<Bullet>();
        if (b == null)
            return;
        b.InitBullet(user, start, target, user.transform.right);
        if (target != null)
            b.Dm.damageTo = target;
    }

    void FireFromHead(Chess user, Vector3 firePos, Chess target, float laneWorldY, bool crossRow)
    {
        GameObject prefab = crossRow && crossRowBullet != null ? crossRowBullet : bullet;
        if (prefab == null || ObjectPool.instance == null)
            return;
        GameObject go = ObjectPool.instance.Create(prefab);
        if (go == null)
            return;
        Bullet b = go.GetComponent<Bullet>();
        if (b == null)
            return;

        b.InitBullet(user, firePos, target, user.transform.right);
        if (crossRow)
        {
            Vector2 tp = b.targetPos;
            tp.y = laneWorldY;
            b.targetPos = tp;
            b.bulletMove?.InitMove(b);
        }
        if (target != null)
            b.Dm.damageTo = target;
    }
}
