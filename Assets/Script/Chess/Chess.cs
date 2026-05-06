using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Reflection;
using System;
using UnityEngine.Events;
/// <summary>
/// 制作一个单位可能要重写的逻辑
/// 1.weapon:继承weapon 就是攻击逻辑 也有通用的组合方法，但是如果有其他逻辑就要额外写了
/// 2.skill:继承Iskill 就是技能具体要做什么
/// 3.state:继承IState 这个主要是有某个特殊状态的时候才会写的
/// 4.FindTileMethod tileMethod 这个一般是僵尸才有用的 就是有特殊移动方式的时候
/// 5.animatorController 就是有特殊的动画控制效果的时候才需要写的东西
/// 所以说最需要写的其实是Skill和Weapon 其他的就重写的没有那么频繁了
/// </summary>
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
    [FoldoutGroup(groupName: "controller", GroupID = "controller")]
    public AnimatorController animatorController;
    [FoldoutGroup(groupName:"Event",GroupID ="Event")]
    public UnityEvent<Chess> WhenEnterGame;//没有被清除的固定事件,这个是不能清除的  
    /// <summary>仅进入 <see cref="DeathState"/> 时触发，用于亡语；直接 <see cref="Death"/> 不会走此事件。</summary>
    [HideInInspector]public UnityEvent<Chess> DeathEvent;//这个会被自动清除
    /// <summary>棋子离场时触发（含直接 <see cref="Death"/>、铲除等）；通用离场逻辑请挂这里。</summary>
    [HideInInspector]public UnityEvent<Chess> OnRemove;//这个也会被自动清除
    //public Animator animator;
    //public SpriteRenderer sprite;
    bool FacingRight = true;
    /// <summary>是否面向世界 +X（与 <see cref="Flap"/> / <see cref="ForceFlip"/> 一致）。</summary>
    public bool FacingWorldPositiveX => FacingRight;
    public bool IfDeath{get;  set;}
    public bool IfSelectable { get;private set;}
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
        if (animatorController == null) animatorController = GetComponent<AnimatorController>();
        animatorController.InitController(this);
        col = GetComponent<Collider2D>();
        //animController.InitController(this);
        //audioController.InitController(this);
    }
    /// <summary>
    /// 所有棋子进入场景时就会调用一次
    /// </summary>
    /// <param name="triggerEvents">读档恢复时为 false，不触发 WhenPlantChess/WhenChessEnterWar</param>
    public void WhenChessEnterWar(bool triggerEvents = true){
        IfDeath = false;
        propertyController.WhenControllerEnterWar();
        equipWeapon.WhenControllerEnterWar();
        skillController.WhenControllerEnterWar();
        stateController.WhenControllerEnterWar();
        moveController.WhenControllerEnterWar();
        buffController.WhenControllerEnterWar();
        animatorController.WhenControllerEnterWar();
        if (triggerEvents)
        {
            WhenEnterGame?.Invoke(this);
            EventController.Instance.TriggerEvent<Chess>(EventName.WhenChessEnterWar.ToString(), this);
        }
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
        //if (IfDeath == true)
        //{
        //    Debug.Log("这个角色已经死亡");
        //    return;
        //}
        IfDeath = true;
        ResumeSelectable();
        skillController.WhenControllerLeaveWar();
        equipWeapon.WhenControllerLeaveWar();
        buffController.WhenControllerLeaveWar();
        stateController.WhenControllerLeaveWar();
        moveController.WhenControllerLeaveWar();
        propertyController.WhenControllerLeaveWar();
        animatorController?.WhenControllerLeaveWar();
        ChessTeamManage.Instance.RecycleChess(this);
        EventController.Instance.TriggerEvent<Chess>(EventName.WhenDeath.ToString(), this);
        DeathEvent?.RemoveAllListeners();
        OnRemove?.Invoke(this);
        OnRemove.RemoveAllListeners();
        StopAllCoroutines();
       
    }
    

    /// <summary>
    /// 单纯的左右翻转
    /// </summary>
    /// <param name="target"></param>
    public void Flap(Transform target)
    {
        if ((FacingRight && target.position.x < transform.position.x) || (!FacingRight && target.position.x > transform.position.x))
        {
            ForceFlip();
        }
    }
    /// <summary>
    /// 强制 180 度转向，用于恐惧等无需判断目标的场景
    /// </summary>
    public void ForceFlip()
    {
        transform.Rotate(0, 180, 0);
        FacingRight = !FacingRight;
    }

    /// <summary>
    /// 移动时按「当前位置 → 目标」的水平分量决定左右朝向；纯上下移动（水平差过小）不翻面。
    /// </summary>
    /// <param name="deltaToTarget">目标位置 − 当前位置（世界或本地同一空间即可）</param>
    public void UpdateFacingFromHorizontalMove(Vector2 deltaToTarget, float horizontalEpsilon = 0.02f)
    {
        if (Mathf.Abs(deltaToTarget.x) <= horizontalEpsilon)
            return;
        bool wantRight = deltaToTarget.x > 0f;
        if (wantRight == FacingRight)
            return;
        ForceFlip();
    }

    /// <summary>
    /// 与 <see cref="UpdateFacingFromHorizontalMove"/> 一致：<see cref="FacingRight"/> 为 true 表示面向世界 +X；
    /// 不受水平差过小影响，用于出土等必须对齐左右朝向的场景。
    /// </summary>
    public void EnsureFacingWorldX(bool facePositiveWorldX)
    {
        if (facePositiveWorldX == FacingRight) return;
        ForceFlip();
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

    /// <summary>
    /// 进入无法选中状态 本质是将layer切换成无法选中的layer 然后角色无法受到真实伤害以外的伤害
    /// </summary>
    public void UnSelectable()
    {
        if (!IfSelectable)
        {
            IfSelectable = true;
        }
        // 始终设置 layer：CreateChess 会在 WhenChessEnterWar 之前设置 Enemy，需在进场时再次应用
        gameObject.layer = LayerMask.NameToLayer("Unselectable");
    }
    public void ResumeSelectable()
    {
        if (IfSelectable)
        {
            //Debug.Log("可以选中");
            IfSelectable = false;
            
        }
        if(!CompareTag("Untagged"))
            gameObject.layer = LayerMask.NameToLayer(tag);
    }
    public void StopMove() => moveController.ContinuMove();
    public void ContinumMove() => moveController.StopMove();

#if UNITY_EDITOR
    /// <summary>
    /// 转发 <see cref="IGridFindTarget.DrawGizmos"/>。用 <see cref="OnDrawGizmos"/> 而非 <see cref="OnDrawGizmosSelected"/>，
    /// 否则运行时不选中该棋子就不会画线框；线框只在 <b>Scene</b> 视图显示，Game 视图默认看不到。
    /// </summary>
    void OnDrawGizmos()
    {
        if (equipWeapon == null) return;
        if (equipWeapon.weapon is Weapon_Sample ws && ws.findTarget is IGridFindTarget grid)
        {
            if (Passive_Mujica_Oblivionis.TryDrawAttackRangeGizmos(this))
                return;
            grid.DrawGizmos(this);
        }
    }
#endif

}
