using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class Item
{
    public ItemNames ItemName;
    public Sprite ItemSprite;
    public Chess user;
    public int instanceID;
    public virtual void SetUser(Chess user){
        this.user=user;
    }
    public virtual void ResetItem(){

    }
    public virtual void ItemEffect(){

    }
    public virtual void ResetItemAll(){
        
    }
}
public class FireSword:Item{
    public GameObject test;
    public Buff FireBuff;
    public override void ItemEffect()
    {
        base.ItemEffect();
        FireBuff=GameManage.instance.buffManage.GetBuff("FireBuff");
        EventController.Instance.AddListener<DamageMessege>(user.instanceID+EventName.WhenAttackTakeDamages.ToString(),
        FireAttack);
    }
    public void FireAttack(DamageMessege messege){
        messege.damageTo.buffController.AddBuff(messege.damageFrom,FireBuff);
    }
}
public enum ItemNames{
    FireSword=0,
}