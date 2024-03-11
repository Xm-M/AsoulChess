using System.Diagnostics.Contracts;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelsoftGames.PixelUI;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;
using UnityEngine.Events;

//属性控制类，负责属性的更改修正等
[Serializable]
public class PropertyController:Controller 
{
    public PropertyCreator creator;//这个就是一个基本数据
    public Chess chess;//拥有该属性的棋子
    //public class OnGetDamage : UnityEvent<DamageMessege> { }
    public UnityEvent<DamageMessege> onGetDamage;//造成伤害的事件
    public UnityEvent<DamageMessege> onTakeDamage;//收到伤害的事件
    Property Data;
    public void InitController(Chess chess)
    {
        Data = creator.GetClone();
        this.chess = chess;
    }

    public void WhenControllerEnterWar()
    {
        Data.ResetAllProperty(creator.baseProperty);
        creator.WhenChessEnterWar(chess);
    }
    public void WhenControllerLeaveWar()
    {
        onGetDamage?.RemoveAllListeners();
        onTakeDamage?.RemoveAllListeners();
        creator.WhenChessLeaveWar(chess);
    }
    public void Update()
    {
        if (GameManage.instance.ifGameStart)
        {
            float flash = chess.GetComponent<SpriteRenderer>().material.GetFloat("_FlashAmount");
            if (flash > 0)
            {
                flash -= Time.deltaTime;
                chess.GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", flash);
            }
        }
    }
    //受到伤害的函数
    public void GetDamage(DamageMessege mes)
    {
        if (mes.damageType == DamageType.Physical)
            mes.damage *= (1-(Data.AR / (Data.AR + 100)));
        else if (mes.damageType == DamageType.Magic)
            mes.damage *= (1-(Data.MR / (Data.MR + 100)));
        else if (mes.damageType == DamageType.Miss)
            mes.damage = 0;
        //Debug.Log("当前伤害" + mes.damage);
        mes.damage *= (1 - Data.extraDefence);
        if (Data.shiledNum > mes.damage) Data.shiledNum -= mes.damage;
        else
        {
            mes.damage -= Data.shiledNum;
            Data.shiledNum = 0;
            Data.Hp -= mes.damage;
        }
        onGetDamage?.Invoke(mes);
        chess.GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0.3f);
        //UIManage.instance.CreateDamage(mes);
        //chess.StartCoroutine(ColorChange(1f));
        //Debug.Log("受到" + mes.damage + "伤害");
        //if (Data.Hp <= 0) chess.Death();死亡事件应该由状态机控制
    }
    //造成伤害的函数
    public void TakeDamage(DamageMessege mes)
    {
        mes.damage = UnityEngine.Random.Range(0, 1f) > Data.crit ? mes.damage * Data.critDamage : mes.damage;
        //Debug.Log("造成" + mes.damage + "伤害");
        mes.damage *= (1 + Data.extraDamge);
        //然后这里应该还有一段元素计算，但我就先不管了
        
        mes.damageTo.propertyController.GetDamage(mes);
        onTakeDamage?.Invoke(mes);
        float heal = mes.damage *= Data.lifeStealing;
    }
    //下面的是各种属性的修改函数
    public void Heal(float heal)
    {
        heal *= Data.healRate;
        Data.Hp = Mathf.Min(Data.HpMax, Data.Hp + heal);
    }
    IEnumerator ColorChange(float time)
    {
        float times = 0;
        
        while (times < time)
        {
            times += Time.deltaTime;
            yield return null;
        }
        chess.GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0);
    }   
    public void ChangeHPMax(float value) => Data.HpMax += value;
    public void ChangeAttack(float value) => Data.attack = Mathf.Max(0, Data.attack + value);
    public void ChangeSpell(float value) => Data.spell = Mathf.Max(0, Data.spell + value);
    public void ChangeCrit(float value)
    {
        if (value < 0) Data.crit = Mathf.Max(0, Data.crit + value);
        else
        {
            Data.crit += value;
            if (Data.crit > 1)
            {
                ChangeCritDamage((Data.crit - 1) / 2);
                Data.crit = 1;
            }
        }
    }
    public void ChangeCritDamage(float value)
    {
        Data.critDamage += value;
    }
    public void ChangeAR(float value) => Data.AR += value;
    public void ChangeMR(float value) => Data.MR += value;
    public void ChangeExtraDamage(float value) => Data.extraDamge *= (1 + value);
    public void ChangeExtraDefence(float value) => Data.extraDefence *= (1 + value);
    public void ChangeShiled(float value) => Data.shiledNum += value;//改变护盾肯定有问题
    public void ChangeLifeSteeling(float value) => Data.lifeStealing = Mathf.Max(0, Data.lifeStealing + value);
    public void ChangeHealRate(float value) => Data.healRate = Mathf.Max(0, Data.healRate + value);
    public void ChangeAttackSpeed(float value)
    {
        Data.attackSpeed = Mathf.Min(0.5f, Data.attackSpeed - Data.baseAttackSpeed * value);
    }
    public void ChangeAttackRange(int value) => Data.attacRange = Mathf.Max(1, Data.attacRange + value);
    public void SlowDown(float value)
    {
        Data.slowDownRate = Data.slowDownRate * (1 - value);
    }
    public void ResumeSlowDown(float value)
    {
        Data.slowDownRate = Data.slowDownRate / (1 - value);
    }
    public void Accelerate(float value)
    {
        Data.acceleRate += value;
        Data.acceleRate = Mathf.Max(Data.acceleRate, 1);
    }

    public float GetMoveSpeed()
    {
        return Data.speed * Data.slowDownRate * Data.acceleRate;
    }
    public float GetAttack()
    {
        return Data.attack;
    }
    public float GetAR()
    {
        return Data.AR;
    }
    public float GetMR()
    {
        return Data.MR;
    }
    public float GetSpell()
    {
        return Data.spell;
    }
    public float GetHp()
    {
        return Data.Hp;
    }
    public float GetMaxHp()
    {
        return Data.HpMax;
    }
    public float GetShiledNum()
    {
        return Data.shiledNum;
    }
    public float GetAttackSpeed()
    {
        return Data.attackSpeed;
    }
    public float GetAttackRange()
    {
        return Data.attacRange;
    }
    public int GetPrice(){
        return Data.price;
    }
    public int GetRarity(){
        return Data.rarity;
    }
    public void ChangePrice(float extra){
        Data.price=(int)(Data.price*extra);
        Data.price=Mathf.Max(Data.price,0);
    }
    public void ChangePrice(int extraPrice){
        Data.price+=extraPrice;
        Data.price=Mathf.Max(Data.price,0);
    }
    public int GetSize()
    {
        return Data.Size;
    }
    public float GetHpPerCent()
    {
        return Data.Hp / Data.HpMax;
    }
}
//属性类，每个单位都有自己的属性 
[Serializable]
public class Property
{
    public float attack;//基础攻击
    public float spell;//基础法强
    public float crit;//暴击率
    public float critDamage=1.3f;//暴击伤害
    public float extraDamge=0f;//额外增伤
    public float AR;//护甲
    public float MR;//魔抗
    [Range(0,1)]
    public float extraDefence=0;//额外减伤
    
