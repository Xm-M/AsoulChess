using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
/// <summary>
/// 要不然默认weapon的动画不是loop的 然后如果攻击间隔小于等于0就不要重新播放动画代表这是loop动画
/// 至少在PVZ这个游戏里 他应该是loop的 
/// </summary>
[Serializable]
public class AttackController :  Controller
{
    [HideInInspector] public Chess master;//这个就是武器的拥有者
    public Transform weaponPos;//这个的主要作用是设计类武器可以找到子弹发射的位置
    [SerializeReference]
    public Weapon weapon;
    Timer timer;//这个是计时器
    
    public virtual void InitController(Chess chess)
    {
        master=chess;
        //weapon.InitWeapon(this);
    }
    public void ChangeWeapon(Weapon newWeapon)
    {
         this.weapon=newWeapon;
    }

    public virtual void WhenControllerEnterWar()
    {
        //attackSpeed = master.propertyController.GetAttackInterval();
        weapon.InitWeapon(this);        
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
        master.animator.Play(atk);
        if (weapon.interval>0)
        {
            timer = GameManage.instance.timerManage.AddTimer(
                () =>
                {
                //Debug.Log("attack");
                master.animator.Play(atk);
                }
                , weapon.interval/master.propertyController.GetAccelerate());
        }
    }
    public virtual void StopAttack()
    {
        if(timer!=null)
            timer.Stop();
        timer=null;
    }
    public virtual void TakeDamages()
    {
        weapon.TakeDamage(master);
    } 
}
public interface IAttackFunction
{
    public void Attack(Chess user, List<Chess> targets);
}
public interface IInitWeapon
{
    public void InitWeapon(AttackController weapon);
}

public class Weapon
{
    [SerializeReference]
    protected IInitWeapon initWeapon;
    [SerializeReference]
    protected IFindTarget FindTarget;
    [SerializeReference]
    protected IAttackFunction attack;
    List<Chess> target;//这个是攻击目标
    public float interval;//攻击间隔 如果是0或者-1就表明是loop动画

    public Weapon()
    {
        initWeapon = null;
        FindTarget=new StraightFindTarget();
        attack = new CloseAttack();
    }
    public void InitWeapon(AttackController attackController)
    {
        target = new List<Chess>();
        initWeapon?.InitWeapon(attackController);
    }
    public int FindEnemy(Chess user)
    {
        FindTarget.FindTarget(user, target);
        return target.Count;
    }
    public void TakeDamage(Chess user)
    {
        if(FindEnemy(user)>0)
            attack.Attack(user, target);
        
    }
}
