using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���b���ܵ��ô����Ǹ������׵ļ��ܼ�CD
/// </summary>
public class Passive_SweetPotatoMine : ISkill
{
    public float reduceRate;

    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        return false;
    }

    public  void InitSkill(Chess chess)
    {
 
        
    }

    public void LeaveSkill(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public void UseSkill(Chess user)
    {
        int level = PowerBarPanel.GetView<SweetBar>().GetStage();
        user.propertyController.ChangeSpellHaste(level * reduceRate);
    }
}
