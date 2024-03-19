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
    public bool loop=true;
    float attackSpeed;
    Timer timer;
    Chess master;//这个就是武器的拥有者
    List<Chess> target;//这个是攻击目标
    Animator animator;
    public virtual void InitController(Chess chess)
    {
        master=chess;
        animator = master.animator;
        target = new List<Chess>();
    }
    public virtual void WhenControllerEnterWar()
    {
        attackSpeed = master.propertyController.GetAttackSpeed();
    }
    public virtual void WhenControllerLeaveWar()
    {
        timer?.Stop();
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
                //Debug.Log("attack");
                animator.Play(atk);
            }
            ,attackSpeed,loop);
    }
    public virtual void StopAttack()
    {
        timer.Stop();
        timer=null;
    }


    public void StartAttack(string atk)
    {
        animator.Play(atk);
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
