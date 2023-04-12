using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillState", menuName = "State/SkillState")]
public class SkillState : State
{
    public float t;
    public SkillState(){
        stateName=StateName.SkillState;
    }
    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        EventController.Instance.TriggerEvent(chess.instanceID.ToString()+EventName.WhenUseSkill.ToString());
        chess.skillController.UseSkll();       
    }
    public override void Excute(Chess chess)
    {
        base.Excute(chess);
        chess.skillController.TimeAdd();
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
        //chess.property.GetValue(ValueType.Mana).currentValue=0;
        EventController.Instance.TriggerEvent(chess.instanceID.ToString() + EventName.WhenSkillOver.ToString());
    }

    public override State Clone()
    {
        SkillState ans= new SkillState();
        ans.stateName=stateName;
        return ans;
    }
}
