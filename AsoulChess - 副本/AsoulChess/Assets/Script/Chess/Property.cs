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

[Serializable]
public class PropertyController
{
    public PropertyCreator creator;
    public Property Data
    {
        get;
        private set;
    }
     
    public Chess chess;
    public class OnGetDamage : UnityEvent<DamageMessege> { }
    public OnGetDamage onGetDamage;
    public OnGetDamage onTakeDamage;
    public void InitProperty(Chess chess)
    {
        Data = creator.GetClone();
        this.chess = chess;
    }
    public void GetDamage(DamageMessege mes)
    {
        if (mes.damageType == DamageType.Physical)
            mes.damage *= (Data.AR / (Data.AR + 100));
        else if (mes.damageType == DamageType.Magic)
            mes.damage *= (Data.MR / (Data.MR + 100));
        else if (mes.damageType == DamageType.Miss)
            mes.damage = 0;
        mes.damage *=(1- Data.extraDefence);  
        if(Data.shiledNum> mes.damage)Data.shiledNum -= mes.damage;
        else
        {
            mes.damage -= Data.shiledNum;
            Data.shiledNum = 0;
            Data.Hp-= mes.damage;
        }
        onGetDamage?.Invoke(mes);
        if (Data.Hp <= 0) chess.Death();
    }
    public void TakeDamage(DamageMessege mes)
    {
        mes.damage=UnityEngine.Random.Range(0, 1) < Data.crit ? mes.damage *Data.critDamage:mes.damage*1;
        mes.damage *= (1+Data.extraDamge);
        //然后这里应该还有一段元素计算，但我就先不管了
        mes.damageTo.propertyController.GetDamage(mes);
        onTakeDamage?.Invoke(mes);
        float heal = mes.damage *= Data.lifeStealing;
    }
    public void Heal(float heal)
    {
        heal*=Data.healRate;
        Data.Hp = Mathf.Min(Data.HpMax, Data.Hp + heal);
    }
    public void ChangeHPMax(float value)=>Data.HpMax+=value;
    public void ChangeAttack(float value)=> Data.attack=Mathf.Max(0,Data.attack+value);
    public void ChangeSpell(float value)=>Data.spell=Mathf.Max(0,Data.spell+value);
    public void ChangeCrit(float value)
    {
        if (value < 0) Data.crit = Mathf.Max(0, Data.crit + value);
        else
        {
            Data.crit += value;
            if(Data.crit > 1)
            {
                ChangeCritDamage( (Data.crit - 1) / 2);
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
    public void ChangeExtraDamage(float value) => Data.extraDamge *= (1+value);
    public void ChangeExtraDefence(float value) => Data.extraDefence *=(1+value);
    public void ChangeShiled(float value) => Data.shiledNum += value;//改变护盾肯定有问题
    public void ChangeLifeSteeling(float value)=>Data.lifeStealing=Mathf.Max(0,Data.lifeStealing+value);
    public void ChangeHealRate(float value)=>Data.healRate=Mathf.Max(0,Data.healRate+value);
    public void ChangeAttackSpeed(float value)
    {
        Data.attackSpeed = Mathf.Min(0.5f, Data.attackSpeed - Data.baseAttackSpeed * value);
    }
    public void ChangeAttackRange(int value)=>Data.attacRange=Mathf.Max(1,Data.attacRange+value);   
    public void ResetProperty(float value)
    {

    }
}
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
    public float lifeStealing=0f;//生命偷取
    public float healRate=1f;//回复增益
    public float attackSpeed = 1f;//攻击速度
    public float baseAttackSpeed=0.25f;
    public int attacRange=1;//攻击距离
    public Property()
    {

    }
    public Property(Property property)
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
        speed=property.speed;
        lifeStealing=property.lifeStealing;
        healRate=property.healRate;
        attackSpeed=property.attackSpeed;
        baseAttackSpeed=property.baseAttackSpeed;
    }
}
public class DamageMessege
{
    public Chess damageFrom;
    public Chess damageTo;
    public float damage;
    public DamageType damageType;
    public ElementType damageElementType;
}
public enum DamageType
{
    Physical,
    Magic,
    Real,
    Miss,
}
public enum ElementType
{
    Windy,
    Aqua,
    Ice,
    Thunder,
    Plant,
    Rock,
    Fire,
    None,
}
