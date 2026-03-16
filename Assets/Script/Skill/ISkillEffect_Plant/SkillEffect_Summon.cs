using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SkillEffect_Summon : ISkillEffect
{
    public List<PropertyCreator> creators;
    public Transform summonPos;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        int n=Random.Range(0, creators.Count);
        Chess summon= ChessTeamManage.Instance.CreateChess(creators[n], user.moveController.standTile,
             user.tag);
        if (summonPos != null) summon.transform.position = summonPos.transform.position;
    }
}
