using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Item : ISkillEffect
{
    [SerializeReference]
    public Buff ItemBuff;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        //user.UnSelectable();
        Chess target = user.moveController.standTile.stander;
        if (target != null)
           target.buffController.AddBuff(ItemBuff);
    }
 
}
