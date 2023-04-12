using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// 
/// </summary>
public class AStart 
{
    static List<Vector2Int> open;
    static List<Vector2Int> close;
    static List<Tile> tiles; 
    public Tile nextTile;
    public AStart()
    {
        if (open == null)
        {
            open = new List<Vector2Int>();
            close = new List<Vector2Int>();
            tiles = new List<Tile>();
        }
    }
    public virtual void Search(Vector2Int current,Vector2Int target,Tile[,] maps,int range)
    {
        ResetList();
        open.Add(current);
        float dis = MapManage.instance.CalculateH(current, target);
        maps[current.x, current.y].AstarValue.x = 0;
        maps[current.x, current.y].AstarValue.y = dis;
         
        while (open.Count != 1)
        {
 
            Vector2Int cPos = FindMinF(maps);
            close.Add(cPos);
            MapManage.instance.RoundTile(MapManage.instance.tiles[cPos.x, cPos.y], tiles);
            foreach (var tile in tiles)
            {
                if (MapManage.instance.Distance(tile, MapManage.instance.tiles[target.x, target.y]) < range)
                {
                    nextTile = maps[cPos.x, cPos.y];
                    while (nextTile.preTile != maps[current.x, current.y])
                    {
                        nextTile = nextTile.preTile;
                    }
                    return;
                }
                else if (tile.IfMoveable == false)
                    continue;
                
                Vector2Int newPos = tile.mapPos;
                float ng = maps[cPos.x, cPos.y].AstarValue.x + 1;
                float nf = ng + MapManage.instance.CalculateH(target, newPos);
                if (open.Contains(newPos))
                {
                    if (maps[newPos.x, newPos.y].AstarValue.y < nf)
                    {
                        tile.preTile = maps[cPos.x, cPos.y];
                        maps[newPos.x, newPos.y].AstarValue.y = nf;
                        maps[newPos.x, newPos.y].AstarValue.x = ng;
                        open.Remove(newPos);
                        OpenAdd(newPos, maps);
                    }
                }
                else if (!close.Contains(newPos))
                {
                    tile.preTile = maps[cPos.x, cPos.y];
                    maps[newPos.x, newPos.y].AstarValue.y = nf;
                    maps[newPos.x, newPos.y].AstarValue.x = ng;
                    OpenAdd(newPos, maps);
                }
            }
        }
        nextTile = maps[current.x, current.y];
    }
    public Vector2Int FindMinF(Tile[,] maps)
    {
        Vector2Int vector2Int = open[1];
        Vector2Int t = open[1];
        open[1] = open[open.Count - 1];
        open[open.Count - 1] = t;
        open.RemoveAt(open.Count-1);
        SifiDown(maps);
        return vector2Int;
    }
    public void OpenAdd(Vector2Int vector2Int, Tile[,] maps)
    {
        open.Add(vector2Int);
        SifiUp(maps);
    }
    public void SifiDown(Tile[,] maps)
    {
        int i=1;
        while (i*2 < open.Count)
        {
            i *= 2;
            if (i + 1 < open.Count && maps[open[i].x,open[i].y].AstarValue.y > maps[open[i+1].x, open[i+1].y].AstarValue.y ) i++;
            if (maps[open[i].x, open[i].y].AstarValue.y < maps[open[i/2].x, open[i/2].y].AstarValue.y )//i���·���i/2���Ϸ�
            {
                Vector2Int n = open[i];
                open[i] = open[i / 2];
                open[i / 2] = n;
            }
            else return;
        }
    }
    public void SifiUp(Tile[,] maps)
    {
        int i = open.Count-1;
        if (i == 1) return;
        while (i/2 >= 1)
        {
            if (maps[open[i].x, open[i].y].AstarValue.y < maps[open[i/2].x, open[i/2].y].AstarValue.y ) 
            {
                Vector2Int n = open[i];
                open[i]=open[i/2];
                open[i / 2] = n;
            }
            else return;
            i /= 2;
        }
    }
    public void ResetList()
    {
        MapManage.instance.InitAllTileValue();
        open.Clear();
        open.Add(new Vector2Int(100, 100));
        close.Clear();
    }
}

