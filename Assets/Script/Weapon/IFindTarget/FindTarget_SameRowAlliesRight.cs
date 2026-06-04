using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 与施法者同一行的所有友方棋子（整行，含自身及身后）。
/// </summary>
[Serializable]
public class FindTarget_SameRowAlliesRight : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        if (user == null || user.IfDeath || MapManage.instance == null)
            return;

        Tile stand = user.moveController?.standTile;
        if (stand == null)
            return;

        int rowY = stand.mapPos.y;
        MapManage map = MapManage.instance;

        for (int x = 0; x < map.mapSize.x; x++)
        {
            if (!map.IfInMapRange(x, rowY))
                continue;

            Tile tile = map.tiles[x, rowY];
            Chess ally = tile?.stander;
            if (ally == null || ally.IfDeath || !ally.CompareTag(user.tag))
                continue;

            targets.Add(ally);
        }
    }
}
