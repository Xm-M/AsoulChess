using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Henshin", menuName = "Skill/Skill_Henshin")]
public class Skill_Henshin : Skill
{
    //private RuntimeAnimatorController run;

    // Start is called before the first frame update
    public AnimationClip idle;
    public AnimationClip run;
    public AnimationClip BaseIdle;
    public AnimationClip BaseRun;
    public override void SkillEffect(Chess user, params Chess[] target)
    {
        base.SkillEffect(user, target);
        Animator anima = user.GetComponent<Animator>();
        Replace(anima);
    }
    public void Replace(Animator anima)
    {
        //重要!!预先保存
        //run = anima.runtimeAnimatorController;
        var ride = new AnimatorOverrideController(anima.runtimeAnimatorController);
        anima.runtimeAnimatorController=ride;
        ride["idle"] = idle;
        ride["run"] = run;
    }
    public override void OnSkillExit(Chess user, params Chess[] target)
    {
        base.OnSkillExit(user, target);
        Animator anima = user.GetComponent<Animator>();
        HenshinBack(anima);
    }
    public void HenshinBack(Animator anima)
    {
        var ride = new AnimatorOverrideController(anima.runtimeAnimatorController);
        anima.runtimeAnimatorController = ride;
        ride["idle"] = BaseIdle;
        ride["run"] = BaseRun;
    }
}
