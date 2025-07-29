using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimtorController_Nina : AnimatorController
{
    public override void PlaySkill()
    {
        int t = animator.GetInteger("skill1");
        if (t == 2)
        {
            animator.Play("skill2");
        }else if(t==1)
        {
            animator.Play("skill1");
        }
        else base.PlaySkill();

    }
}
