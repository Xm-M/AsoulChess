using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlingMove : FindTileMethod
{
    Vector2Int Dir;
    public BowlingArmor armor;
    float maxy, miny;
    public override void StartMoving(Chess c)
    {
        base.StartMoving(c);
        Dir = new Vector2Int((int)c.transform.right.x,0);
        armor.onHit.AddListener(OnHit);
        miny = MapManage.instance.tiles[0, 0].transform.position.y;
        maxy = MapManage.instance.tiles[0, 4].transform.position.y;
    }
    public override void EndMoving(Chess c)
    {
        base.EndMoving(c);
        armor.onHit.RemoveListener(OnHit);
    }
    public override Tile FindNextTile(Chess c)
    {
        //Tile current = c.moveController.standTile;
        //Vector2Int nextPos = current.mapPos+Dir;
        //Vector2Int mapsize = MapManage.instance.mapSize;
        //if (nextPos.y >= mapsize.y|| nextPos.y < 0)
        //{
        //    ChangeDir();
        //}
        //nextPos = current.mapPos + Dir;
        //if (nextPos.x >= MapManage.instance.mapSize.x)
        //{
        //    return (MapManage.instance as MapManage_PVZ).deathTile;
        //}
        //else if (nextPos.x < 0)
        //{
        //    return (MapManage.instance as MapManage_PVZ).roomTile[current.mapPos.y];
        //}
        //return MapManage.instance.tiles[nextPos.x,nextPos.y];
        return null;
    }
    public override void WhenMoving(Chess c)
    {

        c.transform.position = Vector2.MoveTowards(c.transform.position, (Vector2)c.transform.position + Dir,
            Time.deltaTime * c.propertyController.GetMoveSpeed());
        Vector3 currentPos = c.transform.position;
        if (currentPos.y > maxy )
        {
            ChangeDir();
            c.transform.position = new Vector3(currentPos.x,maxy, currentPos.z);
        }else if( currentPos.y < miny)
        {
            ChangeDir();
            c.transform.position = new Vector3(currentPos.x,miny, currentPos.z);
        }
    }
    public void OnHit()
    {
        ChangeDir();
    }
    public void ChangeDir()
    {
        if (Dir.y == 0)
        {
            Dir.y = Random.Range(-1f, 1f) > 0 ? 1 : -1;
        }
        if (Dir.y > 0)
        {
            Dir.y = -1;
        }
        else
        {
            Dir.y = 1;
        }
    }
}
