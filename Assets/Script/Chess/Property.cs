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
    [HideInInspector]protected Chess chess;//拥有该属性的棋子
    //这几个都是只能在战斗中添加事件 因为结束的时候会被清除 所以不能在游戏开始前在Inspector面板添加事件
    [HideInInspector] public UnityEvent<DamageMessege> onGetDamage;//受到伤害的事件
    [HideInInspector] public UnityEvent<DamageMessege> onSetDamage;//受到伤害前的事件(主要是增伤或者缓和，还有护甲抵挡等问题)
    [HideInInspector] public UnityEvent<DamageMessege> onTakeDamage;//造成伤害的事件
    Property Data;
    //bool freezy = false;
    public void InitController(Chess chess)
    {
        Data = creator.GetClone();
        this.chess = chess;
    }

    public void WhenControllerEnterWar()
    {
        Data.ResetAllProperty(creator.baseProperty);
        //freezy = false;
    }
    public void WhenControllerLeaveWar()
    {
        onGetDamage?.RemoveAllListeners();
        onTakeDamage?.RemoveAllListeners();
        onSetDamage?.RemoveAllListeners();
    }
     
    //受到伤害的函数
    public void GetDamage(DamageMessege mes)
    {
        onSetDamage?.Invoke(mes);
        //Debug.Log("基础伤害" + mes.damage);
        if (mes.takeBuff != null)
        {
            chess.buffController.AddBuff(mes.takeBuff);
        }
        if (mes.damageType != DamageType.Real)
        {
            //Debug.Log("倍率" + (1 - (Data.AR / (Data.AR + 100))));
            mes.damage *= (1 - (Data.AR / (Data.AR + 100)));
        }
        else if (mes.damageType == DamageType.Miss)
            mes.damage = 0;
        else if(mes.damageType == DamageType.Heal)
        {
            Heal(mes.damage);
            return;
        }
        //Debug.Log("当前伤害" + mes.damage);
        mes.damage *= (1 - Data.extraDefence);
        float n = UnityEngine.Random.Range(0, 1f);
        if (n < GetDodge())
        {
            mes.damage = 0;
            UIManage.GetView<DamagePanel>().ShowMiss(mes);
        }

        if (mes.damage > 0)
        {
            Data.Hp -= mes.damage;
            UIManage.GetView<DamagePanel>().ShowDamageMes(mes);
            //Debug.Log(creator.name + "受到了" + mes.damage);
            //chess.animator.SetFloat();
            onGetDamage?.Invoke(mes);
            chess.animatorController.OnGetDamage(mes);
        }
        
        //chess.sprite?.material.SetFloat("_FlashAmount", Time.time);
        //UIManage.instance.CreateDamage(mes);
        //chess.StartCoroutine(ColorChange(1f));
    }
    //造成伤害的函数 
    public void TakeDamage(DamageMessege mes)
    {
        if (mes.damageTo != null && !mes.damageTo.IfDeath)
        {
            if (mes.damageType != DamageType.Heal)
            {
                //暴击伤害
                float n = UnityEngine.Random.Range(0, 1f);
                if (n < GetCrit())
                {
                    mes.damage = mes.damage * GetCritDamage();
                    mes.ifCrit = true;
                }
                else
                {
                    mes.ifCrit = false;
                }
                //mes.damage =  n< GetCrit() ? mes.damage * GetCritDamage() : mes.damage;

                //增伤
                mes.damage *= (1 + GetExtraDamage());
                //if(GetExtraDamage() > 0)Debug.Log(mes.damage+" 增伤了");
                mes.damageTo.propertyController.GetDamage(mes);
                onTakeDamage?.Invoke(mes);
                float heal = mes.damage *= Data.lifeStealing;
                if(heal > 0)
                    Heal(heal);
            }
            else
            {
                mes.damageTo.propertyController.GetDamage(mes);
            }
        }
    }
    //下面的是各种属性的修改函数
    public void Heal(float heal)
    {
        heal *= Data.healRate;
        Data.Hp = Mathf.Min(Data.HpMax, Data.Hp + heal);
        UIManage.GetView<DamagePanel>().ShowHeal(heal, chess);
    }
    public void ChangeHp(float value) => Data.Hp = value;
    public void ChangeHPMax(float value)
    {
        Data.HpMax += value;
        if (value > 0)
        {
            Data.Hp += value;
        }
    }
    public void ChangeAttack(float value)
    {
        Data.attackRate += value;        //if(Data.attackRate <0)Data.attackRate = 0;
        Data.attack = creator.baseProperty.attack * Data.attackRate;
    }
    public void ChangeCrit(float value)
    {
        if (value < 0) Data.crit = Mathf.Max(0, Data.crit + value);
        else
        {
            Data.crit += value;
           
        }
    }
    public void ChangeDodgeRate(float value)
    {
        Data.dodgeRate += value;
    }
    public void ChangeCritDamage(float value)
    {
        Data.critDamage += value;
    }
    public void ChangeAR(float value) => Data.AR += value;
    public void ChangeExtraDamage(float value) => Data.extraDamge += value;
    public void ChangeExtraDefence(float value) => Data.extraDefence +=  value;
    //public void ChangeShiled(float value) => Data.shiledNum += value;//改变护盾肯定有问题
    public void ChangeLifeSteeling(float value) => Data.lifeStealing = Mathf.Max(0, Data.lifeStealing + value);
    public void ChangeHealRate(float value) => Data.healRate = Mathf.Max(0, Data.healRate + value);
    public void ChangeAcceleRate(float value)
    {
        Data.acceleRated += value;
        //chess.animator.speed = Data.acceleRated;
        chess.animatorController.ChangeSpeed(Data.acceleRated);
        //if (!freezy)
            
    }
    public void ChangeDizznessTime(float value)
    {
        
        if (chess.stateController.currentState.state.stateName == StateName.DizzyState)
        {
            Data.dizzinessTime = Mathf.Max(Data.dizzinessTime, value);
            //Debug.Log("重置眩晕状态");
        }
        else
        {
            Data.dizzinessTime = value;
            if(GetDizznessTime()>0)
                chess.stateController.ChangeState(StateName.DizzyState);
            //Debug.Log("进入眩晕状态");
        }
    }
    public float GetDizznessTime()
    {
        return Data.dizzinessTime*(1-Data.tenacity);
    }
    public void ResetDizznessTime()
    {
        Data.dizzinessTime = 0;
    }
    public void ChangeAttackRange(float value) => Data.attackRange = Data.attackRange + value;
    public void SetAtttackRange(float value) => Data.attackRange = value;
    public float GetMoveSpeed()
    {
        return Data.speed * Mathf.Max(0, Data.acceleRated);
    }
    public float GetAttack()
    {
        return Mathf.Max(Data.attack,0);
    }
    public float GetExtraDamage()
    {
        return  Data.extraDamge;
    }
    public float GetDodge()
    {
        return Data.dodgeRate;
    }
    public float GetExtraDefence()
    {
        return Data.extraDefence;
    }
    /// <summary>
    /// 本来是有一个技能急速的设定的 现在统一和攻速绑定
    /// </summary>
    /// <param name="coldDown"></param>
    /// <returns></returns>
    public float GetColdDown(float coldDown)
    {

        return coldDown/(GetAccelerate());
    }
    public float GetAR()
    {
        return Data.AR;
    }
    public float GetHp()
    {
        return Data.Hp;
    }
    public float GetMaxHp()
    {
        return Data.HpMax;
    }
    //public float GetShiledNum()
    //{
    //    return Data.shiledNum;
    //}
    public float GetCrit()
    {
        return Data.crit;
    }
    public float GetCritDamage()
    {
        float crit = GetCrit();
        float critedamage = Data.critDamage;
        if (crit > 1)
        {
            //ChangeCritDamage((Data.crit - 1) / 2);
            critedamage += (crit - 1) / 2;
        }
        return critedamage;
    }
    public float GetAccelerate()
    {
        return Data.acceleRated;
    }

    public float GetAttackRange()
    {
        return Data.attackRange;
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
    public void ChangeSize(int value)
    {
        Data.Size += value;
        Data.Size = Mathf.Max(Data.Size, 1);
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
    public float attackRate;//攻击倍率

    public float crit;//暴击率
    public float critDamage=1.3f;//暴击伤害
    public float dodgeRate;//闪避概率

    public float extraDamge=0f;//额外增伤
    public float AR;//护甲

    [Range(0,1)]
    public float extraDefence=0;//额外减伤
    
    
    public float Hp=300;
    public float HpMax=300f;//最大生命值
    //public float shiledNum=0f;//护盾值


    public float lifeStealing=0f;//生命偷取
    public float healRate=1f;//回复增益

    public float speed = 0f;//移动速度
    //public float attackSpeed = 1f;//攻击速度
    public float acceleRated=1f;//攻速移速的加成都取决于这个属性

    [HideInInspector] public float dizzinessTime;//眩晕时间
    [LabelText("韧性")]
    public float tenacity;//韧性


    public float attackRange=1;//攻击距离

    public int price=50;//价格
    public int rarity=4000;//稀有度
    public int waveLimit = 1;//波数限制

    public float CD=7.5f;
    public int Size = 1;//体型
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
        attackRate = 1;
        crit = property.crit;
        critDamage = property.critDamage;
        dodgeRate = property.dodgeRate;

        extraDefence = property.extraDefence;
        //spellHaste = property.spellHaste;
        Hp = property.Hp;
        HpMax = property.HpMax;

        tenacity =property.tenacity;
        dizzinessTime = 0;
        speed = property.speed;
        lifeStealing = property.lifeStealing;
        healRate = property.healRate;
        //attackSpeed = property.attackSpeed;
        acceleRated = 1;
        
        attackRange = property.attackRange;
        price = property.price;
        rarity = property.rarity;
        waveLimit = property.waveLimit;
        CD = property.CD;
        Size = property.Size;

    }
}
//造成伤害时传递的信息 
[Serializable]
public class DamageMessege
{
    public Chess damageFrom;//造成伤害的对象
    public Chess damageTo;//受到伤害的对象
    public float damage;//伤害的数值
    public DamageType damageType;//伤害的类型
    public ElementType damageElementType;//元素的类型
    public bool ifCrit;//是否暴击
    [SerializeReference]
    public Buff takeBuff;
    public DamageMessege()
    {

    }
    
    public DamageMessege(Chess user,Chess target,float damage,DamageType damageType=DamageType.Physical,ElementType elementType = ElementType.None)
    {
        damageFrom = user;
        damageTo = target;
        this.damage = damage;
        this.damageType = damageType;
        this.damageElementType = elementType;
    }
}
//一个伤害信息包括伤害类型和元素类型两个信息
//伤害类型
public enum DamageType
{
    Physical,//物理伤害
    Magic,//与其说是魔法伤害，不如说是技能伤害
    Real,//真实伤害
    Miss,//未命中
    Heal,//治疗
}
//元素类型,其实是额外信息
[Flags]
public enum ElementType
{
    None=0,//无
    CloseAttack=1<<0,//近战
    Bullet=1<<1,//子弹 这两种是最基本的伤害类型了
    AOE=1<<2,//AOE 范围伤害
    Puncture=1<<3,//穿刺 仙人掌是Bullet Puncture;地刺是CloseAttack Puncture
    Explode=1<<4,//爆炸
    Fire=1<<5,//火焰伤害
    Cutting=1<<6,//切割伤害
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
