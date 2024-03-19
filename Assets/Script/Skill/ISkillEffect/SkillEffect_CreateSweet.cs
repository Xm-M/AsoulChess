using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_CreateSweet : ISkillEffect
{
    float CreateNum;
    public void SkillEffect(Skill skill)
    {
        PowerBarPanel.GetView<SweetBar>().UpdateBar(CreateNum);
    }
}
