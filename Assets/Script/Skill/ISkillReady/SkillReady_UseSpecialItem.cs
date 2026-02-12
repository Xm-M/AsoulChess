using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillReady_UseSpecialItem : ISkillReady
{
    public string ItemTag;

    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        bool Item = false; 
        user.skillController.context.TryGet<bool>(ItemTag, out Item);
        if (Item)
        {
            user.skillController.context.Set<bool>(ItemTag, false);
            return true;
        }
        return false;
    }
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        //throw new NotImplementedException();
    }
}