    public float MP=0f;//蓝条
    public float Hp=500;
    public float HpMax=500f;//最大生命值
    public float shiledNum=0f;//护盾值

    public float speed=2f;//移动速度
    public float slowDownRate = 1;//减速效率
    public float acceleRate = 1;//加速效率
    public float lifeStealing=0f;//生命偷取
    public float healRate=1f;//回复增益
    public float attackSpeed = 1f;//攻击速度
    public float baseAttackSpeed=0.25f;
    public float attacRange=1;//攻击距离

    public int price=50;//价格
    public int rarity=4000;

    public float CD=7.5f;
    public int Size = 1;
    public Property()
    {

    }
    public Property(Property property)
    {
        ResetAllProperty(property);
    }
    public void ResetAllProperty(Property property)
    {
        attack = property.attack;
        spell = property.spell;
        crit = property.crit;
        critDamage = property.critDamage;
        extraDefence = property.extraDefence;
        MP = property.MP;
        Hp = property.Hp;
        HpMax = property.HpMax;
        shiledNum = property.shiledNum;
        speed = property.speed;
        lifeStealing = property.lifeStealing;
        healRate = property.healRate;
        attackSpeed = property.attackSpeed;
        baseAttackSpeed = property.baseAttackSpeed;
        attacRange = property.attacRange;
        price = property.price;
        rarity = property.rarity;
        CD = property.CD;
        Size = property.Size;

    }
}
//造成伤害时传递的信息
[Serializable]
public struct DamageMessege
{
    public Chess damageFrom;//造成伤害的对象
    public Chess damageTo;//受到伤害的对象
    public float damage;//伤害的数值
    public DamageType damageType;//伤害的类型
    public ElementType damageElementType;//元素的类型
}
//一个伤害信息包括伤害类型和元素类型两个信息
//伤害类型
public enum DamageType
{
    Physical,//物理伤害
    Magic,//魔法伤害
    Real,//真实伤害
    Miss,//未命中
}
//元素类型 
public enum ElementType
{
    None=0,//无
    Windy=1,//风
    Aqua=2,//水
    Ice=3,//冰
    Thunder=4,//雷电
    Plant=5,//草
    Rock=6,//岩
    Fire=7,//火
    
}
public class ShiledNum
{
    public float shiled;
    public Chess shiledFrom;
    public virtual float GetDamage(DamageMessege mes)
    {
        if (shiled > mes.damage)
        {
            shiled -= mes.damage;
            return 0;
        }
        else
        {
            shiled = 0;
            return mes.damage - shiled;
        }
    }
    
}
