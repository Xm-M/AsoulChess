using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// min=0的时候说明要一个Mygo单位在附近就能使用
/// min=1的时候说明要四个Mygo单位在附近才能使用
/// </summary>
public class SkillReady_Mygo : ISkillReady
{
    public int min=0;
    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        int mygo = 0;
        user.skillController.context.TryGet<int>("mygo",out mygo);
        return mygo > min;
    }
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
         
    }
}
public class Transition_Mygo : Transition
{
    public int min = 0;
    public bool isCheck=true;
    public override bool ifReach(Chess chess)
    {
        //return base.ifReach(chess);
        int mygo = 0;
        chess.skillController.context.TryGet<int>("mygo", out mygo);
        //Debug.Log(mygo);
        if (isCheck)
        {
            return mygo > min;
        }
        else
        {
            return mygo < min;
        }
    }
    public override Transition Clone()
    {
        //throw new System.NotImplementedException();
        Transition_Mygo transition = new Transition_Mygo();
        transition.min = min;
        transition.isCheck = isCheck;
        return transition;
    }
}