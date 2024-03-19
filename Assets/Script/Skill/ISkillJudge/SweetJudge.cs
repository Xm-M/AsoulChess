using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetJudge :ISkillJudge
{
    public float sweetCost;

    public bool IfSkillOver(Skill skill)
    {
        return PowerBarPanel.GetView<SweetBar>().ConsumeSweet(sweetCost);
    }
}
