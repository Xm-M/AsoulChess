using System;
using UnityEngine;

/// <summary>同排内沿 X 轴逐格靠近目标 mapPos（供初音小推车等定点移动）。</summary>
[Serializable]
public class FindTileMethod_SeekMapPos : FindTileMethod
{
    public Vector2Int targetMapPos;

    public override Tile FindNextTile(Chess c)
    {
        if (c?.moveController?.standTile == null || MapManage.instance == null)
            return null;

        Vector2Int cur = c.moveController.standTile.mapPos;
        if (cur == targetMapPos)
            return null;
        if (cur.y != targetMapPos.y)
            return null;

        int dx = targetMapPos.x > cur.x ? 1 : -1;
        int nx = cur.x + dx;
        var map = MapManage.instance;
        if (nx < 0 || nx >= map.mapSize.x)
            return null;

        SyncFacingTowardTarget(c, dx);
        return map.tiles[nx, cur.y];
    }

    public override void StartMoving(Chess c) => SyncFacingTowardTarget(c);

    void SyncFacingTowardTarget(Chess c, int dxHint = 0)
    {
        if (c?.moveController?.standTile == null)
            return;

        Vector2Int cur = c.moveController.standTile.mapPos;
        if (cur.y != targetMapPos.y || cur == targetMapPos)
            return;

        int dx = dxHint != 0 ? dxHint : (targetMapPos.x > cur.x ? 1 : -1);
        c.UpdateFacingFromHorizontalMove(new Vector2(dx, 0f));
    }
}
