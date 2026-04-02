using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 以自身所在格为原点，对配置的相对格子逐格做 <see cref="Physics2D.OverlapBoxNonAlloc"/> 检测敌方层。
/// X 随 <see cref="Chess.transform.right"/> 镜像；Y 不翻转。可检测 X 最大为 <c>mapSize.x - 2</c>（最右一列为出生格，不检测）。
/// </summary>
[Serializable]
public class IGridFindTarget : IFindTarget
{
    const int ColPoolSize = 1000;

    [Tooltip("相对自身 mapPos 的偏移；(1,0) 为面朝方向的正前方一格")]
    public List<Vector2Int> relativeCells = new List<Vector2Int>();

    [Tooltip("单格检测盒半尺寸（世界单位）。每格在 Tile 中心做 OverlapBox；约为 tileSize 的一半时刚好一格，略调大可略超出单格（易扫到贴边敌人）；过小易漏判。")]
    public Vector2 boxHalfExtents = new Vector2(1.25f, 1.25f);

    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
         
        if (user == null || MapManage.instance == null) return;

        MapManage map = MapManage.instance;
        if (!TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return;

        int forwardX = GetForwardX(user);
        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(ColPoolSize);
        Vector2 ts = map.tileSize;

        if (relativeCells == null)
        {
            CheckObjectPoolManage.ReleaseColArray(ColPoolSize, cols);
            return;
        }

        foreach (Vector2Int rel in relativeCells)
        {
            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (!IsDetectableCell(ax, ay, map.mapSize))
                continue;

            Tile tile = map.tiles[ax, ay];
            if (tile == null)
                continue;

            Vector2 center = GetCellOverlapCenter(tile, ts);
            int count = Physics2D.OverlapBoxNonAlloc(center, boxHalfExtents, 0f, cols, enemyLayer);
            for (int i = 0; i < count; i++)
            {
                if (cols[i] == null)
                    continue;
                Chess c = cols[i].GetComponent<Chess>();
                if (c == null)
                    continue;
                if (!targets.Contains(c))
                    targets.Add(c);
            }
        }

        CheckObjectPoolManage.ReleaseColArray(ColPoolSize, cols);
    }

    static int GetForwardX(Chess user)
    {
        float rx = user.transform.right.x;
        if (Mathf.Approximately(rx, 0f))
            return 1;
        return rx > 0f ? 1 : -1;
    }

    /// <summary>
    /// 有 standTile 用其 mapPos；否则用世界坐标按 <see cref="MapManage.tileSize"/> 估格并钳到地图内。
    /// </summary>
    static bool TryGetBaseMapPos(Chess user, MapManage map, out Vector2Int outPos)
    {
        outPos = default;
        if (user.moveController != null && user.moveController.standTile != null)
        {
            outPos = user.moveController.standTile.mapPos;
            return true;
        }

        Vector2 p = user.transform.position;
        Vector2 ts = map.tileSize;
        if (ts.x <= 0f || ts.y <= 0f)
            return false;

        int ix = Mathf.FloorToInt(p.x / ts.x);
        int iy = Mathf.FloorToInt(p.y / ts.y);
        ix = Mathf.Clamp(ix, 0, Mathf.Max(0, map.mapSize.x - 1));
        iy = Mathf.Clamp(iy, 0, Mathf.Max(0, map.mapSize.y - 1));
        outPos = new Vector2Int(ix, iy);
        return true;
    }

    /// <summary>Y 全高有效；X 排除最右一列（下标 mapSize.x - 1）。</summary>
    static bool IsDetectableCell(int x, int y, Vector2Int mapSize)
    {
        if (mapSize.y <= 0)
            return false;
        if (y < 0 || y >= mapSize.y)
            return false;
        if (mapSize.x < 2)
            return false;
        if (x < 0 || x > mapSize.x - 2)
            return false;
        return true;
    }

    /// <summary>
    /// 单格 OverlapBox 的世界中心。
    /// 优先用草地 <see cref="SpriteRenderer"/> / <see cref="Collider2D"/> 的 <c>bounds.center</c>，
    /// 与场景里格子、冰块（<see cref="Effect_Snow.PlaceOrRefreshIce"/> 用的 <see cref="Tile.transform"/> 同一块地）对齐；
    /// 若 Tile 轴心已在格中心，再用 <c>position + tileSize*0.5</c> 会整体偏右上约半格。
    /// 无渲染体/碰撞体时回退：<see cref="AutoInitMap"/> 左下角锚点 + 半格。
    /// </summary>
    static Vector2 GetCellOverlapCenter(Tile tile, Vector2 tileSize)
    {
        if (tile == null) return default;

        var sr = tile.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null)
            return sr.bounds.center;

        var col = tile.GetComponentInChildren<Collider2D>(true);
        if (col != null)
            return col.bounds.center;

        return (Vector2)tile.transform.position + new Vector2(tileSize.x * 0.5f, tileSize.y * 0.5f);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Scene 视图画出与 <see cref="FindTarget"/> 一致的每格 <see cref="Physics2D.OverlapBoxNonAlloc"/> 范围（半宽为 <see cref="boxHalfExtents"/>）。
    /// 由 <see cref="Chess.OnDrawGizmos"/> 调用；线框仅在 Scene 视图显示。
    /// </summary>
    public void DrawGizmos(Chess user)
    {
        if (user == null) return;

        MapManage map = MapManage.instance;
        if (map == null && !Application.isPlaying)
            map =UnityEngine.Object.FindObjectOfType<MapManage>();
        if (map == null) return;

        if (!TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return;

        int forwardX = GetForwardX(user);
        Vector2 ts = map.tileSize;

        if (relativeCells == null)
            return;

        Color prev = Gizmos.color;
        Gizmos.color = new Color(0f, 0.85f, 1f, 0.95f);

        foreach (Vector2Int rel in relativeCells)
        {
            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (!IsDetectableCell(ax, ay, map.mapSize))
                continue;

            Tile tile = map.tiles[ax, ay];
            if (tile == null)
                continue;

            Vector2 c = GetCellOverlapCenter(tile, ts);
            Vector3 center = new Vector3(c.x, c.y, 0f);
            Vector3 size = new Vector3(boxHalfExtents.x * 2f, boxHalfExtents.y * 2f, 0.05f);
            Gizmos.DrawWireCube(center, size);
        }

        Gizmos.color = prev;
    }
#endif
}
