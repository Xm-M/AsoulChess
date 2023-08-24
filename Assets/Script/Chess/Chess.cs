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
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public ItemController itemController;
    
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public BuffController buffController;
    [FoldoutGroup(groupName:"controller",GroupID ="controller")]
    public MoveController moveController;
   
    [FoldoutGroup(groupName:"Event",GroupID ="Event")]
    public UnityEvent<Chess> EnterWarEvent, DeathEvent,HitEvent;
    public Animator animator;
    bool FacingRight = true;
    //public AnimationCurve curve;
    public bool IfDeath{get;private set;}
    public void InitChess(){
        if (animator == null) { animator = GetComponent<Animator>(); }
        propertyController.InitController(this);
        equipWeapon.InitController(this);
        stateController.InitController(this);
        skillController.InitController(this);
        moveController.InitController(this);
        buffController.InitController(this);
    }
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
        propertyController.Update();
    }
    /// <summary>
    /// 无论如何 死亡时应该清楚所有的绑定事件
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
        ChessFactory.instance.RecycleChess(this);
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
        ChessFactory.instance.RecycleChess(this);
        EnterWarEvent?.RemoveAllListeners();
        DeathEvent?.RemoveAllListeners();
    }
    public void Flap()
    {
        if (equipWeapon.target) 
        if ((FacingRight && equipWeapon.target.transform.position.x < transform.position.x) || (!FacingRight &&equipWeapon.target.transform.position.x > transform.position.x))
        {
                OverTurn();
        }
    }
    public void OverTurn()
    {
        transform.Rotate(0, 180, 0);
        FacingRight = !FacingRight;
    }
    private void OnMouseEnter()
    {
        if (CompareTag("Player"))
        {
            PlantsShop.instance.selectChess = this;
        }
    }
    private void OnMouseExit()
    {
        if (CompareTag("Player"))
        {
            PlantsShop.instance.selectChess = null;
        }
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






public class ItemController{
    public List<Item> items;
    public Chess chess;
    public ItemController(Chess chess){
        this.chess=chess;
        items=new List<Item>();
    }

    //装备武器时触发对应的效果
    public void AddItem(Item item){
        items.Add(item);
        item.SetUser(chess);
        item.ItemEffect();
        UIManage.instance.ShowChessMessage(chess);
    }
    //卖出棋子后要归还所有武器
    public void SellChess(){
        //归还所有的武器
        foreach(var item in items)
            item.ResetItemAll();
    }
    public void ResetAllItem(){
        //重置所有的装备
        foreach(var item in items)
            item.ResetItem();
    }
}