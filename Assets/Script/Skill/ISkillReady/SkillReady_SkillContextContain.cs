using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class SkillReady_SkillContextContain  : ISkillReady
{
    public string contextName;

    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        Object ob = null;
        user.skillController.context.TryGet<Object>(contextName, out ob);
        return ob;
        //return false;
    }

    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
         
    }
}
