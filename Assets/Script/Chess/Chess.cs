using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Reflection;
using System;
using UnityEngine.Events;

public class Chess : MonoBehaviour
{

    [HideInInspector]public int instanceID;
    [FoldoutGroup(groupName: "controller", GroupID = "controller")]
    public Weapon equipWeapon;//装备
    [FoldoutGroup(groupName: "controller", GroupID = "controller")]
    public PropertyController propertyController;//属性是肯定没问题的东西对
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public SkillController skillController;//技能
    [FoldoutGroup(groupName: "controller", GroupID = "controller")]
    public StateController stateController;
    
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public BuffController buffController;
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public MoveController moveController;
    [FoldoutGroup(groupName:"Event",GroupID ="Event")]
    public UnityEvent<Chess> WhenEnterGame;//没有被清除的固定事件,这个是不能清除的
    [HideInInspector]public UnityEvent<Chess> DeathEvent;//这个会被自动清楚
    public Animator animator;
    public SpriteRenderer sprite;
    bool FacingRight = true;
    public bool IfDeath{get;private set;}
    Collider2D col;
    /// <summary>
    /// 初始化所有Controller 只有在生成的时候会调用一次
    /// </summary>
    public void InitChess(){
  
        propertyController.InitController(this);
        equipWeapon.InitController(this);
        stateController.InitController(this);
        skillController.InitController(this);
        moveController.InitController(this);
        buffController.InitController(this);
        col = GetComponent<Collider2D>();
        //animController.InitController(this);
        //audioController.InitController(this);
    }
    /// <summary>
    /// 所有棋子进入场景时就会调用一次
    /// </summary>
    public void WhenChessEnterWar(){
        IfDeath = false;
        propertyController.WhenControllerEnterWar();
        equipWeapon.WhenControllerEnterWar();
        skillController.WhenControllerEnterWar();
        stateController.WhenControllerEnterWar();
        moveController.WhenControllerEnterWar();
        buffController.WhenControllerEnterWar();
        animator.speed = 1;
        WhenEnterGame?.Invoke(this);//主要是可以用来播放声音什么的
        EventController.Instance.TriggerEvent<Chess>(EventName.WhenChessEnterWar.ToString(), this);
    }
    public void Update()
    {
        stateController.StateUpdate();
        //propertyController.Update();
    }
    /// <summary>
    /// 无论如何 死亡时应该清除所有的绑定事件
    /// </summary>
    public virtual void Death()
    {
        if (IfDeath == true) return;
        IfDeath = true;
        propertyController.WhenControllerLeaveWar();
        equipWeapon.WhenControllerLeaveWar();
        stateController.WhenControllerLeaveWar();
        skillController.WhenControllerLeaveWar();
        moveController.WhenControllerLeaveWar();
        buffController.WhenControllerLeaveWar();
        ChessTeamManage.Instance.RecycleChess(this);
        EventController.Instance.TriggerEvent<Chess>(EventName.WhenDeath.ToString(), this);
        DeathEvent?.RemoveAllListeners();
        animator.speed = 1;
    }

    public void Flap(Transform target)
    {
        if ((FacingRight && target .position.x < transform.position.x) || (!FacingRight && target .position.x > transform.position.x))
        {
            transform.Rotate(0, 180, 0);
            FacingRight = !FacingRight;
        }
    }
    public void TakeDamage()
    {
        equipWeapon.TakeDamages();
    }
    public void UseSkill()
    {
        skillController.UseSkill();
    }
    public void RevertPreState()
    {
        stateController.RevertToPreState();
    }
    public void SetCol(bool enable)
    {
        col.enabled = enable;
    }
}