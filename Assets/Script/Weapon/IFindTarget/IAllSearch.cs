using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAllSearch : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        //throw new System.NotImplementedException();
    }
}

public class IAllSearch_Cat : IFindTarget
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
