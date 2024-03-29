using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_VirusInfect : ISkillPassive
{
    public string targetTag;
    public PropertyCreator creator;
    public void InitSkill(Skill user)
    {
        Debug.Log("初始化感染");
        user.user.propertyController.onTakeDamage.AddListener(Infect);
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
    public void OverSkill(Skill user)
    {
        //throw new System.NotImplementedException();
    }
}
