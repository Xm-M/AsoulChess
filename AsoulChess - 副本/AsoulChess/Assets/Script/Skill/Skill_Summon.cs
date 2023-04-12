using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Summon", menuName = "Skill/Skill_Summon/Skill_Summon")]
public class Skill_Summon : Skill
{
    public GameObject summonChess;
    public GameObject summonEffect;
    public override void SkillEffect(Chess user, params Chess[] target)
    {
        if (target == null)
        {
            if (user.target == null || user.target.ifDeath) return;
            else target[0]=user.target;
        }
        List<Tile> tiles = new List<Tile>();
        MapManage.instance.RoundTile(target[0].standTile, tiles);
        base.SkillEffect(user, target);
        Tile t=null;
        foreach (var tile in tiles)
        {
            if (tile.IfMoveable)
            {
                t = tile;
            }
        }
        if (t == null) return;
        Chess chess = ChessFactory.instance.ChessCreate(summonChess.GetComponent<Chess>(),  t,user.tag);
        EventController.Instance.TriggerEvent<Chess>(EventName.ChessEnterDesk.ToString(),chess);
        EventController.Instance.TriggerEvent<Chess>(user.instanceID.ToString() + EventName.WhenSummon.ToString(),
            chess);
        if(summonEffect != null)
            ObjectPool.instance.Create(summonEffect).transform.position=chess.transform.position;

    }
}
