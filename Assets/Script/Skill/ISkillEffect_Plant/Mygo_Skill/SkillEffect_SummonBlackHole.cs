using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class SkillEffect_SummonBlackHole : ISkillEffect
{
    public int FontNum = 4;
    public GameObject blackHole;
    GameObject black;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        black = ObjectPool.instance.Create(blackHole);
        black.tag = user.tag;
        Tile tile;
        Vector2Int standPos = user.moveController.standTile.mapPos;
        if (standPos.x + FontNum < MapManage.instance.mapSize.x)
        {
            tile = MapManage.instance.tiles[standPos.x + FontNum, standPos.y];
        }
        else
        {
            tile = MapManage.instance.tiles[MapManage.instance.mapSize.x - 1, standPos.y];
        }
        black.transform.position = tile.transform.position;
        black.GetComponent<BlackHole>().Init(tile, config.baseDamage[0]);
        if (Effect_Smoke.Instance != null)
            Effect_Smoke.Instance.HideSmoke(tile.transform.position, config.baseDamage[1], config.baseDamage[0]);

    }

 
}
public class LevelUpPlant_Mygo : LevelUpPlant
{
    public override bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        bool ans = tile.stander && (tile.stander.propertyController.creator == basePlant);
        Chess stander = tile.stander;
        if ( ans)
        {
            int mygo = 0;
            stander.skillController.context.TryGet<int>("mygo",out mygo);
            if (mygo == 4)
            {
                //tile.stander.Death();
                //stander.Death();
                return  true;
            }
        }
        return false;
    }
}
