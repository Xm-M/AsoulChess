using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    [SerializeReference]
    public IInitMapManage initMapManage;
    private void Awake()
    {
        if (instance == null||instance!=this)
            instance = this;
        
    }
    protected virtual void Start()
    {
        tiles = new Tile[mapSize.x, mapSize.y];
        initMapManage?.InitMap(tiles,mapSize,tileSize);
        Debug.Log(tiles.Length);
        for (int i = 0; i < mapSize.y; i++)
        {
            preTiles.Add(tiles[mapSize.x - 1, i]);
        }
        Debug.Log(tiles.Length);
        Debug.Log(tiles[0, 0]);
    }
    public bool IfInMapRange(int x,int y)
    {
        return (x >= 0 && x < mapSize.x) && (y >= 0 && y < mapSize.y);
    }
    public virtual void AwakeTile()
    {
        for (int i = 0; i < mapSize.x ; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                tiles[i, j].GetComponent<Collider2D>().enabled = true;
            }
    }
    public virtual void SleepTile()
    {
        for (int i = 0; i < mapSize.x ; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                tiles[i, j].GetComponent<Collider2D>().enabled = false;
            }
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