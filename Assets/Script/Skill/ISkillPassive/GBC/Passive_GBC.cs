using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.ParticleSystemJobs;
/// <summary>
/// 这种被动一般就是上个buff啦
/// MMK的自上   
/// 首先 buff效果是每秒给mmk增加压力值 当压力值大于n的时候 mmk就会去世
/// mmk只有在nina在场时才能使用技能
/// </summary>
public class Passive_GBC : ISkillEffect
{
    [SerializeReference]
    public Buff stressBuff;
    public Chess user;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        this.user = user;
        user.buffController.AddBuff(stressBuff);
    }
}
/// <summary>
/// 也就是说所有压力相关buff都要监听stress词条咯 
/// 压力过大会死亡 要做一个骷髅头的特效 代表压力过大死亡
/// </summary>
/// 
public class Buff_StressBuff_Death : Buff
{
    [LabelText("压力阈值")]
    public int stressLimit=100;//压力阈值
    [LabelText("额外压力值")]
    public int extraStress=1;
    [LabelText("压力值显示器")]
    public GameObject stressTextPre;

    protected int stress;
    protected Chess user;
    protected GameObject stressTextGO;
    protected TMP_Text stressText;
    public Buff_StressBuff_Death()
    {
        buffName = "压力";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        stress = 0;
        this.user = target;
        user.skillController.context.AddEvent(OnStressChange);
        if (stressTextPre == null)
        {
            stressTextPre = Resources.Load<GameObject>("Effect/StressFX");
        }
        if (stressTextPre != null)
        {
            stressTextGO = ObjectPool.instance.Create(stressTextPre);
            stressTextGO.transform.SetParent(target.transform);
            stressTextGO.transform.localPosition = Vector3.zero;
            stressText = stressTextGO.GetComponentInChildren<TMP_Text>();
        }
    }
    public virtual void OnStressChange()
    {
        //stress = 0;
        user.skillController.context.TryGet<int>("stress", out stress);
        if (stressText != null)
        {
            stressText.text = Mathf.Min(999, stress).ToString();
        }
        if (stress > stressLimit)
        {
            DamageMessege DM = new DamageMessege();
            DM.damageTo = user;
            DM.damageFrom = user;
            DM.damage = user.propertyController.GetMaxHp() * 10;
            Debug.Log("紫砂" + DM.damage);
            DM.damageType = DamageType.Real;
            user.propertyController.GetDamage(DM);
           
            //user.Death();
        }
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        stress += (resetBuff as Buff_StressBuff_Death).extraStress;
        user.skillController.context.Set<int>("stress",stress);
        //Debug.Log(stress);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        user.skillController.context.RemoveEvent(OnStressChange);
        if (stressTextGO != null)
        {
           
            ObjectPool.instance.Recycle(stressTextGO);
            stressTextGO = null;
            stressText = null;
        }
    }
}
/// <summary>
/// MMK的话 首先有个自然增长的压力值   
/// 其次就是如何判断小孩姐在不在场呢
/// nina在场的时候 mmk不再会因为压力值过大死亡-将阈值设置为与nina相同
/// </summary>
public class Buff_StressBuff_MMK : Buff_StressBuff_Death
{
    public PropertyCreator Nina;
    public float delayTime = 1;
    public int addStress = 1;//每秒+1
    Timer timer;
    bool nina;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        timer = GameManage.instance.timerManage.AddTimer(AddStress, delayTime, true);
    }
    public void AddStress()
    {
        stress += addStress;
        user.skillController.context.Set<int>("stress", stress);
        foreach (var chess in GameManage.instance.chessTeamManage.GetTeam(user.tag))
        {
            if (chess.propertyController.creator == Nina)
            {
                stressLimit += addStress;
                return;
            }
        }
         
    }
 
    public override void BuffOver()
    {
        base.BuffOver();
        timer.Stop();
        timer = null;
    }
}



/// <summary>
/// Rupa的效果是受到伤害增加压力值 压力值增加数量=受到伤害/10
/// </summary>
public class Buff_StressBuff_Rupa: Buff_StressBuff_Death
{
    float damagePool;
    public override void WriteExtraToSaveData(BuffSaveData data)
    {
        base.WriteExtraToSaveData(data);
        if (data == null) return;
        data.SetExtra("DamagePool", damagePool);
    }
    public override void RestoreExtraFromSaveData(BuffSaveData data)
    {
        base.RestoreExtraFromSaveData(data);
        if (data == null) return;
        damagePool = data.GetExtraFloat("DamagePool", 0);
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        user.propertyController.onGetDamage.AddListener(OnGetDamage);
    }
    public void OnGetDamage(DamageMessege DM)
    {
        damagePool+=DM.damage;
        if (damagePool > 10)
        {
            stress += 1;
            user.skillController.context.Set<int>("stress", stress);
            damagePool -= 10;
        }
        
    }
                                     
