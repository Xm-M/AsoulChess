using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManage : MonoBehaviour
{
    public static MapManage instance;
    public Tile[,] tiles;
    public List<Tile> preTiles;//备战席
    public GameObject tile;
    public Transform tileFather;
    public Vector2Int mapSize;
    public Vector2 tileSize = new Vector2(1.25f, 1);

    protected int[] dx = { 1, -1, 0, 0, -1, 1 };
    protected int[] dy = { 0, 0, 1, -1, 1, -1 };
    int[] dz = { -1, 1, -1, 1, 0, 0 };
    private void Awake()
    {
        if (instance == null||instance!=this)
            instance = this;
        else Destroy(gameObject);
    }
    protected virtual void Start()
    {
        tiles = new Tile[mapSize.x, mapSize.y];
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                GameObject t = Instantiate(tile, tileFather);
                tiles[i, j] = t.GetComponent<Tile>();
                tiles[i, j].cubePos = ChangeMapToCubePos(new Vector2Int(i, j));
                tiles[i, j].mapPos = ChangeCubeToMapPos(tiles[i, j].cubePos);
                t.transform.position = new Vector2(i * tileSize.x, tileSize.y * j + i % 2 * (tileSize.y / 2));
            }
        }
        preTiles=new List<Tile>(8);
        for(int i=0;i<8;i++){{
            GameObject t = Instantiate(tile, tileFather);
            t.GetComponent<Collider2D>().enabled=true;
            Tile ti=t.GetComponent<Tile>();
            ti.ifPrePareTile=true;
            preTiles.Add(ti);
            t.transform.position=new Vector2(-1.5f*tileSize.x,tileSize.y*i);
        }}
    }
    public virtual void InitAllTileValue()
    {
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                tiles[i, j].InitAstarValue();
            }
        }
    }
    public static Vector3Int ChangeMapToCubePos(Vector2Int mapPos)
    {
        Vector3Int pos = new Vector3Int
        {
            x = mapPos.x,
            y = mapPos.y - (mapPos.x) / 2
        };
        pos.z = 0 - (pos.x + pos.y);
        return pos;
    }
    public static Vector2Int ChangeCubeToMapPos(Vector3Int cubePos)
    {
        Vector2Int pos = new Vector2Int
        {
            x = cubePos.x,
            y = cubePos.y + (cubePos.x) / 2
        };
        return pos;
    }
    public virtual int Distance(Tile tile1,Tile tile2)
    {
        if (tile1 == null || tile2 == null) return 1000;
        return (Mathf.Abs(tile1.cubePos.x - tile2.cubePos.x) + Mathf.Abs(tile2.cubePos.y - tile1.cubePos.y) + Mathf.Abs(tile1.cubePos.z - tile2.cubePos.z)) / 2;
    }
    public virtual int Distance(Vector2Int tile1, Vector2Int tile2)
    {
        if(IfInMapRange(tile1.x,tile1.y)&&IfInMapRange(tile1.x,tile2.y))
         return Distance(tiles[tile1.x,tile1.y],tiles[tile2.x,tile2.y]);
        return 100;
    }
    public float RealDis(Vector2Int tile1, Vector2Int tile2)
    {
        if (IfInMapRange(tile1.x, tile1.y) && IfInMapRange(tile1.x, tile2.y))
            return (tiles[tile1.x, tile1.y].transform.position - tiles[tile2.x, tile2.y].transform.position).magnitude;
        return 100;
    }
    public float CalculateH(Vector2Int tile1, Vector2Int tile2)
    {
        return Distance(tile1, tile2)+RealDis(tile1,tile2)/10;
    }
    public bool IfInMapRange(int x,int y)
    {
        return (x >= 0 && x < mapSize.x) && (y >= 0 && y < mapSize.y);
    }
    public void AwakeTile()
    {
        for (int i = 0; i < mapSize.x / 2; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                if (tiles[i, j].IfMoveable)
                {
                    tiles[i, j].GetComponent<Collider2D>().enabled = true;
                    tiles[i, j].GetComponent<SpriteRenderer>().color = Color.red;
                }
            }
    }
    public void SleepTile()
    {
        for (int i = 0; i < mapSize.x / 2; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                if (tiles[i, j].IfMoveable)
                {
                    tiles[i, j].GetComponent<Collider2D>().enabled = false;
                    tiles[i, j].GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
    }
    /// <summary>
    /// 返回一个tile的周围的所有Tile
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="newlist"></param>
    /// <param name="qua"></param>
    public virtual void RoundTile(Tile tile,List<Tile> newlist )
    {
        if (!tile || tile.mapPos.x > mapSize.x || tile.mapPos.y > mapSize.y) return;
        newlist.Clear();
        Vector3Int tilepos = tile.cubePos;
        for (int i = 0; i < dx.Length; i++)
        {
            Vector2Int mapPos = ChangeCubeToMapPos(new Vector3Int(tilepos.x + dx[i], tilepos.y + dy[i], tilepos.z + dz[i]));
            if (IfInMapRange(mapPos.x, mapPos.y))
            {
                newlist.Add(tiles[mapPos.x, mapPos.y]);
            }
        }
    }
    public Tile GetPreTile(){
        foreach(var tile in preTiles){
            if(tile.ifMoveable)
                return tile;
        }
        return null;
    }
}