using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
/// <summary>
/// 如果要访问某一行的chess，用MapManage.instance.tiles[i,j]的方式访问就好
/// </summary>
public class MapManage : MonoBehaviour
{
    public static MapManage instance;
    public Tile[,] tiles;
    public List<Tile> preTiles;
    public Vector2Int mapSize;
    public Vector2 tileSize = new Vector2(1.25f, 1);
    /// <summary>≥0 时仅 <see cref="mapPos.x"/> 不大于本值的格可参与种植；-1 表示不限制（保龄球等插件写入）。</summary>
    [System.NonSerialized] public int plantMaxMapColumnX = -1;
    [SerializeReference]
    public IInitMapManage initMapManage;
    public AudioPlayer BGMPlayer;
    public SpriteRenderer backGround;
    protected virtual void Awake()
    {
        if (instance == null||instance!=this)
            instance = this;
    }
    protected virtual void Start()
    {
        tiles = new Tile[mapSize.x, mapSize.y];
        initMapManage?.InitMap(tiles,mapSize,tileSize);
        //Debug.Log(tiles.Length);
        for (int i = 0; i < mapSize.y; i++)
        {
            preTiles.Add(tiles[mapSize.x - 1, i]);
        }
        //Debug.Log(tiles.Length);
        //Debug.Log(tiles[0, 0]);
    }
    public bool IfInMapRange(int x,int y)
    {
        return (x >= 0 && x < mapSize.x) && (y >= 0 && y < mapSize.y);
    }
    public bool IfInMapViewRange(int x, int y)
    {
        return (x >= 0 && x < mapSize.x-1) && (y >= 0 && y < mapSize.y);
    }
    public bool IsPlantColumnAllowed(int columnX)
    {
        return plantMaxMapColumnX < 0 || columnX <= plantMaxMapColumnX;
    }

    public virtual void AwakeTile()
    {
        for (int i = 0; i < mapSize.x ; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                if (!IsPlantColumnAllowed(i))
                    continue;
                var c = tiles[i, j] != null ? tiles[i, j].GetComponent<Collider2D>() : null;
                if (c != null) c.enabled = true;
            }
    }
    public virtual void SleepTile()
    {
        for (int i = 0; i < mapSize.x ; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                var c = tiles[i, j] != null ? tiles[i, j].GetComponent<Collider2D>() : null;
                if (c != null) c.enabled = false;
            }
    }
    public Tile   RandomTile()
    {
        return tiles[Random.Range(0,mapSize.x-1),Random.Range(0,mapSize.y-1)];
    }
    public List<Tile> NearTile(Tile tile)
    {
        List<Tile> list = new List<Tile>();
        Vector2Int mapPos = tile.mapPos;
        int[] dx = {-1,1,0,0};
        for(int i = 0; i < dx.Length; i++)
        {
            Vector2Int newPos = new Vector2Int(mapPos.x + dx[i], mapPos.y + dx[3 - i]);
            if (IfInMapRange(newPos.x,newPos.y))
            {
                list.Add(tiles[newPos.x, newPos.y]);
            }
        }
        return list;
    }
    /// <summary>周围八格（含对角）</summary>
    public List<Tile> GetEightNeighborTiles(Tile center)
    {
        var list = new List<Tile>();
        var p = center.mapPos;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if ((dx != 0 || dy != 0) && IfInMapRange(p.x + dx, p.y + dy))
                    list.Add(tiles[p.x + dx, p.y + dy]);
        return list;
    }
}
public interface IInitMapManage
{
    public void InitMap(Tile[,] tiles,Vector2Int mapSize,Vector2 tileSize);
}
public class AutoInitMap : IInitMapManage
{
    public GameObject tile;//tile的预制体
    public Transform tileFather;
    public void InitMap(Tile[,] tiles, Vector2Int mapSize, Vector2 tileSize)
    {
       
       
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                GameObject t =GameManage.Instantiate(tile, tileFather);
                tiles[i, j] = t.GetComponent<Tile>();
                tiles[i, j].mapPos = new Vector2Int(i, j);
                t.transform.position = new Vector2(i * tileSize.x, tileSize.y * j );
            }
        }
       
    }
}
public class ManualInitMap : IInitMapManage
{
    public List<Tile> tileList;
    public void InitMap(Tile[,] tiles, Vector2Int mapSize, Vector2 tileSize)
    {
        //tiles = new Tile[mapSize.x, mapSize.y];
        for (int i = 0; i < tileList.Count; i++)
        {
            Tile tile = tileList[i];
            tiles[tile.mapPos.x, tile.mapPos.y] = tile;
        }
    }
}