    public override void BuffOver()
    {
        base.BuffOver();
        user.propertyController.onGetDamage.RemoveListener(OnGetDamage);
    }
}
/// <summary>
/// 486的话 有个自增的压力 压力超过阈值就会生成阳光 
/// 每秒会分担周围友方的压力值                        
/// </summary>
public class Buff_StressBuff_486 : Buff_StressBuff_Death
{
    public float delayTime = 1;
    public float checkDelay=2;
    public int addStress = 1;//每秒+1
    //public DamageMessege DM;
    Timer timer;
    Timer checkTimer;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        timer = GameManage.instance.timerManage.AddTimer(AddStress, delayTime, true);
        checkTimer = GameManage.instance.timerManage.AddTimer(ShareStress, checkDelay, true);
    }
    public void AddStress()
    {
        stress += addStress;
        user.skillController.context.Set<int>("stress", stress);
    }
    public void ShareStress()
    {
        int extra=0;
        //DM.damageFrom = user;
        foreach (var friend in GameManage.instance.chessTeamManage.GetTeam(user.tag))
        {
            if (Vector2.Distance(friend.transform.position, user.transform.position) < user.propertyController.GetAttackRange())
            {
                if (friend == user) continue;
                int fstress = 0;
                friend.skillController.context.TryGet<int>("stress",out fstress);
                extra += fstress / 2;
                //DM.damageTo = friend;
                //DM.damage = fstress * 5;
                //user.propertyController.TakeDamage(DM);
                friend.skillController.context.Set<int>("stress", fstress/2);
            }
        }
        stress += extra;
        user.skillController.context.Set<int>("stress", stress);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        timer.Stop();
        timer = null;
        checkTimer.Stop();
        checkTimer = null;
    }
}
/// <summary>
/// 如果场上存在rupa 则将多余的压力值传递给Rupa
/// </summary>
public class Buff_StressBuff_Tomo : Buff_StressBuff_Death
{
    public PropertyCreator Rupa;
    public GameObject spineRainPre;
    ParticleSystem particle;
    public override void BuffEffect(Chess target)
    {
         
        base.BuffEffect(target);
        user.skillController.context.OnValueChange.AddListener(SearchRupa);
        if (particle == null)
        {
            GameObject rain  =ObjectPool.instance.Create(spineRainPre);
            rain.transform.SetParent(target.transform);
            rain.transform.localPosition = Vector2.zero;
            particle = rain.GetComponent<ParticleSystem>();
            target.skillController.onUseSkill.AddListener(ShowRain);
            target.skillController.onSkillOver.AddListener(StopRain);
        }

    }
    public void ShowRain(Chess user)
    {
        particle.Play();
    }
    public void StopRain(Chess user)
    {
        particle.Stop();
    }


    public void SearchRupa()
    {
        user.skillController.context.TryGet<int>("stress", out stress);
        if (stress > 50)
        {
            float dis = 100;
            Chess rupa = null;
            foreach (var friend in GameManage.instance.chessTeamManage.GetTeam(user.tag))
            {
                float d = Vector2.Distance(friend.transform.position, user.transform.position);
                if (friend.propertyController.creator == Rupa && d < dis)
                {
                    dis = d;
                    rupa = friend;
                }
            }
            if (rupa != null)
            {
                stress -= 50;
                user.skillController.context.Set<int>("stress", 50);
                int rstress = 0;
                rupa.skillController.context.TryGet<int>("stress", out rstress);
                rupa.skillController.context.Set<int>("stress", rstress + stress);
            }
        }

    }
    
    public override void BuffOver()
    {
        base.BuffOver();
        user.skillController.context.OnValueChange.RemoveListener(SearchRupa);
        if (particle != null)
        {
            target.skillController.onUseSkill.RemoveListener(ShowRain);
            target.skillController.onSkillOver.RemoveListener(StopRain);
            ObjectPool.instance.Recycle(particle.gameObject);
            particle = null;
        }
    }
}
/// <summary>
/// 井芹仁菜的压力上限极高 当压力值大于x时 下次攻击会消耗所有的额外压力值 对下次攻击命中的首个目标造成额外压力值的伤害
/// 当压力值大于200时 每次攻击会对自己造成伤害
/// </summary>
public class Buff_StressBuff_Nina : Buff_StressBuff_Death
{
    [LabelText("压力限制")]
    public int attackLimit=100;
    public GameObject stressIconPre;//十字压力图标
    GameObject stressIcon;  
    [SerializeReference]
    [LabelText("一次性压力buff")]
    public Buff_BaseValueBuff_Attack attackBuff;
    [LabelText("倍率")]
    public float stressRate=1;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.equipWeapon.OnAttack.AddListener(OnAttack);
        target.skillController.context.OnValueChange.AddListener(OnValueChange);
        user.propertyController.onTakeDamage.AddListener(OnTakeDamage);
        stressIcon = ObjectPool.instance.Create(stressIconPre);
        stressIcon.transform.SetParent(user.transform);
        stressIcon.transform.localPosition = Vector3.zero;
        stressIcon.SetActive(false);
    }
    public void OnValueChange()
    {
        int rstress = 0;
        user.skillController.context.TryGet<int>("stress", out rstress);
        if (rstress >= attackLimit)
        {
            if (stressIcon != null) stressIcon.SetActive(true);

        }
        else
        {
            if (stressIcon != null) stressIcon.SetActive(false);
        }
    }
    public void OnAttack(Chess chess)
    {
        int rstress = 0;
        user.skillController.context.TryGet<int>("stress", out rstress);
        if (rstress > attackLimit)
        {
            int extraAttack = rstress - attackLimit;
            user.skillController.context.Set("stress", attackLimit);
            attackBuff.extraAttack = extraAttack*stressRate;
            user.buffController.AddBuff(attackBuff);
            
        }
    }
    public void OnTakeDamage(DamageMessege DM)
    {
        user.buffController.TryOverBuff(attackBuff);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.equipWeapon.OnAttack.RemoveListener(OnAttack);
        target.skillController.context.OnValueChange.RemoveListener(OnValueChange);
        user.propertyController.onTakeDamage.RemoveListener(OnTakeDamage);
        ObjectPool.instance.Recycle(stressIcon);
        stressIcon = null;
        
    }
 
}