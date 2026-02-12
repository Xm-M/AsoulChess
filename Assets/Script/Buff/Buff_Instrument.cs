using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 和乐队相关的buff 都被放在这个文件里了
/// </summary>
 
public class Buff_Vocal : Buff
{
    public int maxCount;
    public GameObject extraBullet;
    //这里用添加可能施加的随机buff 
    //public List<Buff> buffs;
    int count;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.equipWeapon.OnAttack.AddListener(WhenTakeDamage);
    }

    public void WhenTakeDamage(Chess chess)
    {
        count++;
        if(count == maxCount)
        {
            count = 0;
            Bullet b=ObjectPool.instance.Create(extraBullet).GetComponent<Bullet>();
            b.InitBullet(chess, chess.equipWeapon.weaponPos.position, null, chess.transform.right);
            ////这里要给takebuff 添加随机buff
            //b.Dm.takeBuff=
            ////如果有多种颜色的话 就用这个吧
            //b.GetComponent<Animator>().SetInteger("type",0);
        }
    }

    public override void BuffOver()
    {
        base.BuffOver();
        target.equipWeapon.OnAttack.RemoveListener(WhenTakeDamage);
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
    }
}
/// <summary>
/// 贝斯手 buff 效果是获得一些数值 以及
/// </summary>
public class Buff_Bass : Buff
{
    public float extraHpMax;//额外最大生命值
    public float extraHpSteal;//额外生命偷取
    public int extraSize;//额外体型 应该是0/0/1/1/2 也就是说如果开5贝斯 凉是可以吃巨人的
    public float coldDowm=50;
    [SerializeReference]
    public Buff_BassHide buff;
    Timer timer;
    bool cold;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeHPMax(extraHpMax);
        //target.propertyController.ChangeHp(extraHpMax);
        target.propertyController.ChangeLifeSteeling(extraHpSteal);
        target .propertyController.ChangeSize(extraSize);
        target.propertyController.onSetDamage.AddListener(OnGetDamage);
        cold = true;
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
    }
    public void OnGetDamage(DamageMessege dm)
    {
        if (dm.damage>target.propertyController.GetHp()&&cold)
        {
            cold = false;
            dm.damage = 0;
            target.buffController.AddBuff(buff);
            timer = GameManage.instance.timerManage.AddTimer(() => cold = true,coldDowm);
        }
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeHPMax(-extraHpMax);
        //target.propertyController.ChangeHp(extraHpMax);
        target.propertyController.ChangeLifeSteeling(-extraHpSteal);
        target.propertyController.ChangeSize(-extraSize);
        target.propertyController.onSetDamage.RemoveListener(OnGetDamage);
        if (timer != null)
        {
            timer.Stop();
            timer = null;
        }
    }
}
/// <summary>
/// 贝斯隐身buff
/// </summary>
public class Buff_BassHide : TimeBuff
{
    //public float continueTime = 2;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.UnSelectable();
        target.animatorController.ChangeColor(new Color(1, 1, 1, 0.5f));
        
    }

    public override void BuffOver()
    {
        base.BuffOver();
        target.ResumeSelectable();
        target.animatorController.ChangeColor(Color.white);
    }
}

/// <summary>
/// 吉他的buff  就是加双爆的  
/// 这个就是很单纯的buff了  
/// </summary>
public class Buff_Guitar : Buff
{
    public float extraCrit;//额外暴击率
    public float extraCritDamage;//额外暴击伤害A
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeCrit(extraCrit);
        target.propertyController.ChangeCritDamage(extraCritDamage);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeCrit(-extraCrit);
        target.propertyController.ChangeCritDamage(-extraCritDamage);
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
    }
    
}
/// <summary>
/// 键盘手的buff 给全体友军使用的。但是键盘手会获得三倍效果
/// 所以说键盘手的技能都跟双抗有关
/// ?壮壮应该要被设计成前排了 
/// </summary>
public class Buff_KeyBoard : Buff
{
    public float extraArmor;//额外护甲
    //public float extraMagicDefence; 这个游戏里没有魔抗
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (target.propertyController.creator.plantTags.Contains("键盘"))
        {
            target.propertyController.ChangeAR(extraArmor * 3);
        }
        else
        {
            target.propertyController.ChangeAR(extraArmor);
        }
    }
    public override void BuffOver()
    {
        base.BuffOver();
        if (target.propertyController.creator.plantTags.Contains("键盘"))
        {
            target.propertyController.ChangeAR(-extraArmor * 3);
        }
        else
        {
            target.propertyController.ChangeAR(-extraArmor);
        }
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
    }
}// 