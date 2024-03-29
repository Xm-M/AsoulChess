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
    protected IInitWeapon initWeapon;
    [SerializeReference]
    protected IFindTarget FindTarget;
    [SerializeReference]
    protected IAttackFunction attack;
    public bool loop=true;
    float attackSpeed;
    Timer timer;
    [HideInInspector]public Chess master;//这个就是武器的拥有者
    List<Chess> target;//这个是攻击目标
    Animator animator;
    IFindTarget baseFindtarget;
    IAttackFunction baseAttack;
    public virtual void InitController(Chess chess)
    {
        master=chess;
        animator = master.animator;
        target = new List<Chess>();
        baseFindtarget=FindTarget;
        baseAttack = attack;
    }
    public void ChangeWeapon(IAttackFunction attack,IFindTarget findTarget)
    {
        this.attack = attack;
        this.FindTarget = findTarget;
    }
    public virtual void WhenControllerEnterWar()
    {
        attackSpeed = master.propertyController.GetAttackInterval();
        initWeapon?.InitWeapon(this);
    }
    public virtual void WhenControllerLeaveWar()
    {
        timer?.Stop();
        timer = null;
        FindTarget = baseFindtarget;
        attack = baseAttack;
    }
    /// <summary>
    /// attack是播放动画，Takedamage是实际上造成伤害（或者发射子弹）是绑定在动画上的
    /// </summary>
    /// <param name="atk"></param>
    public virtual void Attack(string atk="attack") {
        animator.Play(atk);
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
        float newSpeed = master.propertyController.GetAttackInterval();
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
public interface IInitWeapon
{
    public void InitWeapon(Weapon weapon);
}
