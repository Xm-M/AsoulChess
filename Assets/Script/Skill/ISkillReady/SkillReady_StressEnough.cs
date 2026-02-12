using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillReady_StressEnough : ISkillReady
{
    public int limit;
    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        int stress = 0;
        user.skillController.context.TryGet<int>("stress",out stress);
        if (stress > limit)
        {
            stress-=limit;
            user.skillController.context.Set<int>("stress", stress);
            return true;
        }
        return false;
    }
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        //throw new NotImplementedException();
    }
}

