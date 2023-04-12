using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentCreate : MonoBehaviour
{
    public Sprite roomUISprite;//房间的UI图标 为什么不保存在roommap里呢
    public int roomSizeX=18,roomSizeY=10;//房间的大小
    public float tileSize=1;
    public GameObject mapTile;//地板的tile
    public GameObject wallTile;//墙壁的tile
    public GameObject wallTileDown;//下面

    public GameObject wallTileLeft;//左边

    public GameObject wallTileRight;//右边
    IEnumerator SetPos(Vector2 roomPos){
        yield return null;
        Camera.main.GetComponent<Camare>().MoveTo( new Vector3(roomSizeX*tileSize/2,roomSizeY*tileSize/2,Camera.main.transform.position.z)+(Vector3)roomPos);
        MapManage.instance.tileFather.transform.position=new Vector2(roomSizeX*tileSize/2,roomSizeY*tileSize/2)+roomPos;
    }
    public virtual void Create(Vector2 roomPos){
        for(int i=0;i<roomSizeX;i++){
            for(int j=1;j<roomSizeY;j++){
                GameObject maptile= Instantiate(mapTile);
                maptile.transform.position=new Vector2(tileSize*i,tileSize*j)+roomPos;
            }
        }
        for(int i=0;i<roomSizeX;i++){
            GameObject maptile= Instantiate(wallTile);
            maptile.transform.position=new Vector2(tileSize*i,tileSize*roomSizeY)+roomPos;
            GameObject downMapTile= Instantiate(wallTileDown);
            downMapTile.transform.position=new Vector2(tileSize*i,tileSize*0)+roomPos;
        }
        for(int i=1;i<roomSizeY;i++){
            GameObject tileLeft= Instantiate(wallTileLeft);
            tileLeft.transform.position=new Vector2(tileSize*-1,tileSize*i)+roomPos;
            GameObject tileRight= Instantiate(wallTileRight);
            tileRight.transform.position=new Vector2(tileSize*roomSizeX,tileSize*i)+roomPos;
        }
    }
}
