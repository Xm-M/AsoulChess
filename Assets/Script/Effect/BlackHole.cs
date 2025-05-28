using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    public Tile standTile;
    public float speed=5f;
    [SerializeReference]
    public DizznessBuff buff;
    List<Chess> zombies;
    Timer timer;
    //float delay;
    bool over;
    private void Start()
    {
        zombies=new List<Chess>();
    }
    public void Init(Tile tile,float delay)
    {
        standTile = tile;
        over = false;
        timer = GameManage.instance.timerManage.AddTimer(Over, delay);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(tag))
        {
             
            Chess chess = collision.GetComponent<Chess>();
            if (chess == null) return;
            Vector2Int pos=Sock(chess.moveController.standTile.mapPos);
            Tile nextPos = MapManage.instance.tiles[pos.x, pos.y];
            zombies.Add(chess);
            //chess.StartCoroutine(moveToNextPos(chess, nextPos));
            SockToTile(chess, nextPos);
            
        }
    }
    public void SockToTile(Chess c,Tile tile)
    {
        //c.stateController.ChangeState(StateName.DizzyState);
        c.moveController.MoveToTarget(tile, speed);
        //Debug.Log("剩余时间" + timer.LeftTime());
        if (timer != null && timer.LeftTime() > 0)
        {
            buff.continueTime=timer.LeftTime();
            //Debug.Log("剩余时间" + buff.continueTime);
           
        }else buff.continueTime=Time.deltaTime;
        c.buffController.AddBuff(buff);
    }
    //public void SockOver(Chess c)
    //{
    //    if (!c.IfDeath)
    //    {
    //        c.stateController.ChangeState(StateName.IdleState);
    //       //c.moveController.standTile = targetTile;
    //   }
    //}
    //IEnumerator moveToNextPos(Chess c,Tile targetTile)
    //{
    //    
    //    while (Vector2.Distance(c.transform.position,targetTile.transform.position)>0.1f)
    //    {

    //        c.transform.position = Vector2.MoveTowards(c.transform.position, targetTile.transform.position,
    //            speed * Time.deltaTime);
    //        yield return null;
    //    }
    //    while (!over)
    //    {
    //        yield return null;
    //    }
    //    
    //}
    public void Over()
    {
        GetComponent<Animator>().Play("over");
        //foreach(var chess in zombies)
        //{
        //    if(!chess.IfDeath)
        //        chess.stateController.ChangeState(StateName.IdleState);
        //}
        over = true;
        zombies.Clear();
        timer = null;
    }
    public Vector2Int Sock(Vector2Int StartPos)
    {
        
        Vector2Int center = standTile.mapPos;

        // 如果StartPos已经在中心位置，不做修改
        if (StartPos == center)
        {
            return StartPos;
        }

        // 移动StartPos一步，向中心点靠近
        int newX, newY;
        if (center.x - StartPos.x != 0)
        {
            newX = StartPos.x + (int)((center.x - StartPos.x) / Mathf.Abs(center.x - StartPos.x));
        }
        else newX = center.x;
        if (center.y - StartPos.y != 0)
        {
            newY = StartPos.y + (int)((center.y - StartPos.y) / Mathf.Abs(center.y - StartPos.y));
        }
        else newY=center.y;
        //Debug.Log(StartPos + " " + newX + "," + newY);
        return new Vector2Int(newX, newY);
    }
}
