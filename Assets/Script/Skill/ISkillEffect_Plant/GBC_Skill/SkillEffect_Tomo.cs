using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// 
/// </summary>
public class SkillEffect_Tomo : ISkillEffect
{
    public Buff_Tomo_Correct buffTomo;
    IFindAll_Circle find;
    public DamageMessege DM;// buff也加在这里面 造成切割伤害  
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (find == null) find = new IFindAll_Circle();
        //寻找周围所有的单位并施加矫正buff
        find.FindTarget(user, targets);
        DM.damageFrom = user;
        foreach(var chess in targets)
        {
            DM.damageTo = chess;
            DM.damage = user.propertyController.GetAttack() * config.baseDamage[0];
            user.propertyController.TakeDamage(DM);
        }
    }
}
/// <summary>
/// 这个是增加矫正buff 基础时间延长 然后基础数值增加 
/// </summary>
public class Buff_Tomo_Correct : TimeBuff
{
    
    public float stressFrequence=5;
    public float extraAttack=0.1f;
    public int extraStress=1;
    public int baseStress=5;
    public float baseAttack=0.5f;
    public int maxCount = 5;
    int index;
    Timer timer;
    float currentAttack;
    int currentStress;  
    Chess user;
    public override void BuffEffect(Chess target)
    {
        user = target;
        index = 0;
        currentAttack = 0;
        currentStress = baseStress;
        currentAttack+=baseAttack;
        timer = GameManage.instance.timerManage.AddTimer(AddStress, stressFrequence, true);
        target.propertyController.ChangeAttack(currentAttack);
        base.BuffEffect(target);
        
    }
    public void AddStress()
    {
        int stress = 0; 
        user.skillController.context.TryGet<int>("stress",out stress);
        stress += currentStress;
        user.skillController.context.Set<int>("stress", stress);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeAttack(-currentAttack);
        timer.Stop();
        timer = null;
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        index += 1;
        if (index < maxCount)
        {
            target.propertyController.ChangeAttack(extraAttack);
            currentAttack += extraAttack;
            currentStress += extraStress;
        }
    }
}