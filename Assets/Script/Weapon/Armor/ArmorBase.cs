using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Armor属于什么类型呢 Armor属于防具对吧 所以我们等等也要改就是了 但是先放着
/// Armor的本质是为Chess承伤，所以他的最主要用处还是加载在WhenGetDamage事件上。
/// <para><b>二类</b>（Buff 不进本体）：在 <c>onSetDamage</c> 回调里请对传入的 <see cref="DamageMessege"/> 置
/// <see cref="DamageMessege.suppressTakeBuffApplication"/>，不要写 <c>takeBuff = null</c>，以免破坏对象池子弹上预置的 <c>takeBuff</c>。</para>
/// WhenLeaveGame 有几种情况：1.使用者阵亡时（比如说铁门被掏屁股）
/// 2.自己消亡(吸收伤害达到上限)
/// 3.被其他方法剥夺（比如磁力菇） 
/// 
/// </summary>
public abstract class ArmorBase : MonoBehaviour,IDamageable
{
    public Chess user;
    public ArmorType type;
    public UnityEvent<ArmorBase> OnArmorBroken;
    private void Awake()
    {
        InitArmor();
    }
    public abstract void InitArmor();
    public abstract void ResetArmor(Chess chess);
    public abstract void GetDamage(DamageMessege dm);
    public virtual void BrokenArmor() { OnArmorBroken?.Invoke(this); }
 
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
