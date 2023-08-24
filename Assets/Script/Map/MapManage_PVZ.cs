using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//类似PVZ类型的地图 都是向右走啦 
public class MapManage_PVZ : MapManage_Qua
{
    public Transform sunLightRecyclePos;
    public List<Tile> zombiePreTile;
    public Tile roomTile;
    public AudioSource au;
    protected override void Start()
    {
        base.Start();
        EventController.Instance.AddListener(EventName.GameOver.ToString(), () => au.Stop());
    }
    public override int Distance(Tile tile1, Tile tile2)
    {
        if (tile1.mapPos.y != tile2.mapPos.y) return 100;
        return base.Distance(tile1, tile2);
    }
    public override int Distance(Vector2Int tile1, Vector2Int tile2)
    {
        if (tile1 .y != tile2.y) return 100;
        return base.Distance(tile1, tile2);
    }

}
