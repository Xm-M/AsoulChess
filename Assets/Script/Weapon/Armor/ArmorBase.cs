using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Armor的本质是为Chess承伤，所以他的最主要用处还是加载在WhenGetDamage事件上
/// WhenLeaveGame 有几种情况：1.使用者阵亡时（比如说铁门被掏屁股）
/// 2.自己消亡(吸收伤害达到上限)
/// 3.被其他方法剥夺（比如磁力菇） 
/// </summary>
public abstract class ArmorBase : MonoBehaviour,IDamageable
{
    public Chess user;
    public ArmorType type;
    private void Awake()
    {
        InitArmor();
    }
    public abstract void InitArmor();
    public abstract void ResetArmor();
    public abstract void GetDamage(DamageMessege dm);
    public abstract void BrokenArmor();
    
}
public interface IDamageable
{
    public void GetDamage(DamageMessege dm);
}
[Flags]
public enum ArmorType
{
    None=0,
    Head=1<<0,//是否头部 区分一类二类
    Invincible=1<<1,//是否无敌
    Metal=1<<2,//是否金属
    PassDamage=1<<3,//是否传递伤害
}
