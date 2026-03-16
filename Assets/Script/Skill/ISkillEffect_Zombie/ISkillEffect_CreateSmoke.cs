using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISkillEffect_CreateSmoke : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (Effect_Smoke.Instance != null)
        {
            Effect_Smoke.Instance.ShowSmoke(user.moveController.standTile, 3);
            Debug.Log("使用烟雾技能");
        }
    }
}
