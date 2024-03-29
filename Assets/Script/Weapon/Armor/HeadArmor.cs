using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadArmor :ArmorBase
{
    public float armorMax;
    public Animator animator;
    public string animName;
    float armorCurrent;
    float brokenPercent;
    /// <summary>
    /// 这个是初始化，只会被调用一次
    /// </summary>
    public override void InitArmor()
    {
        armorCurrent = armorMax;
        user.WhenEnterGame.AddListener((Chess) => ResetArmor());
    }
    public override void GetDamage(DamageMessege dm)
    {
        if (armorCurrent >= dm.damage)
        {
            Debug.Log("防具抵消");
            armorCurrent -= dm.damage;
            dm.damage = 0;
        }
        else
        {
            Debug.Log("防具溢出");
            dm.damage -= armorCurrent;
            armorCurrent = 0;
            BrokenArmor();
        }
        brokenPercent = armorCurrent / armorMax;
        animator?.SetFloat(animName, brokenPercent);
    }
    /// <summary>
    ///  就是重置函数
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void ResetArmor()
    {
        tag = user.tag;
        user.propertyController.onSetDamage.AddListener(GetDamage);
        armorCurrent = armorMax;
    }
    /// <summary>
    ///   是防具损坏函数
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void BrokenArmor()
    {
        //如果提前死亡的话这个也是会被清除的 所以可以不管
        Debug.Log("防具破坏");
        user.propertyController.onSetDamage.RemoveListener(GetDamage);
    }
}
