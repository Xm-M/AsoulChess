using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_SweetPotatoMine : ISkillPassive
{
    public float reduceRate;
    public string checkAnim;
    float baseColdDown;

    public  void InitSkill(Skill chess)
    {
        baseColdDown = chess.coldDown;
        int level = PowerBarPanel.GetView<SweetBar>().GetStage();
        chess.coldDown=(1-reduceRate*level)*baseColdDown;
    }

    public  void OverSkill(Skill user)
    {
        user.coldDown = baseColdDown;
    }
}
