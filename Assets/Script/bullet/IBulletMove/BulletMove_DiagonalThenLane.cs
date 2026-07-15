using UnityEngine;

/// <summary>
/// 三线射手式：先斜飞到目标行高度，再沿 <see cref="Bullet.transform.right"/> 直线前进。
/// 目标行 Y 取自 <see cref="Bullet.targetPos"/>.y（由攻击侧写入目标或行中心）。
/// </summary>
public class BulletMove_DiagonalThenLane : IBulletMove
{
    public float speed = 8f;
    [Tooltip("斜飞段：相对起点沿朝向前进的水平距离，再落到目标行 Y")]
    public float enterAhead = 0.6f;
    [Tooltip("已接近目标行 Y 时跳过斜飞")]
    public float sameLaneEpsilon = 0.08f;

    Vector2 _waypoint;
    bool _inLane;

    public void InitMove(Bullet bullet)
    {
        _inLane = false;
        if (bullet == null)
            return;

        Vector2 start = bullet.startPos;
        float laneY = bullet.targetPos.y;
        if (Mathf.Abs(start.y - laneY) <= sameLaneEpsilon)
        {
            _inLane = true;
            Vector3 p = bullet.transform.position;
            p.y = laneY;
            bullet.transform.position = p;
            return;
        }

        float ahead = enterAhead;
        float dirX = bullet.transform.right.x;
        if (Mathf.Approximately(dirX, 0f))
            dirX = 1f;
        ahead *= Mathf.Sign(dirX);
        _waypoint = new Vector2(start.x + ahead, laneY);
    }

    public void MoveBullet(Bullet bullet)
    {
        if (bullet == null)
            return;

        float step = speed * Time.deltaTime;
        if (!_inLane)
        {
            Vector2 pos = bullet.transform.position;
            Vector2 next = Vector2.MoveTowards(pos, _waypoint, step);
            bullet.transform.position = next;
            if ((next - _waypoint).sqrMagnitude <= 0.0001f)
            {
                _inLane = true;
                Vector3 p = bullet.transform.position;
                p.y = _waypoint.y;
                bullet.transform.position = p;
            }
            return;
        }

        Vector2 p2 = bullet.transform.position;
        bullet.transform.position = Vector2.MoveTowards(
            p2,
            p2 + (Vector2)bullet.transform.right,
            step);
    }
}
