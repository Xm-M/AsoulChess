using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_NGskill : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
         user.moveController.standTile.stander.Death();
    }
}
