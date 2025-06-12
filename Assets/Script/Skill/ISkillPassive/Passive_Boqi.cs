using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_Boqi : ISkill
{
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        return true;
    }

    public void InitSkill(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public void LeaveSkill(Chess user)
    {
        user.propertyController.onSetDamage.RemoveListener(OnSetDamage);
    }

    public void UseSkill(Chess user)
    {
        user.propertyController.onSetDamage.AddListener(OnSetDamage);
    }

    public void OnSetDamage(DamageMessege dm)
    {
        if ((dm.damageType & DamageType.Real) == 0)
        {
            float n = Random.Range(0, 1);
            if (n < dm.damageTo.propertyController.GetCrit())
            {
                Debug.Log("»áÐÄ·ÀÓù£¡");
                float critDamege = dm.damageTo.propertyController.GetCritDamage();
                dm.damage *= ((critDamege-1) / (critDamege));
            }
        }
    }

    public void WhenEnter(Chess user)
    {
        //throw new System.NotImplementedException();
    }
}
