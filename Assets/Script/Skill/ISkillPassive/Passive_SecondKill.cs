using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_SecondKill : ISkill
{
    public float possibilities=0.022f;
    public bool IfSkillReady(Chess user)
    {
        return false;
    }

    public void InitSkill(Chess user)
    {
         
    }

    public void LeaveSkill(Chess user)
    {
        //throw new System.NotImplementedException();
        //这里就不需要清除了 因为Property会在死亡的时候自动清除
    }
    public void Seckill(DamageMessege DM)
    {
        float lucky = PowerBarPanel.GetView<LuckyBar>().GetBarValue();
        float realP = 1 - (1 - possibilities) * (1 + lucky / 100);
        float p=Random.Range(0,1);
        if (p > realP)
        {
            DM.damage = DM.damageTo.propertyController.GetMaxHp() * 100;
            DM.damageType = DamageType.Real;
        }
    }
    public void UseSkill(Chess user)
    {
        user.propertyController.onTakeDamage.AddListener(
            Seckill);
    }
}
