using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveState",menuName ="State/MoveState")]
public class MoveState : State
{
    AStart aStart;
    bool ifMove;
    public MoveState(){
        stateName=StateName.MoveState;
        aStart=new AStart();
    }
     

    public override void Enter(Chess chess)
    {
        chess.animator.Play("run");
        GameManage.instance.AddMoveChess(chess);
        base.Enter(chess);
    }

    public override void Exit(Chess chess)
    {
        chess.animator.Play("idle");
        base.Exit(chess);
    }
    public void SearchTile()
    {
         chess.FindTarget();
        //List<Chess> chessList = FindTarget();
        if (chess.target == null) return;
        aStart.Search(chess.standTile.mapPos, chess.target.standTile.mapPos, MapManage.instance.tiles, chess.propertyController.Data.attacRange);
        //for (int i = 0; i < chessList.Count; i++)
        //{
        //    chess.target = chessList[i];
            
        //    if (aStart.nextTile != chess.standTile) break;
        //}
    }

    public List<Chess>   FindTarget()
    {
        List<Chess> enemyTeam = ChessFactory.instance.FindEnemyList(chess.tag);
        if (enemyTeam.Count > 0)
        {
            for (int i = 0; i < enemyTeam.Count; i++)
            {
                int dis = MapManage.instance.Distance(chess.standTile, enemyTeam[i].standTile);
                for (int j = i+1; j < enemyTeam.Count; j++)
                {
                    int dis2= MapManage.instance.Distance(chess.standTile, enemyTeam[j].standTile);
                    if (dis > dis2)
                    {
                        dis = dis2;
                        Chess t = enemyTeam[i];
                        enemyTeam[i]=enemyTeam[j];
                        enemyTeam[j] = t;
                    }
                }
            }
        }
        return enemyTeam;
    }
    public virtual void MoveToNextTile()
    {
        if (ifMove == true) return;
        SearchTile();
        if (aStart.nextTile == chess.standTile || chess.propertyController.Data.speed == 0)
        {
            return;
        }
        chess.standTile.ChessLeave();
        aStart.nextTile.ChessEnter(chess,true);
        if (ifMove == false)
        { 
           chess.StartCoroutine(Moving());
        }
    }
    IEnumerator Moving()
    {
        ifMove = true;
        while (Vector2.Distance(chess.transform.position,chess.standTile.transform.position) > 0.0001)
        {            
            chess.transform.position = Vector2.MoveTowards(chess.transform.position, chess.standTile.transform.position, chess.propertyController.Data.speed * Time.deltaTime);
            yield return null;
        }
        ifMove = false;
        GameManage.instance.AddMoveChess(chess);
    }
    
    public override State Clone()
    {
        MoveState ans= new MoveState();
        ans.stateName=stateName;
        return ans;
    }
}
