using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
/// <summary>
/// 要不然默认weapon的动画不是loop的 然后如果攻击间隔小于等于0就不要重新播放动画代表这是loop动画
/// 至少在PVZ这个游戏里 他应该是loop的 
/// 那就不需要在这改 而是要在其他地方改
/// </summary>
[Serializable]
public class AttackController :  Controller
{
    [HideInInspector] public Chess master;//这个就是武器的拥有者
    public Transform weaponPos;//这个的主要作用是设计类武器可以找到子弹发射的位置
    [SerializeReference]
    public Weapon weapon;
    [HideInInspector]public UnityEvent<Chess> OnAttack;
    public bool attackOver;
    Timer timer;//这个是计时器
    bool loop;
    public virtual void InitController(Chess chess)
    {
        master=chess;
        //weapon.InitWeapon(this);
        if (weapon == null) weapon = new Weapon_Sample();
    }
    public void ChangeWeapon(Weapon newWeapon)
    {
         this.weapon=newWeapon;
    }

    public virtual void WhenControllerEnterWar()
    {
        //attackSpeed = master.propertyController.GetAttackInterval();
        attackOver=true;
        weapon.InitWeapon(this);        
    }
    public virtual void WhenControllerLeaveWar()
    {
        timer?.Stop();
        timer = null;
        OnAttack.RemoveAllListeners();
    }
    /// <summary>
    /// attack是播放动画，Takedamage是实际上造成伤害（或者发射子弹）是绑定在动画上的
    /// </summary>
    /// <param name="atk"></param>
    public virtual void Attack(string atk="attack") {
        //master.animator.Play(atk);
        master.animatorController.PlayAttack();
        loop=true;
        //这一段是重复播放attack动画 
        if (weapon.GetInterval()>0)
        {
            loop = false;
            timer = GameManage.instance.timerManage.AddTimer(
                () =>
                {
                //Debug.Log("attack");
                //master.animator.Play(atk);
                attackOver=true;
                master.animatorController.PlayAttack();
                }
                , weapon.GetInterval()/master.propertyController.GetAccelerate(),true);
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
        //
        //也就是说这里是触发onattack的函数对吧
        //这里也是绑定在攻击动画上触发的函数
        weapon.TakeDamage(master);
        OnAttack?.Invoke(master);
        if(!loop)
            attackOver=false;
        //EventController  
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

//public interface Weapon
//{ 
//    [SerializeReference]
//    protected IInitWeapon initWeapon;
//    [SerializeReference]
//    protected IFindTarget FindTarget;
//    [SerializeReference]
//    protected IAttackFunction attack;
//    List<Chess> target;//这个是攻击目标
//    public float interval;//攻击间隔 如果是0或者-1就表明是loop动画
//    public Weapon()
//    {
//        initWeapon = null;
//        FindTarget=new StraightFindTarget();
//        attack = new CloseAttack();
//    }
//    public void InitWeapon(AttackController attackController)
//    {
//        target = new List<Chess>();
//        initWeapon?.InitWeapon(attackController);
//    }
//    public int FindEnemy(Chess user)
//    {
//        FindTarget.FindTarget(user, target);
//        return target.Count;
//    } 
//    public void TakeDamage(Chess user)
//    {
//        if(FindEnemy(user)>0)
//            attack.Attack(user, target);
//    
//    }
//}  
public interface Weapon
{
    public float GetInterval();
    public void InitWeapon(AttackController attackController);

    public int FindEnemy(Chess user);

    public void TakeDamage(Chess user);
    
}