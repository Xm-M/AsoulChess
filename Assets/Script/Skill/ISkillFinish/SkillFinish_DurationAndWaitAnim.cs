using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillFinish_DurationAndWaitAnim : ISkillFinish
{
    public string backName;
    bool play;
    public bool IsFinished(Chess user, SkillConfig config, SkillRuntimeInfo runtime)
    {
        SkillRuntimeInfo_Duration info = runtime as SkillRuntimeInfo_Duration;
        if (!play)
        {
            info.currentTime += Time.deltaTime;
            if(info.currentTime > info.maxTime )
            {
                play = true;
                user.animatorController.animator.Play(backName);
            }
            return false;
        }
        AnimatorStateInfo animStateInfo = user.animatorController.animator.GetCurrentAnimatorStateInfo(0);
        if (!animStateInfo.IsName(backName))
        {
            return false;
        }
        bool anim= user.animatorController.IfAnimPlayOver();
        if (anim)
        {
            info.currentTime = 0;
            play = false;
            return true;
        }
        else return false;
    }
}
