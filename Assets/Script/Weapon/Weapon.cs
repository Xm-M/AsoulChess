using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
[Serializable]
public class Weapon :  Controller
{
    public Transform weaponPos;
    public UnityEvent<Weapon> OnWeaponAttack;
    [SerializeReference]
    public IFindTarget FindTarget;
    [SerializeReference]
    public IAttackFunction attack;
    float attackSpeed;
    Timer timer;
    Chess master;//这个就是武器的拥有者
    List<Chess> target;//这个是攻击目标
    Animator animator;
    public virtual void InitController(Chess chess)
    {
        master=chess;
        animator = master.animator;
    }
    public virtual void WhenControllerEnterWar()
    {
        attackSpeed = master.propertyController.GetAttackSpeed();
    }
    public virtual void WhenControllerLeaveWar()
    {
        timer.Stop();
        timer = null;
    }
    /// <summary>
    /// attack是播放动画，Takedamage是实际上造成伤害（或者发射子弹）是绑定在动画上的
    /// </summary>
    /// <param name="atk"></param>
    public virtual void Attack(string atk="attack") {

        timer = GameManage.instance.timerManage.AddTimer(
            () =>
            {
                animator.Play(atk);
            }
            ,attackSpeed,true);
    }
    public void StartAttack(string atk)
    {
        animator.Play(atk);
        //这里要改变攻击速度;
        //动画的播放速度应该在property改变
        if (target.Count > 0)
        {
            master.Flap(target[0].transform);
        }
        float newSpeed = master.propertyController.GetAttackSpeed();
        if (newSpeed != attackSpeed)
        {
            attackSpeed = newSpeed;
            timer.ChangeDelayTime(attackSpeed);
        }
    }
    public virtual void TakeDamages()
    {
        if (IfFindEnemy())
        {
            attack.Attack(master, target);
        }
        else
        {
            //这里是负责状态转换的
        }
    } 
    public bool IfFindEnemy()
    {
        FindTarget.FindTarget(master, target);
        return target.Count > 0;
    }
}
public interface IAttackFunction
{
    public void Attack(Chess user, List<Chess> targets);
}
public class CloseAttack : IAttackFunction
{
    public void Attack(Chess user, List<Chess> targets )
    {
        if (targets != null && targets.Count > 0)
        {
            DamageMessege DM;
            float damage = user.propertyController.GetAttack();
            for (int i = 0; i < targets.Count; i++)
            {
                if (!targets[i].IfDeath)
                {
                    DM = new DamageMessege();
                    DM.damageFrom = user;
                    DM.damageTo = targets[i];
                    DM.damage = damage;
                    user.propertyController.TakeDamage(DM);
                }
            }
             
        }        
    }
}
 