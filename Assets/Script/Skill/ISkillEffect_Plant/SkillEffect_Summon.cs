using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SkillEffect_Summon : ISkillEffect
{
    public List<PropertyCreator> creators;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        int n=Random.Range(0, creators.Count);
        ChessTeamManage.Instance.CreateChess(creators[n], user.moveController.standTile,
             user.tag);
    }
}
