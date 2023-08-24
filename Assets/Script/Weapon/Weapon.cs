using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class Weapon : MonoBehaviour,Controller
{
    public Chess master;//这个就是武器的拥有者
    public Chess target;//这个是攻击目标
    public Animator animator;
    public DamageMessege DM;
    protected float timer = 0;//攻击计时器
    protected bool ifAttack = false;//是否攻击
    public UnityEvent<Weapon> OnWeaponAttack;
    public virtual void InitController(Chess chess)
    {
        master=chess;
    }
    public virtual void WhenControllerEnterWar()
    {
        ifAttack = false;
        timer = 0;
    }
    public virtual void WhenControllerLeaveWar()
    {

    }
    public virtual void Attack(string atk="attack") {
        
        if (ifAttack == false)
        {
            animator.Play(atk);
            ifAttack = true;
        }
    }
    public virtual void TakeDamage(Chess target)
    {
        if (target == null) return;
        OnWeaponAttack?.Invoke(this);
        DM.damageFrom = master;
        DM.damageTo = target;
        DM.damage = master.propertyController.GetAttack();
        master.propertyController.TakeDamage(DM);
    }
    public virtual void TakeDamages()
    {
        TakeDamage(target);
    }
    public virtual void WeaponUpdate()
    {
        if (ifAttack)
        {
            timer += Time.deltaTime;
            if (timer > master.propertyController.GetAttackSpeed())
            {
                ifAttack=false;
                timer = 0;
                Attack();
            }
        }
    }
    public virtual void FindTarget(){
        Chess chess=null;
        int minDistance;
        List<Chess> enemyTeam=ChessFactory.instance.FindEnemyList(tag);
        if (enemyTeam.Count > 0)
        {
            chess =null;
            minDistance = 100;
            for (int i = 0; i < enemyTeam.Count; i++){
                int dis=MapManage.instance.Distance(master.moveController. standTile,enemyTeam[i].moveController.standTile);
                if (!enemyTeam[i].IfDeath&&dis < minDistance)
                {
                    minDistance = dis;
                    chess = enemyTeam[i];
                }else  if(dis==minDistance&& MapManage.instance.RealDis(chess.moveController.standTile.mapPos,master.moveController. standTile.mapPos)>
                    MapManage.instance.RealDis(enemyTeam[i].moveController.standTile.mapPos,master.moveController. standTile.mapPos)) {
                    chess=enemyTeam[i];
                }
            }
        }//
        target = chess;
    }
    public virtual bool IfInRange()
    {
        if(target&&MapManage.instance.Distance(master.moveController.standTile, target.moveController.standTile) <= master.propertyController.GetAttackRange())
        {
            return true;
        }
        return false;
    }   
}
 