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
    public PropertyController propertyController;//属性是肯定没问题的东西对吧
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public Weapon equipWeapon;
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public StateController stateController;
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public SkillController skillController;
    //[FoldoutGroup(groupName:"controller",GroupID ="controller")]
    //public ItemController itemController;
    
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public BuffController buffController;
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public MoveController moveController;
    [FoldoutGroup(groupName:"Event",GroupID ="Event")]
    public UnityEvent<Chess> EnterWarEvent, DeathEvent,HitEvent;
    public Animator animator;
    bool FacingRight = true;
    public bool IfDeath{get;private set;}
    /// <summary>
    /// 初始化所有Controller 只有在生成的时候会调用一次
    /// </summary>
    public void InitChess(){
        if (animator == null) { animator = GetComponent<Animator>(); }
        propertyController.InitController(this);
        equipWeapon.InitController(this);
        stateController.InitController(this);
        skillController.InitController(this);
        moveController.InitController(this);
        buffController.InitController(this);
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
        EnterWarEvent?.Invoke(this);
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
        DeathEvent?.Invoke(this);
        propertyController.WhenControllerLeaveWar();
        equipWeapon.WhenControllerLeaveWar();
        stateController.WhenControllerLeaveWar();
        skillController.WhenControllerLeaveWar();
        moveController.WhenControllerLeaveWar();
        buffController.WhenControllerLeaveWar();
        GameManage.instance.RecycleChess(this);
        EventController.Instance.TriggerEvent<Chess>(EventName.WhenDeath.ToString(), this);
        EnterWarEvent?.RemoveAllListeners();
        DeathEvent?.RemoveAllListeners();
         
    }
    public virtual void RemoveChess()
    {
        if (IfDeath == true) return;
        IfDeath = true;
        propertyController.WhenControllerLeaveWar();
        equipWeapon.WhenControllerLeaveWar();
        stateController.WhenControllerLeaveWar();
        skillController.WhenControllerLeaveWar();
        moveController.WhenControllerLeaveWar();
        buffController.WhenControllerLeaveWar();
        GameManage.instance.RecycleChess(this);
        EnterWarEvent?.RemoveAllListeners();
        DeathEvent?.RemoveAllListeners();
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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (HitEvent != null)
        {
            if (!collision.CompareTag(tag) && collision.GetComponent<Chess>())
            {
                HitEvent.Invoke(collision.GetComponent<Chess>());
            }
        }
    }
}