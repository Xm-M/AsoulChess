using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个名字真不能改 所以理解一下 这个就是技能结束的判断函数
/// </summary>
public class IfAnimPlayOverTransition : Transition
{
    public override bool ifReach(Chess chess)
    {
        return chess.skillController.activeSkill.IsSkillFinished(chess);
    }
    public override Transition Clone()
    {
        IfAnimPlayOverTransition clone = new IfAnimPlayOverTransition();
        return clone;
    }
}
public class IfAnimationOverTransisiton : Transition
{
    public string animName; // 这里建议你明确：它到底是 State 名 还是 Clip 名

    public override bool ifReach(Chess chess)
    {
        var animator = chess.animatorController?.animator;
        if (animator == null) return false;

        // 可选：过渡中通常不判“播完”
        if (animator.IsInTransition(0))
        {
            return false;
          
        }
        // 1) 如果你这里的 animName 是“State 名”
        if (!string.IsNullOrEmpty(animName))
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(animName);
        }
        return chess.animatorController.IfAnimPlayOver();
    }

    public override Transition Clone()
    {
        return new IfAnimationOverTransisiton
        {
            animName = this.animName
        };
    }
}