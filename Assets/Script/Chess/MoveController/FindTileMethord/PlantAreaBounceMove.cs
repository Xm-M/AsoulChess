using System;
using UnityEngine;

/// <summary>
/// 在「可种植」矩形世界范围内做匀速直线运动；撞边时以镜面反射为基底并叠加随机转角（非纯反射）。
/// <see cref="FindNextTile"/> 返回 null，由本类在 <see cref="WhenMoving"/> 内直接推进 <see cref="Transform.position"/>。
/// 每帧按世界坐标将 <see cref="MoveController.standTile"/> 设为最近格（下标钳在 <c>0..mapSize.x-2</c>、<c>0..mapSize.y-1</c>），不调用 <see cref="Tile.ChessEnter"/>（避免把棋子瞬移到格点）。
/// 边界矩形由 <c>tiles[0,0]</c> 与末角格 transform 位置及 <see cref="MapManage.tileSize"/> 推导。
/// 每帧按速度水平分量 <see cref="Chess.UpdateFacingFromHorizontalMove"/> 同步左右朝向；对 X 取反以与本棋子美术默认朝向（与 <see cref="MoveController.WhenMoving"/> 里 Player 分支一致）对齐。
/// </summary>
[Serializable]
public class PlantAreaBounceMove : FindTileMethod
{
    [Tooltip("撞边后在反射方向上额外旋转的最大角度（度），越大越「乱弹」。")]
    [SerializeField]
    float bounceJitterDegrees = 28f;

    [Tooltip("开局随机方向时，若 |sin| 与 |cos| 都大于该值则更易呈斜向；多抽几次仍失败则用最后一次结果。")]
    [SerializeField]
    float preferDiagonalMinAbs = 0.18f;

    [Tooltip("贴边向内钳制，减轻浮点卡在边界外。")]
    [SerializeField]
    float boundaryEpsilon = 0.02f;

    [SerializeField]
    int diagonalSampleAttempts = 12;

    Vector2 _dir = Vector2.right;

    public override void StartMoving(Chess c)
    {
        _dir = SampleInitialDirection();
        if (_dir.sqrMagnitude < 0.0001f)
            _dir = Vector2.right;
        else
            _dir.Normalize();

        SyncFacingToMoveDirection(c);
    }

    public override Tile FindNextTile(Chess c) => null;

    public override void WhenMoving(Chess c)
    {
        MapManage map = MapManage.instance;
        if (map == null || map.tiles == null || c == null || c.moveController == null)
            return;

        if (!TryGetPlantableWorldBounds(map, boundaryEpsilon, out Vector2 min, out Vector2 max))
            return;

        float spd = c.propertyController != null ? c.propertyController.GetMoveSpeed() : 0f;
        if (spd <= 0f)
            return;

        Vector2 p = c.transform.position;
        p += _dir * (spd * Time.deltaTime);

        for (int iter = 0; iter < 4; iter++)
        {
            bool hit = false;
            if (p.x < min.x)
            {
                p.x = min.x;
                ApplyBounce(ref _dir, Vector2.right);
                hit = true;
            }
            else if (p.x > max.x)
            {
                p.x = max.x;
                ApplyBounce(ref _dir, -Vector2.right);
                hit = true;
            }

            if (p.y < min.y)
            {
                p.y = min.y;
                ApplyBounce(ref _dir, Vector2.up);
                hit = true;
            }
            else if (p.y > max.y)
            {
                p.y = max.y;
                ApplyBounce(ref _dir, -Vector2.up);
                hit = true;
            }

            if (!hit)
                break;
        }

        c.transform.position = new Vector3(p.x, p.y, c.transform.position.z);
        SyncNearestStandTile(c, map);
        SyncFacingToMoveDirection(c);
    }

    void SyncFacingToMoveDirection(Chess c)
    {
        if (c == null) return;
        // 与「速度向右应朝右」直觉一致：若左右反了，多半是精灵默认朝 -X，这里对水平分量取反再交给既有翻面逻辑
        c.UpdateFacingFromHorizontalMove(new Vector2(-_dir.x, _dir.y));
    }

    static bool TryGetPlantableWorldBounds(MapManage map, float eps, out Vector2 min, out Vector2 max)
    {
        min = default;
        max = default;
        Vector2 ts = map.tileSize;
        if (ts.x <= 0f || ts.y <= 0f)
            return false;
        Vector2Int sz = map.mapSize;
        if (sz.x < 2 || sz.y < 1)
            return false;

        Tile t00 = map.tiles[0, 0];
        Tile tBr = map.tiles[sz.x - 3, sz.y - 2];
        if (t00 == null || tBr == null)
            return false;

        Vector2 bl00 = t00.transform.position;
        Vector2 blBr = tBr.transform.position;
        // 左下角取 (0,0) 格原点；右上角取末格 (sz.x-2, sz.y-1) 的右上内侧一点，便于碰撞反弹
        min = bl00 + new Vector2(eps, eps);
        max = blBr + new Vector2(ts.x - eps, ts.y - eps);
        return min.x < max.x && min.y < max.y;
    }

    void SyncNearestStandTile(Chess c, MapManage map)
    {
        Vector2 ts = map.tileSize;
        Vector2 p = c.transform.position;
        int ix = Mathf.FloorToInt(p.x / ts.x);
        int iy = Mathf.FloorToInt(p.y / ts.y);
        int maxX = Mathf.Max(0, map.mapSize.x - 2);
        int maxY = Mathf.Max(0, map.mapSize.y - 1);
        ix = Mathf.Clamp(ix, 0, maxX);
        iy = Mathf.Clamp(iy, 0, maxY);

        Tile t = map.tiles[ix, iy];
        if (t != null && c.moveController.standTile != t)
            c.moveController.standTile = t;
    }

    Vector2 SampleInitialDirection()
    {
        for (int i = 0; i < diagonalSampleAttempts; i++)
        {
            Vector2 d = UnityEngine.Random.insideUnitCircle;
            if (d.sqrMagnitude < 0.0001f)
                continue;
            d.Normalize();
            if (Mathf.Abs(d.x) >= preferDiagonalMinAbs && Mathf.Abs(d.y) >= preferDiagonalMinAbs)
                return d;
        }
        Vector2 f = UnityEngine.Random.insideUnitCircle;
        return f.sqrMagnitude > 0.0001f ? f.normalized : new Vector2(0.71f, 0.71f);
    }

    void ApplyBounce(ref Vector2 dir, Vector2 normalIntoPlayable)
    {
        if (normalIntoPlayable.sqrMagnitude < 0.0001f)
            return;
        normalIntoPlayable.Normalize();

        dir = Vector2.Reflect(dir, normalIntoPlayable);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Rotate(normalIntoPlayable, UnityEngine.Random.Range(-89f, 89f) * Mathf.Deg2Rad);
        else
            dir.Normalize();

        float jitter = UnityEngine.Random.Range(-bounceJitterDegrees, bounceJitterDegrees) * Mathf.Deg2Rad;
        dir = Rotate(dir, jitter);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;
        else
            dir.Normalize();
    }

    static Vector2 Rotate(Vector2 v, float radians)
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
