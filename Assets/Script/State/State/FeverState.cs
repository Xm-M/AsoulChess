using UnityEngine;

/// <summary>
/// AveMujica 羁绊 Fever；状态图边与迁移由预制体上的 <see cref="StateGraph"/> 自行配置。
/// </summary>
public class FeverState : State
{
    public FeverState()
    {
        stateName = StateName.FeverState;
    }

    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        if (chess.animatorController != null && chess.animatorController.animator != null)
            chess.animatorController.animator.Play("fever");
    }

    public override void Exit(Chess chess)
    {
        base.Exit(chess);
        if (chess?.stateController?.preState?.state != null &&
            chess.stateController.preState.state.stateName == StateName.SkillState)
        {
            chess.skillController.activeSkill?.ReturnCD();
        }
    }

    public override State Clone()
    {
        var ans = new FeverState();
        ans.stateName = stateName;
        return ans;
    }
}
