using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTile : MonoBehaviour
{
    public List<SpriteRenderer> tiles;
    public Vector2Int Pos;

    public void SetSprite(Vector2Int pos)
    {
        Pos = pos;
        Vector2Int up= pos.y + 1<= MapCreator.instance.yMax-1&& MapCreator.instance.maps[pos.x, pos.y + 1] != 1?Vector2Int.up:Vector2Int.zero;
        Vector2Int left =pos.x-1>=0&& MapCreator.instance.maps[pos.x-1, pos.y] != 1 ? Vector2Int.left : Vector2Int.zero;
        Vector2Int down =pos.y-1>=0&& MapCreator.instance.maps[pos.x, pos.y-1] != 1 ? Vector2Int.down : Vector2Int.zero;
        Vector2Int right =pos.x+1<= MapCreator.instance.xMax - 1&& MapCreator.instance.maps[pos.x+1, pos.y] != 1 ? Vector2Int.right : Vector2Int.zero;
        if (up == Vector2Int.zero&&left==Vector2Int.zero&&down==Vector2Int.zero&&right==Vector2Int.zero)
            ObjectPool.instance.Recycle(gameObject);
        tiles[0].sprite = MapCreator.instance.GetSprite(up + left);
        tiles[1].sprite = MapCreator.instance.GetSprite(up + right);
        tiles[2].sprite = MapCreator.instance.GetSprite(down + left);
        tiles[3].sprite = MapCreator.instance.GetSprite(down + right);
    }
}
