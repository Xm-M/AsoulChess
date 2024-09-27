using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个效果是生化幽灵感染的效果
/// </summary>
public class Passive_VirusInfect : ISkill
{
    public string targetTag;
    public PropertyCreator creator;
    public void InitSkill(Chess user)
    {
        //Debug.Log("初始化感染");
        user. propertyController.onTakeDamage.AddListener(Infect);
    }
    public void Infect(DamageMessege dm)
    {
        //Debug.Log("infect");
        if (dm.damageTo.propertyController.creator.plantTags.Contains(targetTag))
        {
            ChessTeamManage.Instance.CreateChess(creator, dm.damageTo.moveController.standTile, dm.damageFrom.tag);
            //Debug.Log("感染成功" + dm.damageTo.name);
            dm.damageTo.Death();

        }
    }

    public void UseSkill(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public void LeaveSkill(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public bool IfSkillReady(Chess user)
    {
        return false;
        //throw new System.NotImplementedException();
    }
}
