using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillReady_BeAttack : ISkillReady
{
    //public float checkRange;
    bool beAttack;
    //bool init;
    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {   
        bool ans=beAttack;
        beAttack = false;
        return ans;
    }
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        //Debug.Log("³õÊ¼»¯");
        user.propertyController.onGetDamage.AddListener(OnGetDamage);
    }
    void OnGetDamage(DamageMessege DM)=>beAttack=true;

}
