using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_Item : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
         user.UnSelectable();
    }
}
