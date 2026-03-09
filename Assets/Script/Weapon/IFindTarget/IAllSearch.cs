using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class IAllSearch : IFindTarget
//{
//    public void FindTarget(Chess user, List<Chess> targets)
//    {
//        //throw new System.NotImplementedException();
//    }
//}

public class IAllSearch_Rana : IFindTarget
{
    int minX;
    Chess current;
    public void FindTarget(Chess user, List<Chess> targets)
    {
        //throw new System.NotImplementedException();
        targets.Clear();
        if (ChessTeamManage.Instance.GetEnemyTeam(user.tag).Count == 0) return;
        minX = MapManage_PVZ.instance.mapSize.x;
        foreach(var chess in ChessTeamManage.Instance.GetEnemyTeam(user.tag))
        {
            if (chess.moveController.standTile.mapPos.x < minX)
            {
                minX = chess.moveController.standTile.mapPos.x;
                current = chess;
            }else if (chess.moveController.standTile.mapPos.x == minX)
            {
                if (chess.transform.position.x < current.transform.position.x)
                {
                    current = chess;
                }
            }
        }
        if(Vector2.Distance(user.transform.position,current.transform.position)<user.propertyController.GetAttackRange())
            targets.Add(current);
    }
}
//public class IAllSearch_Rana : IFindTarget
//{
//    public void FindTarget(Chess user, List<Chess> targets)
//    {
//        //throw new System.NotImplementedException();
//        float mindis = 1000;
//        foreach(var enemy in GameManage.instance.chessTeamManage.GetEnemyTeam(user.tag))
//        {
//            //这里应该有一个优先检测气球僵尸的
//            if (Vector2.Distance(user.transform.position, enemy.transform.position)<mindis)
//            {

//            }
//        }
//    }
//}
