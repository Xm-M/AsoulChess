using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SocialPlatforms;
/// <summary>
/// MMK每5次攻击会使用一次技能 说明是攻回
/// 只要attack—>skilllState的时候记得带上是否播放完毕的就好
/// 那mmk的技能还挺好做的
/// mmk 125 攻击20 会自增压力值
/// 
/// </summary>
public class SKillEffect_MMK : ISkillEffect { 
    [LabelText("坚毅buff")]
    public string buffName;
    public DamageMessege DM;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        UseSkill(user,config,targets);

    }

    public void UseSkill(Chess user, SkillConfig config, List<Chess> targets) 
    {
        bool takeDamage=false;
        foreach(var enemy in targets)
        {
            if (enemy != null && enemy.CompareTag(user.tag))
            {
                enemy.buffController.buffDic.TryGetValue(buffName, out Buff baseBuff);
                Buff_MMKFirm buff = baseBuff as Buff_MMKFirm;
                if (buff != null)
                {
                    buff.TriggerBuff();
                }
            }else if (enemy != null && !enemy.CompareTag(user.tag)&&!takeDamage)
            {
                int stress = 0;
                enemy.skillController.context.TryGet<int>("stress", out stress);
                if(stress != 0)
                {
                    takeDamage = true;
                    DM.damage = stress * config.baseDamage[0];
                    DM.damageFrom = user;
                    DM.damageTo = enemy;
                    user.propertyController.TakeDamage(DM);
                }
            }
        }
    }

}
 
public class Buff_MMKFirm : TimeBuff
{
    [LabelText("不屈buff")]
    [SerializeReference]
    public Buff_Unyielding buff_Unyielding;
    //public float buffBase = 40;
    public int maxBuffCount = 5;
    [LabelText("不屈冷却时间")]
    public float coldTime=15;//不屈的冷却时间
    int buffCount; //层数 
    bool cold;
    Chess user;
    Timer timer;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        buffCount = 1;
        cold = true;
        user = target;
        target.propertyController.onSetDamage.AddListener(OnGetDamage);
    } 
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.onSetDamage.RemoveListener(OnGetDamage);
        timer?.Stop();
        timer = null;
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if(buffCount<maxBuffCount&&cold)
            buffCount++;
    }
    public void TriggerBuff()
    {
        target.propertyController.Heal(buffCount * user.propertyController.GetAttack());
        Debug.Log("治疗了" + target.name);
    }
    public void OnGetDamage(DamageMessege dm)
    {
        if ((dm.damage >= dm.damageTo.propertyController.GetHp() && cold))
        {
            dm.damage = 0;
            cold = false;
            Debug.Log("触发不屈");
            UIManage.GetView<DamagePanel>().ShowText(dm, "Unyielding!", Color.white);
            buff_Unyielding.continueTime = buffCount;
            user.buffController.AddBuff(buff_Unyielding);
            timer = GameManage.instance.timerManage.AddTimer(OnTimerOver, coldTime);
            buffCount = 0;
        }
    }
    public void OnTimerOver()
    {
        cold = true;
        timer = null;
    }
}
/// <summary>
/// 不屈buff
/// </summary>
public class Buff_Unyielding : TimeBuff
{
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.onSetDamage.AddListener(OnGetDamage);
        Debug.Log(continueTime);
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.onSetDamage.RemoveListener(OnGetDamage);
    }
    public void OnGetDamage(DamageMessege dm)
    {
        dm.damage = 0;
        UIManage.GetView<DamagePanel>().ShowText(dm, "Unyielding!", Color.white);
        
    }
}