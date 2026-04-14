using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_ZombieKing_ChangeIdleTransition : Transition
{
    public float changeStateTime=10;
    float t = 0;
    bool stand = true;
    public override bool ifReach(Chess chess)
    {
        // 由 Skill_ZombieKingBoss 统一管理 stand / 站立俯身循环，避免与 Boss 技能抢写 context
        if (chess.skillController != null && chess.skillController.activeSkill is Skill_ZombieKingBoss)
            return false;

        t += Time.deltaTime;
        if (t >= changeStateTime)
        {
            t = 0;
            stand = !stand;
            chess.skillController.context.Set<bool>("stand",stand);
            return true; 
        }
        return false;
    }

    public override Transition Clone()
    {
        Transition clone = new Transition_ZombieKing_ChangeIdleTransition();
        return clone;
    }
}
