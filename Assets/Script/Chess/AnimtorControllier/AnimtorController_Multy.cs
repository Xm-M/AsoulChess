using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimtorController_Multy : AnimatorController
{
    public string skillTag;
    public List<string> skillNames;

    public override void PlaySkill()
    {
        base.PlaySkill();
        int n = animator.GetInteger(skillTag);
        animator.Play(skillNames[n]);
    }
}
