using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManage_Qua : MapManage
{
    protected override void Start()
    {
        dx=new int[4] { 0,0,1,-1};
        dy=new int[4] { 1,-1,0,0};
        tiles = new Tile[mapSize.x+1, mapSize.y];
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                Debug.Log(j);
                GameObject t = Instantiate(tile, tileFather);
                tiles[i, j] = t.GetComponent<Tile>();
                tiles[i, j].mapPos = new Vector2Int(i,j);
                t.transform.position = new Vector2(i * tileSize.x, tileSize.y * j);
            }
        }
        preTiles = new List<Tile>(mapSize.y);
        for (int i = 0; i < mapSize.y; i++)
        {
            {
                GameObject t = Instantiate(tile, tileFather);
                t.GetComponent<Collider2D>().enabled = true;
                Tile ti = t.GetComponent<Tile>();
                tiles[mapSize.x-1, i] = ti;
                ti.mapPos= new Vector2Int(mapSize.x - 1, i);
                ti.ifPrePareTile = true;
                preTiles.Add(ti);
                t.transform.position = new Vector2(mapSize.x * tileSize.x, tileSize.y * i);
            }
        }
    }
    public override int Distance(Tile tile1, Tile tile2)
    {
        return Mathf.Abs( tile1.mapPos.x-tile2.mapPos.x )+Mathf.Abs(tile2.mapPos.y-tile1.mapPos.y);
    }
    public override int Distance(Vector2Int tile1, Vector2Int tile2)
    {
        return Mathf.Abs(tile1.x - tile2.x) + Mathf.Abs(tile2.y - tile1.y);
    }
    public override void RoundTile(Tile tile, List<Tile> newlist )
    {
        if (tile == null || !IfInMapRange(tile.mapPos.x, tile.mapPos.y)) return;
        newlist.Clear();
        for (int i = 0; i < dx.Length; i++)
        {
            Vector2Int mapPos = new Vector2Int(dx[i]+tile.mapPos.x, dy[i]+tile.mapPos.y);
            if (IfInMapRange(mapPos.x, mapPos.y))
            {
                newlist.Add(tiles[mapPos.x, mapPos.y]);
            }
        }
    }
}
