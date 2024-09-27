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
    public AttackController equipWeapon;//攻击管理
    [FoldoutGroup(groupName: "controller", GroupID = "controller")]
    public PropertyController propertyController;//属性是肯定没问题的东西对
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public SkillController skillController;//技能   
    [FoldoutGroup(groupName: "controller", GroupID = "controller")]
    public StateController stateController;//状态管理 这个比较重要
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public BuffController buffController;
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public MoveController moveController;


    [FoldoutGroup(groupName:"Event",GroupID ="Event")]
    public UnityEvent<Chess> WhenEnterGame;//没有被清除的固定事件,这个是不能清除的
    [HideInInspector]public UnityEvent<Chess> DeathEvent;//这个会被自动清除
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
        sprite.color=Color.white;
        animator.speed = 1;
        WhenEnterGame?.Invoke(this);//主要是可以用来播放声音什么的
        EventController.Instance.TriggerEvent<Chess>(EventName.WhenChessEnterWar.ToString(), this);
    }
    public void Update()
    {
        stateController.StateUpdate();
    }
    /// <summary>
    /// 无论如何 死亡时应该清除所有的绑定事件
    /// </summary>
    public virtual void Death()
    {
        if (IfDeath == true) return;
        IfDeath = true;
        skillController.WhenControllerLeaveWar();
        equipWeapon.WhenControllerLeaveWar();
        buffController.WhenControllerLeaveWar();
        stateController.WhenControllerLeaveWar();
        moveController.WhenControllerLeaveWar();
        propertyController.WhenControllerLeaveWar();
        ChessTeamManage.Instance.RecycleChess(this);
        EventController.Instance.TriggerEvent<Chess>(EventName.WhenDeath.ToString(), this);
        DeathEvent?.RemoveAllListeners();
        animator.speed = 1;
    }

    /// <summary>
    /// 单纯的左右翻转
    /// </summary>
    /// <param name="target"></param>
    public void Flap(Transform target)
    {
        if ((FacingRight && target .position.x < transform.position.x) || (!FacingRight && target .position.x > transform.position.x))
        {
            transform.Rotate(0, 180, 0);
            FacingRight = !FacingRight;
        }
    }
    /// <summary>
    /// 这个函数应该是用在估计动画触发效果的时候
    /// </summary>
    public void TakeDamage()
    {
        equipWeapon.TakeDamages();
    }
    /// <summary>
    /// 这个函数是用在技能动画触发技能效果的时候
    /// </summary>
    public void UseSkill()
    {
        skillController.UseSkill();
    }
    /// <summary>
    /// 这个估计是...我也不知道
    /// </summary>
    public void RevertPreState()
    {
        stateController.RevertToPreState();
    }
    /// <summary>
    /// 这个故事是死亡动画调用的
    /// </summary>
    /// <param name="enable"></param>
    public void SetCol(bool enable)
    {
        col.enabled = enable;
    }
}