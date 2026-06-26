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

    public virtual void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
         
        if (user == null || MapManage.instance == null) return;

        MapManage map = MapManage.instance;
        if (!GridFindTargetGeometry.TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return;

        int forwardX = GridFindTargetGeometry.GetForwardX(user);
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
            if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, map.mapSize))
                continue;

            Tile tile = map.tiles[ax, ay];
            if (tile == null)
                continue;

            Vector2 center = GridFindTargetGeometry.GetCellOverlapCenter(tile, ts);
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

#if UNITY_EDITOR
    /// <summary>
    /// Scene 视图画出与 <see cref="FindTarget"/> 一致的每格 <see cref="Physics2D.OverlapBoxNonAlloc"/> 范围（半宽为 <see cref="boxHalfExtents"/>）。
    /// 由 <see cref="Chess.OnDrawGizmos"/> 调用；线框仅在 Scene 视图显示。
    /// </summary>
    public virtual void DrawGizmos(Chess user)
    {
        if (user == null) return;

        MapManage map = MapManage.instance;
        if (map == null && !Application.isPlaying)
            map =UnityEngine.Object.FindObjectOfType<MapManage>();
        if (map == null) return;

        if (!GridFindTargetGeometry.TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return;

        int forwardX = GridFindTargetGeometry.GetForwardX(user);
        Vector2 ts = map.tileSize;

        if (relativeCells == null)
            return;

        Color prev = Gizmos.color;
        Gizmos.color = new Color(0f, 0.85f, 1f, 0.95f);

        foreach (Vector2Int rel in relativeCells)
        {
            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, map.mapSize))
                continue;

            Tile tile = map.tiles[ax, ay];
            if (tile == null)
                continue;

            Vector2 c = GridFindTargetGeometry.GetCellOverlapCenter(tile, ts);
            Vector3 center = new Vector3(c.x, c.y, 0f);
            Vector3 size = new Vector3(boxHalfExtents.x * 2f, boxHalfExtents.y * 2f, 0.05f);
            Gizmos.DrawWireCube(center, size);
        }

        Gizmos.color = prev;
    }
#endif
}
